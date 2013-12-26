#pragma warning disable 659

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    /// <summary>
    /// Squelch locals and expression blocks
    /// </summary>
    /// <remarks>
    /// The squelch phase removes unnecessary temporaries. Temporaries are unnecessary when
    /// either it's a temporary that is only read once, or when temporary a
    /// temporary is just an alias of a normal local.
    /// 
    /// The squelch phase executes in two steps. The first steps gathers information
    /// about all temporaries. The rewrite phase rewrites the replacing the
    /// temporaries that can be removed by what they are written to.
    /// </remarks>
    internal static class SquelchPhase
    {
        public static BoundProgram Perform(BoundProgram node)
        {
            return node.Update(Perform(node.Body));
        }

        public static BoundFunction Perform(BoundFunction node)
        {
            return node.Update(
                node.Name,
                node.Parameters,
                Perform(node.Body),
                node.Location
            );
        }

        private static BoundBody Perform(BoundBody node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var statistics = new Dictionary<BoundTemporary, Statistics>();

            new Gatherer(statistics).Visit(node);

            Validate(statistics);

            node = (BoundBody)new Rewriter(statistics).Visit(node);

            return node;
        }

        [Conditional("DEBUG")]
        private static void Validate(Dictionary<BoundTemporary, Statistics> statistics)
        {
            // This method performs a sanity check on the gathered statistics.

            foreach (var item in statistics)
            {
                // All temporaries should be written to.
                Debug.Assert(item.Value.WriteState != WriteType.Not);

                // We should at least have a single read on a temporary.
                // Debug.Assert(item.Value.Reads > 0);
            }
        }

        private class Gatherer : BoundTreeWalker
        {
            private readonly Dictionary<BoundTemporary, Statistics> _statistics;
            private readonly Dictionary<IBoundWritable, int> _variableWriteCount = new Dictionary<IBoundWritable, int>();

            public Gatherer(Dictionary<BoundTemporary, Statistics> statistics)
            {
                _statistics = statistics;
            }

            public override void VisitGetVariable(BoundGetVariable node)
            {
                base.VisitGetVariable(node);

                var temporary = node.Variable as BoundTemporary;
                if (temporary != null)
                    MarkRead(temporary);
            }

            private void MarkRead(BoundTemporary temporary)
            {
                // Check if the local is still valid. If this temporary is
                // only written to from a local, it is eligible for replacement.
                // However, we can only do this when the local hasn't been
                // changed before we read the temporary. This is what is
                // checked below.

                var statistics = GetStatistic(temporary);
                if (statistics.WriteState == WriteType.Local && statistics.Value != null)
                {
                    var getVariable = statistics.Value.Expression as BoundGetVariable;
                    if (getVariable != null)
                    {
                        var local = getVariable.Variable as BoundLocal;
                        if (local != null)
                        {
                            int writeCount;
                            _variableWriteCount.TryGetValue(local, out writeCount);

                            if (writeCount != statistics.Value.WriteCount)
                                statistics.WriteState = WriteType.DoNotRemove;
                        }
                    }
                }


                statistics.Reads++;
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                base.VisitSetVariable(node);

                // Bump the write count of the target.

                int writeCount;
                _variableWriteCount.TryGetValue(node.Variable, out writeCount);
                writeCount++;
                _variableWriteCount[node.Variable] = writeCount;

                // Update the statistic if the target is a temporary.

                var temporary = node.Variable as BoundTemporary;
                if (temporary != null)
                {
                    var statistic = GetStatistic(temporary);

                    if (statistic.WriteState == WriteType.Not)
                    {
                        // If we're reading from a local, we need to get the
                        // current write count for the local to check whether
                        // the value the temporary is receiving is going to be
                        // killed.
                        int localWriteCount = -1;
                        var getVariable = node.Value as BoundGetVariable;
                        BoundLocal local = null;
                        if (getVariable != null)
                        {
                            local = getVariable.Variable as BoundLocal;
                            if (local != null)
                                _variableWriteCount.TryGetValue(local, out localWriteCount);
                        }

                        statistic.Value = new ReadState(node.Value, localWriteCount);

                        statistic.WriteState = local != null
                            ? WriteType.Local
                            : WriteType.Expression;
                    }
                    else if (statistic.WriteState != WriteType.Local)
                    {
                        statistic.WriteState = WriteType.DoNotRemove;
                    }
                }
            }

            public override void VisitExpressionBlock(BoundExpressionBlock node)
            {
                base.VisitExpressionBlock(node);

                // The BuildingVisitor only allows temporaries to be put into
                // the result of the expression block. In the Squelch
                // phase, we're relaxing this requirement, but we can
                // safely cast here.

                MarkRead((BoundTemporary)node.Result);
            }

            public override void VisitCatch(BoundCatch node)
            {
                var temporary = node.Target as BoundTemporary;
                if (temporary != null)
                {
                    var statistic = GetStatistic(temporary);
                    if (statistic.WriteState != WriteType.Local)
                        statistic.WriteState = WriteType.DoNotRemove;
                }

                base.VisitCatch(node);
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                var temporary = node.Target as BoundTemporary;
                if (temporary != null)
                {
                    var statistic = GetStatistic(temporary);
                    if (statistic.WriteState != WriteType.Local)
                        statistic.WriteState = WriteType.DoNotRemove;
                }

                base.VisitForEachIn(node);
            }

            public override void VisitSwitch(BoundSwitch node)
            {
                MarkRead(node.Temporary);

                // Mark the write as other to prevent removing of the temporary.
                // The whole point of the temporary is that we require it for the
                // switch construct, so it cannot be removed.
                GetStatistic(node.Temporary).WriteState = WriteType.DoNotRemove;

                base.VisitSwitch(node);
            }

            private Statistics GetStatistic(BoundTemporary temporary)
            {
                Statistics statistics;
                if (!_statistics.TryGetValue(temporary, out statistics))
                {
                    statistics = new Statistics();
                    _statistics.Add(temporary, statistics);
                }

                return statistics;
            }
        }

        private class Rewriter : BoundTreeRewriter
        {
            private readonly Dictionary<BoundTemporary, Statistics> _statistics;

            public Rewriter(Dictionary<BoundTemporary, Statistics> statistics)
            {
                _statistics = statistics;
            }

            public override BoundNode VisitExpressionBlock(BoundExpressionBlock node)
            {
                // Check for expression squelching. We can only squelch an
                // expression if it's the only thing in the expression block.
                // If this is the case, just skip over the whole algorithm and
                // return the expression.
                var resultStatistic = _statistics[(BoundTemporary)node.Result];
                if (node.Body.Nodes.Count == 1 && resultStatistic.WriteState == WriteType.Expression)
                    return resultStatistic.Value.Expression.Accept(this);

                // If we can't squelch the whole expression, we also cannot
                // squelch the result temporary if it isn't a local.
                // We for the write state to other to indicate this.
                if (resultStatistic.ShouldRemove && resultStatistic.WriteState != WriteType.Local)
                    resultStatistic.WriteState = WriteType.DoNotRemove;

                // Rewrite the body.
                var body = (BoundBlock)Visit(node.Body);

                // Check whether the result must be updated.
                IBoundReadable result;

                if (resultStatistic.ShouldReplace)
                {
                    // We should only get locals here, because the result counts
                    // as a read and there should also be a read in the expression
                    // block.
                    Debug.Assert(resultStatistic.WriteState == WriteType.Local);

                    result = ((BoundGetVariable)resultStatistic.Value.Expression).Variable;
                }
                else
                {
                    result = node.Result;
                }

                return node.Update(result, body);
            }

            public override BoundNode VisitBlock(BoundBlock node)
            {
                // First check whether we have work.

                bool haveWork = false;

                foreach (var temporary in node.Temporaries)
                {
                    if (_statistics[temporary].ShouldReplace)
                    {
                        haveWork = true;
                        break;
                    }
                }

                // If we don't have work, just cascade.
                if (!haveWork)
                    return base.VisitBlock(node);

                // Create a new list of temporaries with the variables to be
                // squelched removed.
                var newTemporaries = new ReadOnlyArray<BoundTemporary>.Builder();
                foreach (var temporary in node.Temporaries)
                {
                    if (_statistics[temporary].ShouldReplace)
                        temporary.Type.MarkUnused();
                    else
                        newTemporaries.Add(temporary);
                }
                var temporaries = newTemporaries.ToReadOnly();

                // Rebuild the nodes with the new rules applied.
                var nodes = new ReadOnlyArray<BoundStatement>.Builder();

                foreach (var statement in node.Nodes)
                {
                    var setVariable = statement as BoundSetVariable;
                    if (setVariable != null)
                    {
                        setVariable = (BoundSetVariable)Visit(setVariable);

                        // If the set variable reduced to an assignment to itself,
                        // remove the set variable. This happens when the variable
                        // of the set variable is replaced.

                        var getVariable = setVariable.Value as BoundGetVariable;
                        if (getVariable != null && setVariable.Variable == getVariable.Variable)
                            continue;

                        // If we're going to squelch this local, remove the
                        // set variable for it.

                        var temporary = setVariable.Variable as BoundTemporary;
                        if (temporary != null && _statistics[temporary].ShouldRemove)
                            continue;
                    }

                    nodes.Add((BoundStatement)Visit(statement));
                }

                // Return the new block.

                return new BoundBlock(temporaries, nodes.ToReadOnly(), node.Location);
            }

            public override BoundNode VisitSetVariable(BoundSetVariable node)
            {
                var variable = node.Variable;

                var temporary = variable as BoundTemporary;
                if (temporary != null)
                {
                    var statistic = _statistics[temporary];
                    if (statistic.WriteState == WriteType.Local)
                        variable = (IBoundWritable)((BoundGetVariable)statistic.Value.Expression).Variable;
                }

                return node.Update(
                    variable,
                    (BoundExpression)Visit(node.Value),
                    node.Location
                );
            }

            public override BoundNode VisitGetVariable(BoundGetVariable node)
            {
                // Return the associated expression of the temporary if we're
                // going to squelch the temporary.

                var temporary = node.Variable as BoundTemporary;
                if (temporary != null)
                {
                    var statistic = _statistics[temporary];
                    if (statistic.ShouldReplace)
                        return statistic.Value.Expression.Accept(this);
                }

                return base.VisitGetVariable(node);
            }

            public override BoundNode VisitCreateFunction(BoundCreateFunction node)
            {
                // It's the responsibility of the phases to apply the actions
                // to nested functions.

                return node.Update(
                    node.Function.Update(
                        node.Function.Name,
                        node.Function.Parameters,
                        Perform(node.Function.Body),
                        node.Function.Location
                    )
                );
            }
        }

        private class Statistics
        {
            public WriteType WriteState { get; set; }
            public ReadState Value { get; set; }
            public int Reads { get; set; }

            public bool ShouldRemove
            {
                get
                {
                    // We can only remove temporaries that haven't been
                    // prevented from being removed and that have at most
                    // one read.

                    return WriteState != WriteType.DoNotRemove && Reads <= 1;
                }
            }

            public bool ShouldReplace
            {
                get { return ShouldRemove || WriteState == WriteType.Local; }
            }
        }

        private class ReadState
        {
            public int WriteCount { get; private set; }
            public BoundExpression Expression { get; private set; }

            public ReadState(BoundExpression expression, int writeCount)
            {
                Expression = expression;
                WriteCount = writeCount;
            }

            public override bool Equals(object obj)
            {
                var other = obj as ReadState;
                return
                    other != null &&
                    Expression == other.Expression &&
                    WriteCount == other.WriteCount;
            }
        }

        private enum WriteType
        {
            Not,
            Local,
            Expression,
            DoNotRemove
        }
    }
}

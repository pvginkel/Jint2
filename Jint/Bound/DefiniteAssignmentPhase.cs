using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class DefiniteAssignmentPhase
    {
        public static IList<BoundExpression> Perform(BoundProgram node)
        {
            return Perform(node.Body, null);
        }

        public static void Perform(BoundFunction node)
        {
            Perform(node.Body, null);
        }

        private static BoundExpression[] Perform(BoundBody node, BoundTypeManager.DefiniteAssignmentMarker.Branch parentBranch)
        {
            var marker = new Marker(node.TypeManager, parentBranch);

            marker.Visit(node);

            return marker.ResultExpressions;
        }

        private class Marker : BoundTreeWalker
        {
            private readonly BoundTypeManager _typeManager;
            private readonly BoundTypeManager.DefiniteAssignmentMarker.Branch _parentBranch;
            private readonly List<Block> _blocks = new List<Block>();
            private readonly Dictionary<BoundNode, string> _labels = new Dictionary<BoundNode, string>();
            private BoundTypeManager.DefiniteAssignmentMarker.Branch _branch;

            public BoundExpression[] ResultExpressions { get; private set; }

            public Marker(BoundTypeManager typeManager, BoundTypeManager.DefiniteAssignmentMarker.Branch parentBranch)
            {
                _typeManager = typeManager;
                _parentBranch = parentBranch;
            }

            private void PushBlock(Block block)
            {
                _blocks.Add(block);
            }

            private void PopBlock(Block block)
            {
                // Verify that we're popping the right block.
                Debug.Assert(block == _blocks[_blocks.Count - 1]);

                _blocks.RemoveAt(_blocks.Count - 1);

                // Join any pending join branches.
                if (block.Joins != null)
                    block.Branch.Join(block.Joins);

                // Restore the current branch from the block.
                _branch = block.Branch;
            }

            private void MarkRead(IBoundReadable variable)
            {
                if (!_branch.IsKilled)
                {
                    var hasBoundType = variable as BoundVariable;

                    if (hasBoundType != null)
                        _branch.MarkRead(hasBoundType);
                }
            }

            private void MarkWrite(IBoundWritable variable)
            {
                if (!_branch.IsKilled)
                {
                    var hasBoundType = variable as BoundVariable;

                    if (hasBoundType != null)
                        _branch.MarkWrite(hasBoundType);
                }
            }

            private void JoinOnBlock(string label, JoinType type)
            {
                // This method implements break/continue/return/throw handling.
                // A break/continue joins the branch on the block that is
                // a break/continue target and has the same label, if one is
                // provided. Return/throw joins on the root branch.
                // If we see a block with a finally, we visit it (because the
                // finally is always taken).
                // The result of this function either is an exception when we
                // don't have a break/continue block (that matches the label if
                // one is provided), or a joined and killed branch.

                for (int i = _blocks.Count - 1; i >= 0; i--)
                {
                    var block = _blocks[i];

                    // If this block has a finally, we need to visit it because
                    // we're falling through it.

                    if (block.Finally != null)
                        Visit(block.Finally);

                    bool match = false;

                    switch (type)
                    {
                        case JoinType.Break:
                            match = block.IsBreakTarget && (label == null || block.Label == label);
                            break;

                        case JoinType.Continue:
                            match = block.IsContinueTarget && (label == null || block.Label == label);

                            // We need to set the ContinueTaken flag if we're a
                            // continue so that for and do/while can do proper
                            // handling.

                            if (match)
                                block.ContinueTaken = true;
                            break;

                        case JoinType.Return:
                            match = i == 0;
                            break;
                    }

                    if (match)
                    {
                        block.JoinBranch(_branch);
                        _branch.IsKilled = true;
                        return;
                    }
                }

                throw new JintException("Could not find break/continue target");
            }

            private string FindLabel(BoundNode node)
            {
                string label;
                _labels.TryGetValue(node, out label);
                return label;
            }

            private bool? ToBoolean(BoundExpression node)
            {
                // This method tests whether the expression evaluates to a constant
                // expression. This depends on a constant folding phase for
                // correct results (which we don't have, so currently it just
                // checks for a true/false constant).

                var constant = node as BoundConstant;
                if (constant != null && constant.Value is bool)
                    return (bool)constant.Value;

                return null;
            }

            public override void VisitBody(BoundBody node)
            {
                using (var marker = _typeManager.CreateDefiniteAssignmentMarker(_parentBranch))
                {
                    _branch = marker.CreateDefaultBranch();

                    var block = new Block(_branch, null, false, false, null);

                    PushBlock(block);

                    base.VisitBody(node);

                    PopBlock(block);

                    ResultExpressions = _branch.Expressions;

                    // If the last statement of the body is a return, the return
                    // provides the result expression and we don't have to track
                    // the result expressions.

                    if (
                        node.Body.Nodes.Count == 0 ||
                        node.Body.Nodes[node.Body.Nodes.Count - 1] is BoundReturn
                    )
                        ResultExpressions = null;

                    _branch.IsKilled = true;
                    _branch = null;
                }
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                base.VisitSetVariable(node);

                MarkWrite(node.Variable);
                _branch.MarkExpression(node.Value);
            }

            public override void VisitExpressionStatement(BoundExpressionStatement node)
            {
                base.VisitExpressionStatement(node);

                _branch.MarkExpression(node.Expression);
            }

            public override void VisitGetVariable(BoundGetVariable node)
            {
                MarkRead(node.Variable);
            }

            public override void VisitCatch(BoundCatch node)
            {
                MarkWrite(node.Target);

                base.VisitCatch(node);
            }

            public override void VisitExpressionBlock(BoundExpressionBlock node)
            {
                base.VisitExpressionBlock(node);

                MarkRead(node.Result);
            }

            public override void VisitLabel(BoundLabel node)
            {
                _labels.Add(node.Statement, node.Label);

                base.VisitLabel(node);
            }

            public override void VisitBreak(BoundBreak node)
            {
                JoinOnBlock(node.Target, JoinType.Break);
            }

            public override void VisitContinue(BoundContinue node)
            {
                JoinOnBlock(node.Target, JoinType.Continue);
            }

            public override void VisitReturn(BoundReturn node)
            {
                base.VisitReturn(node);

                JoinOnBlock(null, JoinType.Return);
            }

            public override void VisitThrow(BoundThrow node)
            {
                base.VisitThrow(node);

                // Throw acts like a return in the context of definite
                // assignment.

                JoinOnBlock(null, JoinType.Return);
            }

            public override void VisitDoWhile(BoundDoWhile node)
            {
                // Rules for do/while:
                // * _branch stays default branch (we always take the do);
                // * Target for break/continue;
                // * Test is only executed when the default branch reaches the end
                //   or a continue was executed.

                var block = new Block(_branch, FindLabel(node), true, true, null);

                PushBlock(block);

                _branch = block.Branch.Fork();

                Visit(node.Body);

                bool wasKilled = _branch.IsKilled;

                if (!_branch.IsKilled || block.ContinueTaken)
                {
                    // We need to restore the branch because if the branch is
                    // killed, but we have a continue, the test is still done on
                    // the branch.
                    _branch.IsKilled = false;

                    Visit(node.Test);
                }

                if (!wasKilled)
                    block.JoinBranch(_branch);

                PopBlock(block);
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                // Rules for for each in:
                // * _branch becomes a fork and we create an empty fork;
                // * Target for break/continue.

                // Expression is executed on the default branch.
                Visit(node.Expression);

                MarkWrite(node.Target);

                var block = new Block(_branch, FindLabel(node), true, true, null);

                PushBlock(block);

                _branch = block.Branch.Fork();

                Visit(node.Body);

                block.JoinBranch(_branch);

                // Create an empty branch to signal that the body is optional.

                block.JoinBranch(block.Branch.Fork());

                PopBlock(block);
            }

            public override void VisitFor(BoundFor node)
            {
                // Rules for for:
                // * _branch stays default when IsTrue(Test); otherwise _branch
                //   becomes a fork;
                // * Target for break/continue;
                // * Increment is only executed when the default branch reaches
                //   the end or a continue was executed.

                // Initialization and test are executed on the current branch.
                Visit(node.Initialization);
                Visit(node.Test);

                var block = new Block(_branch, FindLabel(node), true, true, null);

                _branch = block.Branch.Fork();

                PushBlock(block);

                Visit(node.Body);

                bool wasKilled = _branch.IsKilled;

                // Only visit the increment when then default branch flows there
                // or a continue was executed.
                if (!_branch.IsKilled || block.ContinueTaken)
                {
                    // We need to restore the branch because if the branch is
                    // killed, but we have a continue, the test is still done on
                    // the branch.
                    _branch.IsKilled = false;

                    Visit(node.Increment);
                }

                if (!wasKilled)
                    block.JoinBranch(_branch);

                // Create an extra empty branch when the test wasn't unconditional.
                if (node.Test != null && ToBoolean(node.Test) != true)
                    block.JoinBranch(block.Branch.Fork());

                PopBlock(block);
            }

            public override void VisitIf(BoundIf node)
            {
                // Rules for if:
                // * When IsTrue(Test), Else is not taken and Then stays the
                //   default branch;
                // * When IsFalse(Test), Then is not taken and If stays the
                //   default branch;
                // * Otherwise, Then and Else become a fork (even when Else
                //   is null).

                bool? result = ToBoolean(node.Test);

                // If Test evaluates to a constant that allows us to skip
                // the Then, and we don't have an Else, we don't have any
                // work for this node.

                if (result == false && node.Else == null)
                    return;

                // Test is executed on the default branch.
                Visit(node.Test);

                var block = new Block(_branch, null, false, false, null);

                PushBlock(block);

                if (!result.HasValue)
                {
                    // Visit the Then.

                    _branch = block.Branch.Fork();

                    Visit(node.Then);

                    block.JoinBranch(_branch);

                    // Visit the Else. Note that we always fork the branch, even
                    // when we don't have an Else. The reason for this is that
                    // the join algorithm correctly marks variables that are
                    // only written to in the Then branch as not definitely
                    // assigned.

                    _branch = block.Branch.Fork();

                    if (node.Else != null)
                        Visit(node.Else);

                    block.JoinBranch(_branch);
                }
                else
                {
                    Visit(result.Value ? node.Then : node.Else);
                }

                PopBlock(block);
            }

            public override void VisitSwitch(BoundSwitch node)
            {
                // Rules for switch:
                // * Target for break;
                // * If we don't have any cases, there is no work;
                // * If we only have a default case, _branch stays the default
                //   and there is no special handling;
                // * Otherwise, we fully process the cases. Things become a bit
                //   tricky in a switch because of label fall through.
                //   Cases really are nested ifs. Say we have the following:
                //
                //     switch (x) {
                //     case 1:
                //       a();
                //     case 2:
                //       b();
                //       break;
                //     }
                //
                //   This can be rewritten as
                //
                //     if (x == 1 || x == 2) {
                //       if (x == 1) {
                //         a();
                //       }
                //       b();
                //     }
                //
                //   With this rewrite, we can construct cases as follows:
                //     - Every case is a fork;
                //     - If the branch isn't killed, we create an extra fork
                //       (the else of the 'if (x == 1)' above);
                //     - Otherwise, we create a new fork for the next case.
                //     - Default is treated like any other case; except that when
                //       we don't have a default, we create an extra fork at the
                //       end of the switch.
                //
                // One extra note: we don't have an expression here. The bound
                // tree has already introduced a temporary for the result of the
                // expression, so we don't have any handling for that here.

                // Mark a read on the temporary.
                MarkRead(node.Temporary);

                // No cases means no work.
                if (node.Cases.Count == 0)
                    return;

                // When we only have a default case, we can just take the default.
                // However, we're still a break target so we do need to create
                // a block.
                if (node.Cases.Count == 1 && node.Cases[0].Expression == null)
                {
                    var block = new Block(_branch, FindLabel(node), true, false, null);

                    PushBlock(block);

                    base.VisitSwitch(node);

                    PopBlock(block);

                    return;
                }

                bool hadDefault = false;

                // Push the block for the switch.

                var switchBlock = new Block(_branch, FindLabel(node), true, false, null);

                PushBlock(switchBlock);

                // Clear the branch to signal the creation of a new branch.

                BoundTypeManager.DefiniteAssignmentMarker.Branch caseBranch = null;

                _branch = null;

                for (int i = 0; i < node.Cases.Count; i++)
                {
                    var @case = node.Cases[i];

                    if (@case.Expression == null)
                        hadDefault = true;

                    // Create a new branch for a series of cases (either the
                    // first one or on fall through).

                    if (caseBranch == null)
                        caseBranch = switchBlock.Branch.Fork();

                    // Create a new branch for the contents of this case.

                    _branch = caseBranch.Fork();

                    Visit(@case);

                    // Do we have a fall through (and we're not the last, because
                    // that doesn't fall through)?

                    if (!_branch.IsKilled && i != node.Cases.Count - 1)
                    {
                        // If we have a fall through, join the branch
                        // and an empty branch to close the current case.
                        // This makes this case optional.

                        caseBranch.Join(new[] { _branch, caseBranch.Fork() });
                    }
                    else
                    {
                        // If we don't have a fall through, check whether the
                        // branch is killed. If it isn't killed, we must be the
                        // last case and it's missing the break, which we insert
                        // here.

                        if (!_branch.IsKilled)
                        {
                            Debug.Assert(i == node.Cases.Count - 1);

                            JoinOnBlock(null, JoinType.Break);
                        }

                        // The case branch has already been joined on the switch
                        // block. Close this case and signal the creation of a new
                        // branch.

                        caseBranch = null;
                    }
                }

                // If we didn't have a default case, we need to create an empty
                // branch for the else.

                if (!hadDefault)
                    switchBlock.JoinBranch(switchBlock.Branch.Fork());

                // And pop the switch block.

                PopBlock(switchBlock);
            }

            public override void VisitTry(BoundTry node)
            {
                // Rules for try/catch:
                // * Try stays the default;
                // * Catch becomes a fork (it's only taken when an exception is
                //   thrown);
                // * Finally is treated special. It's also executed on the default
                //   branch, but it's also executed from break/continue/throw/return's,
                //   because these flow through the finally. We don't do this here,
                //   but in JoinOnBlock.

                var block = new Block(_branch, null, false, false, node.Finally);

                PushBlock(block);

                Visit(node.Try);

                if (node.Catch != null)
                {
                    // Create a block for the catch.

                    var catchBlock = new Block(block.Branch, null, false, false, null);

                    PushBlock(catchBlock);

                    _branch = catchBlock.Branch.Fork();

                    Visit(node.Catch);

                    catchBlock.JoinBranch(_branch);

                    // Create an empty fork to tell the Join algorithm that the
                    // Catch was an optional branch.

                    catchBlock.JoinBranch(catchBlock.Branch.Fork());

                    PopBlock(catchBlock);
                }

                Visit(node.Finally);

                PopBlock(block);
            }

            public override void VisitWhile(BoundWhile node)
            {
                // Rules for while:
                // * When IsFalse(Test), we don't have any work;
                // * When IsTrue(Test), _branch stays the default;
                // * Otherwise, _branch becomes a fork;
                // * Target for break/continue.

                var result = ToBoolean(node.Test);

                // If Test evaluates to a constant expression false, the while
                // loop is never taken.

                if (result == false)
                    return;

                // The Test is executed on the default branch.

                Visit(node.Test);

                var block = new Block(_branch, FindLabel(node), true, true, null);

                PushBlock(block);

                _branch = block.Branch.Fork();

                Visit(node.Body);

                block.JoinBranch(_branch);

                // Create an empty fork to tell the Join algorithm that the
                // Body was an optional branch.

                if (result != true)
                    block.JoinBranch(block.Branch.Fork());

                PopBlock(block);
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                // It's the responsibility of the phase to handle functions.
                // Definite assignment also applies to closed over variables.
                // The only problem is that we don't know when the function is
                // called, so we don't know for sure when the variable is
                // definitely assigned. However, we do know it isn't before the
                // function reference is created, so we do it here.

                Perform(node.Function.Body, _branch);
            }

            private class Block
            {
                private static readonly int IsBreakTargetMask = BitVector32.CreateMask();
                private static readonly int IsContinueTargetMask = BitVector32.CreateMask(IsBreakTargetMask);
                private static readonly int ContinueTakenMask = BitVector32.CreateMask(IsContinueTargetMask);

                private BitVector32 _flags;

                public string Label { get; private set; }
                public BoundTypeManager.DefiniteAssignmentMarker.Branch Branch { get; private set; }
                public BoundFinally Finally { get; private set; }

                public List<BoundTypeManager.DefiniteAssignmentMarker.Branch> Joins { get; private set; }

                public bool IsBreakTarget
                {
                    get { return _flags[IsBreakTargetMask]; }
                    private set { _flags[IsBreakTargetMask] = value; }
                }

                public bool IsContinueTarget
                {
                    get { return _flags[IsContinueTargetMask]; }
                    private set { _flags[IsContinueTargetMask] = value; }
                }

                public bool ContinueTaken
                {
                    get { return _flags[ContinueTakenMask]; }
                    set { _flags[ContinueTakenMask] = value; }
                }

                public Block(BoundTypeManager.DefiniteAssignmentMarker.Branch branch, string label, bool isBreakTarget, bool isContinueTarget, BoundFinally @finally)
                {
                    Label = label;
                    Finally = @finally;
                    Branch = branch;
                    IsBreakTarget = isBreakTarget;
                    IsContinueTarget = isContinueTarget;
                }

                public void JoinBranch(BoundTypeManager.DefiniteAssignmentMarker.Branch branch)
                {
                    // When a branch is killed, _branch becomes null. We test for
                    // that here to simplify the algorithm. We also skip the branch
                    // when it's the branch of this block for the same reason.

                    if (branch.IsKilled || branch == Branch)
                        return;

                    if (Joins == null)
                        Joins = new List<BoundTypeManager.DefiniteAssignmentMarker.Branch>();

                    Joins.Add(branch);
                }
            }

            private enum JoinType
            {
                Break,
                Continue,
                Return
            }
        }
    }
}

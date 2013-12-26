using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal static class ResultRewriterPhase
    {
        public static BoundProgram Perform(BoundProgram program, IList<BoundExpression> resultExpressions)
        {
            // If the last statement of the program is a return, we don't
            // have to rewrite the program.

            var body = program.Body;
            if (
                body.Body.Nodes.Count > 0 &&
                body.Body.Nodes[body.Body.Nodes.Count - 1] is BoundReturn
            )
                return program;

            // If we don't have any result expressions, we only have to
            // insert a return statement.

            if (resultExpressions == null || resultExpressions.Count == 0)
            {
                // Create a new nodes list with the added return statement.

                var nodes = new ReadOnlyArray<BoundStatement>.Builder();

                nodes.AddRange(body.Body.Nodes);
                nodes.Add(new BoundReturn(
                    null,
                    SourceLocation.Missing
                ));

                // Return the updated program.

                return program.Update(
                    body.Update(
                        body.Body.Update(
                            body.Body.Temporaries,
                            nodes.ToReadOnly(),
                            body.Body.Location
                        ),
                        body.Closure,
                        body.ScopedClosure,
                        body.Arguments,
                        body.Locals,
                        body.TypeManager
                    )
                );
            }

            // Otherwise, we need to do a full rewrite.

            return program.Update(
                (BoundBody)new Rewriter(resultExpressions, body.TypeManager).Visit(program.Body)
            );
        }

        private class Rewriter : BoundTreeRewriter
        {
            private readonly BoundTypeManager _typeManager;
            private readonly HashSet<BoundExpression> _resultExpressions;
            private readonly BoundTemporary _resultTemporary;
            private int _lastTemporaryIndex;

            public Rewriter(IEnumerable<BoundExpression> resultExpressions, BoundTypeManager typeManager)
            {
                _typeManager = typeManager;
                _resultExpressions = new HashSet<BoundExpression>(resultExpressions);

                _resultTemporary = new BoundTemporary(
                    --_lastTemporaryIndex,
                    typeManager.CreateType(null, BoundTypeKind.Temporary)
                );
            }

            public override BoundNode Visit(BoundNode node)
            {
                var expression = node as BoundExpression;
                if (expression != null && _resultExpressions.Contains(expression))
                {
                    // Create a temporary to hold the result.

                    var temporary = new BoundTemporary(
                        --_lastTemporaryIndex,
                        _typeManager.CreateType(null, BoundTypeKind.Temporary)
                    );

                    var nodes = new ReadOnlyArray<BoundStatement>.Builder();

                    // Initialize the temporary with the expression.

                    nodes.Add(new BoundSetVariable(
                        temporary,
                        expression,
                        SourceLocation.Missing
                    ));

                    // Copy the temporary to the result.

                    nodes.Add(new BoundSetVariable(
                        _resultTemporary,
                        new BoundGetVariable(temporary),
                        SourceLocation.Missing
                    ));

                    // Return an expression block.

                    return new BoundExpressionBlock(
                        temporary,
                        new BoundBlock(
                            ReadOnlyArray<BoundTemporary>.CreateFrom(temporary),
                            nodes.ToReadOnly(),
                            SourceLocation.Missing
                        )
                    );
                }

                return base.Visit(node);
            }

            public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
            {
                // If the expression is the expression of an expression statement,
                // we don't have to introduce an expression block and instead
                // just change it to a set variable. We also specifically
                // don't visit the expression because (1) this is if no use
                // because the only thing it could do is set the result temporary
                // which we're assigning here anyway and (2) this way we don't
                // need to have a special case in Visit.

                if (_resultExpressions.Contains(node.Expression))
                {
                    return new BoundSetVariable(
                        _resultTemporary,
                        node.Expression,
                        node.Location
                    );
                }

                return base.VisitExpressionStatement(node);
            }

            public override BoundNode VisitBody(BoundBody node)
            {
                node = (BoundBody)base.VisitBody(node);

                // Add the initialization of the result temporary and the
                // return statement.

                var nodes = new ReadOnlyArray<BoundStatement>.Builder(node.Body.Nodes.Count + 2);

                // We always add the default value. The reason for this is that
                // we're already passed the definite assignment phase, so it won't
                // be inserted for us automatically.

                nodes.Add(new BoundSetVariable(
                    _resultTemporary,
                    new BoundGetVariable(BoundMagicVariable.Undefined),
                    SourceLocation.Missing
                ));

                nodes.AddRange(node.Body.Nodes);

                nodes.Add(new BoundReturn(
                    new BoundGetVariable(_resultTemporary),
                    SourceLocation.Missing
                ));

                return node.Update(
                    node.Body.Update(
                        node.Body.Temporaries,
                        nodes.ToReadOnly(),
                        node.Body.Location
                    ),
                    node.Closure,
                    node.ScopedClosure,
                    node.Arguments,
                    node.Locals,
                    node.TypeManager
                );
            }
        }
    }
}

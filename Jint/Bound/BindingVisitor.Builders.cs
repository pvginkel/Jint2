﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Expressions;
using System.Diagnostics;
using Jint.Support;

namespace Jint.Bound
{
    partial class BindingVisitor
    {
        private BoundStatement BuildSet(SyntaxNode syntax, BoundExpression value)
        {
            switch (syntax.Type)
            {
                case SyntaxType.Identifier:
                    return BuildSet(((IdentifierSyntax)syntax).Target, value);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return BuildSetMember(
                        BuildExpression(property.Expression),
                        BoundConstant.Create(property.Name),
                        value
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return BuildSetMember(
                        BuildGet(indexer.Expression),
                        BuildExpression(indexer.Index),
                        value
                    );

                default:
                    throw new InvalidOperationException();
            }
        }

        private BoundStatement BuildSet(Variable variable, BoundExpression value)
        {
            switch (variable.Type)
            {
                case VariableType.Global:
                    return BuildSetMember(
                        new BoundGetVariable(BoundMagicVariable.Global),
                        BoundConstant.Create(variable.Name),
                        value
                    );

                case VariableType.Parameter:
                    return new BoundSetVariable(
                        _scope.GetArgument(variable),
                        value
                    );

                case VariableType.WithScope:
                    var builder = new BlockBuilder(this);

                    var valueTemporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        valueTemporary,
                        value
                    ));

                    builder.Add(BuildSetWithScope(builder, variable.WithScope, variable.FallbackVariable, valueTemporary));

                    return builder.BuildBlock();

                case VariableType.This:
                    return BuildThrow("ReferenceError", "Invalid left-hand side in assignment");

                case VariableType.Local:
                case VariableType.Arguments:
                    if (variable.ClosureField == null)
                        return new BoundSetVariable(_scope.GetLocal(variable), value);

                    return new BoundSetVariable(
                        _scope.GetClosureField(variable),
                        value
                    );

                default:
                    throw new InvalidOperationException("Cannot find variable of argument");
            }
        }

        private BoundIf BuildSetWithScope(BlockBuilder builder, WithScope withScope, Variable fallbackVariable, BoundTemporary value)
        {
            var withLocal = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                withLocal,
                new BoundGetVariable(_withTemporaries[withScope.Variable])
            ));

            var setter = new BoundSetMember(
                new BoundGetVariable(withLocal),
                BoundConstant.Create(fallbackVariable.Name),
                new BoundGetVariable(value)
            );

            BoundBlock @else;

            if (withScope.Parent == null)
            {
                @else = BuildBlock(BuildSet(
                    fallbackVariable,
                    new BoundGetVariable(value)
                ));
            }
            else
            {
                @else = BuildBlock(BuildSetWithScope(
                    builder,
                    withScope.Parent,
                    fallbackVariable,
                    value
                ));
            }

            return new BoundIf(
                new BoundHasMember(
                    new BoundGetVariable(withLocal),
                    BoundConstant.Create(fallbackVariable.Name)
                ),
                BuildBlock(setter),
                @else
            );
        }

        private static BoundStatement BuildThrow(string @class, string message)
        {
            var arguments = new ReadOnlyArray<BoundCallArgument>.Builder();

            if (message != null)
            {
                arguments.Add(new BoundCallArgument(
                    BoundConstant.Create(message),
                    false
                ));
            }

            // Build the throw.
            return new BoundThrow(
                // Instantiate the new error class.
                new BoundNew(
                    // Get the error class.
                    new BoundGetMember(
                        new BoundGetVariable(BoundMagicVariable.Global),
                        BoundConstant.Create(@class)
                    ),
                    // Pass the arguments (the message).
                    arguments.ToReadOnly(),
                    ReadOnlyArray<BoundExpression>.Null
                )
            );
        }

        private BoundExpression BuildGet(ExpressionSyntax syntax)
        {
            return BuildGet(syntax, null);
        }

        private BoundExpression BuildGet(ExpressionSyntax syntax, BoundTemporary withTarget)
        {
            switch (syntax.Type)
            {
                case SyntaxType.Identifier:
                    return BuildGet(((IdentifierSyntax)syntax).Target, withTarget);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return BuildGetMember(
                        BuildExpression(property.Expression),
                        BoundConstant.Create(property.Name)
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return BuildGetMember(
                        BuildGet(indexer.Expression, withTarget),
                        BuildExpression(indexer.Index)
                    );

                default:
                    return BuildExpression(syntax);
            }
        }

        private BoundExpression BuildGet(Variable variable, BoundTemporary withTarget)
        {
            switch (variable.Type)
            {
                case VariableType.Global:
                    return BuildGetMember(
                        new BoundGetVariable(BoundMagicVariable.Global),
                        BoundConstant.Create(variable.Name)
                    );

                case VariableType.Parameter:
                    return new BoundGetVariable(_scope.GetArgument(variable));

                case VariableType.WithScope:
                    var builder = new BlockBuilder(this);
                    var result = builder.CreateTemporary();

                    builder.Add(
                        BuildGetWithScope(builder, variable.WithScope, variable.FallbackVariable, result, withTarget)
                    );

                    return builder.BuildExpression(result);

                case VariableType.This:
                    return new BoundGetVariable(BoundMagicVariable.This);

                case VariableType.Local:
                case VariableType.Arguments:
                    if (variable.ClosureField == null)
                        return new BoundGetVariable(_scope.GetLocal(variable));

                    return new BoundGetVariable(_scope.GetClosureField(variable));

                default:
                    throw new InvalidOperationException("Cannot find variable of argument");
            }
        }

        private BoundIf BuildGetWithScope(BlockBuilder builder, WithScope withScope, Variable fallbackVariable, BoundTemporary result, BoundTemporary withTarget)
        {
            var withLocal = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                withLocal,
                new BoundGetVariable(_withTemporaries[withScope.Variable])
            ));

            var getter = new BlockBuilder(this);

            if (withLocal != null)
            {
                getter.Add(new BoundSetVariable(
                    withTarget,
                    new BoundGetVariable(withLocal)
                ));
            }

            getter.Add(new BoundSetVariable(
                result,
                BuildGetMember(
                    new BoundGetVariable(withLocal),
                    BoundConstant.Create(fallbackVariable.Name)
                )
            ));

            BoundBlock @else;

            if (withScope.Parent == null)
            {
                @else = BuildBlock(new BoundSetVariable(
                    result,
                    BuildGet(
                        fallbackVariable,
                        null
                    )
                ));
            }
            else
            {
                @else = BuildBlock(BuildGetWithScope(
                    builder,
                    withScope.Parent,
                    fallbackVariable,
                    result,
                    withTarget
                ));
            }

            return new BoundIf(
                new BoundHasMember(
                    new BoundGetVariable(withLocal),
                    BoundConstant.Create(fallbackVariable.Name)
                ),
                getter.BuildBlock(),
                @else
            ); 
        }

        private BoundBlock BuildBlock(BoundStatement node)
        {
            var block = node as BoundBlock;
            if (block != null)
                return block;

            var builder = new BlockBuilder(this);

            builder.Add(node);

            return builder.BuildBlock();
        }

        private BoundExpression BuildGetMember(BoundExpression expression, BoundExpression index)
        {
            return new BoundGetMember(
                expression,
                index
            );
        }

        private BoundSetMember BuildSetMember(BoundExpression expression, BoundExpression index, BoundExpression value)
        {
            return new BoundSetMember(
                expression,
                index,
                value
            );
        }

        private BoundBlock BuildBlock(SyntaxNode syntax)
        {
            IEnumerable<SyntaxNode> items;
            var block = syntax as BlockSyntax;
            if (block != null)
                items = block.Statements;
            else
                items = new[] { syntax };

            var builder = new BlockBuilder(this);

            foreach (var item in items)
            {
                var node = item.Accept(this);

                var statement =
                    node as BoundStatement ??
                    new BoundExpressionStatement((BoundExpression)node);

                builder.Add(statement);
            }

            return builder.BuildBlock();
        }

        private BoundExpression BuildExpression(SyntaxNode syntax)
        {
            return (BoundExpression)syntax.Accept(this);
        }

        private class BlockBuilder
        {
            private readonly BindingVisitor _visitor;
            private ReadOnlyArray<BoundTemporary>.Builder _temporaries;
            private ReadOnlyArray<BoundStatement>.Builder _nodes;

            public BlockBuilder(BindingVisitor visitor)
            {
                _visitor = visitor;
            }

            internal void Add(BoundStatement node)
            {
                if (_nodes == null)
                    _nodes = new ReadOnlyArray<BoundStatement>.Builder();

                var block = node as BoundBlock;
                if (block != null && block.Nodes.Count == 1)
                    _nodes.Add(block.Nodes[0]);
                else
                    _nodes.Add(node);
            }

            public BoundExpression BuildExpression(BoundTemporary result)
            {
                return new BoundExpressionBlock(
                    result,
                    new BoundBlock(
                        _temporaries == null ? ReadOnlyArray<BoundTemporary>.Empty : _temporaries.ToReadOnly(),
                        _nodes == null ? ReadOnlyArray<BoundStatement>.Empty : _nodes.ToReadOnly()
                    )
                );
            }

            public BoundBlock BuildBlock()
            {
                if (_nodes == null)
                {
                    Debug.Assert(_temporaries == null);

                    return new BoundBlock(
                        ReadOnlyArray<BoundTemporary>.Empty,
                        ReadOnlyArray<BoundStatement>.Empty
                    );
                }

                var nodes = new ReadOnlyArray<BoundStatement>.Builder();

                foreach (var node in _nodes.ToReadOnly())
                {
                    if (node is BoundEmpty)
                        continue;

                    // Flatten blocks. Nested blocks have no use, so we flatten
                    // them into this block. First we try to see whether the node
                    // is a block. If not, we try to see whether the node is an
                    // expression statement that has an expression block
                    // as its expression. In that case, we're taking the value of
                    // an expression block without doing anything with it.

                    var block = node as BoundBlock;
                    if (block == null)
                    {
                        var statement = node as BoundExpressionStatement;
                        if (statement != null)
                        {
                            var expressionBlock = statement.Expression as BoundExpressionBlock;
                            if (expressionBlock != null)
                                block = expressionBlock.Body;
                        }
                    }

                    if (block != null)
                    {
                        if (_temporaries == null)
                            _temporaries = new ReadOnlyArray<BoundTemporary>.Builder();

                        _temporaries.AddRange(block.Temporaries);
                        nodes.AddRange(block.Nodes);
                        continue;
                    }

                    // Otherwise, we keep the node unchanged.
                    nodes.Add(node);
                }

                return new BoundBlock(
                    _temporaries == null ? ReadOnlyArray<BoundTemporary>.Empty : _temporaries.ToReadOnly(),
                    nodes.ToReadOnly()
                );
            }

            public BoundTemporary CreateTemporary()
            {
                var result = new BoundTemporary(
                    _visitor._scope.GetNextTemporaryIndex(),
                    _visitor._scope.TypeManager.CreateType(BoundTypeKind.Temporary)
                );

                if (_temporaries == null)
                    _temporaries = new ReadOnlyArray<BoundTemporary>.Builder();

                _temporaries.Add(result);

                return result;
            }
        }
    }
}
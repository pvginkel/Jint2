using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Jint.Ast;

namespace Jint.Bound
{
    partial class BindingVisitor
    {
        private BoundStatement BuildSet(SyntaxNode syntax, BoundExpression value)
        {
            switch (syntax.Type)
            {
                case SyntaxType.Identifier:
                    var identifier = ((IdentifierSyntax)syntax).Identifier;

                    if (identifier.Type == IdentifierType.Global)
                        _scope.IsGlobalScopeReferenced = true;

                    return BuildSet(identifier, value);

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

        private BoundStatement BuildSet(IIdentifier identifier, BoundExpression value)
        {
            switch (identifier.Type)
            {
                case IdentifierType.Parameter:
                    return new BoundSetVariable(
                        _scope.GetArgument(identifier),
                        value,
                        SourceLocation.Missing
                    );

                case IdentifierType.Scoped:
                    var builder = new BlockBuilder(this);

                    var valueTemporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        valueTemporary,
                        value,
                        SourceLocation.Missing
                    ));

                    builder.Add(BuildSetWithScope(builder, identifier.WithScope, identifier.Fallback, valueTemporary));

                    return builder.BuildBlock(SourceLocation.Missing);

                case IdentifierType.Local:
                case IdentifierType.Global:
                    if (identifier.Type == IdentifierType.Global)
                        _scope.IsGlobalScopeReferenced = true;

                    if (identifier.Closure == null)
                        return new BoundSetVariable(_scope.GetLocal(identifier), value, SourceLocation.Missing);

                    return new BoundSetVariable(
                        _scope.GetClosureField(identifier),
                        value,
                        SourceLocation.Missing
                    );

                    /*
                    // These are handled upstream.
                case IdentifierType.This:
                case IdentifierType.Null:
                case IdentifierType.Undefined:
                case IdentifierType.Arguments:
                     */

                default:
                    throw new InvalidOperationException("Cannot find variable of argument");
            }
        }

        private BoundIf BuildSetWithScope(BlockBuilder builder, WithScope withScope, IIdentifier fallback, BoundTemporary value)
        {
            var withLocal = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                withLocal,
                new BoundGetVariable(_withIdentifiers[withScope.Identifier]), SourceLocation.Missing
            ));

            var setter = new BoundSetMember(
                new BoundGetVariable(withLocal),
                BoundConstant.Create(fallback.Name),
                new BoundGetVariable(value),
                SourceLocation.Missing
            );

            BoundBlock @else;

            if (withScope.Parent == null)
            {
                @else = BuildBlock(BuildSet(
                    fallback,
                    new BoundGetVariable(value)
                ));
            }
            else
            {
                @else = BuildBlock(BuildSetWithScope(
                    builder,
                    withScope.Parent,
                    fallback,
                    value
                ));
            }

            return new BoundIf(
                new BoundHasMember(
                    new BoundGetVariable(withLocal),
                    fallback.Name
                ),
                BuildBlock(setter),
                @else,
                SourceLocation.Missing
            );
        }

        private BoundStatement BuildThrow(string @class, string message)
        {
            var arguments = new ReadOnlyArray<BoundCallArgument>.Builder();

            if (message != null)
            {
                arguments.Add(new BoundCallArgument(
                    BoundConstant.Create(message),
                    false
                ));
            }

            _scope.IsGlobalScopeReferenced = true;

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
                    ReadOnlyArray<BoundExpression>.Empty
                ),
                SourceLocation.Missing
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
                    var identifier = ((IdentifierSyntax)syntax).Identifier;

                    if (identifier.Type == IdentifierType.Global)
                        _scope.IsGlobalScopeReferenced = true;

                    return BuildGet(identifier, withTarget);

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

        private BoundExpression BuildGet(IIdentifier identifier, BoundTemporary withTarget)
        {
            switch (identifier.Type)
            {
                case IdentifierType.This: return new BoundGetVariable(BoundMagicVariable.This);
                case IdentifierType.Null: return new BoundGetVariable(BoundMagicVariable.Null);
                case IdentifierType.Undefined: return new BoundGetVariable(BoundMagicVariable.Undefined);
                case IdentifierType.Arguments:
                    _scope.IsArgumentsReferenced = true;
                    return new BoundGetVariable(BoundMagicVariable.Arguments);

                case IdentifierType.Parameter:
                    return new BoundGetVariable(_scope.GetArgument(identifier));

                case IdentifierType.Scoped:
                    var builder = new BlockBuilder(this);
                    var result = builder.CreateTemporary();

                    builder.Add(
                        BuildGetWithScope(builder, identifier.WithScope, identifier.Fallback, result, withTarget)
                    );

                    return builder.BuildExpression(result, SourceLocation.Missing);

                case IdentifierType.Local:
                case IdentifierType.Global:
                    if (identifier.Closure != null)
                        return new BoundGetVariable(_scope.GetClosureField(identifier));

                    return new BoundGetVariable(_scope.GetLocal(identifier));

                default:
                    throw new InvalidOperationException("Cannot find variable of argument");
            }
        }

        private BoundIf BuildGetWithScope(BlockBuilder builder, WithScope withScope, IIdentifier fallback, BoundTemporary result, BoundTemporary withTarget)
        {
            var withLocal = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                withLocal,
                new BoundGetVariable(_withIdentifiers[withScope.Identifier]),
                SourceLocation.Missing
            ));

            var getter = new BlockBuilder(this);

            if (withTarget != null)
            {
                getter.Add(new BoundSetVariable(
                    withTarget,
                    new BoundGetVariable(withLocal),
                    SourceLocation.Missing
                ));
            }

            getter.Add(new BoundSetVariable(
                result,
                BuildGetMember(
                    new BoundGetVariable(withLocal),
                    BoundConstant.Create(fallback.Name)
                ),
                SourceLocation.Missing
            ));

            BoundBlock @else;

            if (withScope.Parent == null)
            {
                @else = BuildBlock(new BoundSetVariable(
                    result,
                    BuildGet(
                        fallback,
                        null
                    ),
                    SourceLocation.Missing
                ));
            }
            else
            {
                @else = BuildBlock(BuildGetWithScope(
                    builder,
                    withScope.Parent,
                    fallback,
                    result,
                    withTarget
                ));
            }

            return new BoundIf(
                new BoundHasMember(
                    new BoundGetVariable(withLocal),
                    fallback.Name
                ),
                getter.BuildBlock(SourceLocation.Missing),
                @else,
                SourceLocation.Missing
            ); 
        }

        private BoundBlock BuildBlock(BoundStatement node)
        {
            var block = node as BoundBlock;
            if (block != null)
                return block;

            var builder = new BlockBuilder(this);

            builder.Add(node);

            return builder.BuildBlock(node.Location);
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
                value,
                SourceLocation.Missing
            );
        }

        private BoundBlock BuildBlock(SyntaxNode syntax)
        {
            return BuildBlock(syntax, null);
        }

        private BoundBlock BuildBlock(SyntaxNode syntax, SourceLocation location)
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
                    new BoundExpressionStatement((BoundExpression)node, GetLocation(item));

                builder.Add(statement);
            }

            return builder.BuildBlock(location ?? SourceLocation.Missing);
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
                if (block != null && block.Nodes.Count == 1 && block.Location == SourceLocation.Missing)
                    _nodes.Add(block.Nodes[0]);
                else
                    _nodes.Add(node);
            }

            public BoundExpression BuildExpression(BoundTemporary result, SourceLocation location)
            {
                return new BoundExpressionBlock(
                    result,
                    new BoundBlock(
                        _temporaries == null ? ReadOnlyArray<BoundTemporary>.Empty : _temporaries.ToReadOnly(),
                        _nodes == null ? ReadOnlyArray<BoundStatement>.Empty : _nodes.ToReadOnly(),
                        location
                    )
                );
            }

            public BoundBlock BuildBlock(SourceLocation location)
            {
                if (_nodes == null)
                {
                    Debug.Assert(_temporaries == null);

                    return new BoundBlock(
                        ReadOnlyArray<BoundTemporary>.Empty,
                        ReadOnlyArray<BoundStatement>.Empty,
                        location
                    );
                }

                var nodes = new ReadOnlyArray<BoundStatement>.Builder();

                foreach (var node in _nodes.ToReadOnly())
                {
                    // Flatten blocks. Nested blocks have no use, so we flatten
                    // them into this block. First we try to see whether the node
                    // is a block. If not, we try to see whether the node is an
                    // expression statement that has an expression block
                    // as its expression. In that case, we're taking the value of
                    // an expression block without doing anything with it.

                    var block = node as BoundBlock;
                    if (block != null)
                    {
                        if (_temporaries == null)
                            _temporaries = new ReadOnlyArray<BoundTemporary>.Builder();

                        _temporaries.AddRange(block.Temporaries);
                        if (block.Location != SourceLocation.Missing)
                            nodes.Add(new BoundEmpty(block.Location));
                        nodes.AddRange(block.Nodes);
                        continue;
                    }

                    // Otherwise, we keep the node unchanged.
                    nodes.Add(node);
                }

                return new BoundBlock(
                    _temporaries == null ? ReadOnlyArray<BoundTemporary>.Empty : _temporaries.ToReadOnly(),
                    nodes.ToReadOnly(),
                    location
                );
            }

            public BoundTemporary CreateTemporary()
            {
                var result = new BoundTemporary(
                    _visitor._scope.GetNextTemporaryIndex(),
                    _visitor._scope.TypeManager.CreateType(null, BoundTypeKind.Temporary)
                );

                if (_temporaries == null)
                    _temporaries = new ReadOnlyArray<BoundTemporary>.Builder();

                _temporaries.Add(result);

                return result;
            }
        }
    }
}

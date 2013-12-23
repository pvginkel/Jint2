using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal partial class BindingVisitor : ISyntaxVisitor<BoundNode>
    {
        private readonly Dictionary<SyntaxNode, string> _labels = new Dictionary<SyntaxNode, string>();
        private readonly List<BoundFunction> _functions = new List<BoundFunction>();
        private readonly Dictionary<Variable, BoundTemporary> _withTemporaries = new Dictionary<Variable, BoundTemporary>();

        private Scope _scope;

        public BoundProgram Program { get; private set; }

        public BoundNode VisitArrayDeclaration(ArrayDeclarationSyntax syntax)
        {
            var creation = new BoundNewBuiltIn(BoundNewBuiltInType.Array);

            if (syntax.Parameters.Count == 0)
                return creation;

            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                creation
            ));

            for (int i = 0; i < syntax.Parameters.Count; i++)
            {
                builder.Add(BuildSetMember(
                    new BoundGetVariable(temporary),
                    BoundConstant.Create(i),
                    BuildExpression(syntax.Parameters[i])
                ));
            }

            return builder.BuildExpression(temporary);
        }

        public BoundNode VisitAssignment(AssignmentSyntax syntax)
        {
            if (syntax.Operation == AssignmentOperator.Assign)
            {
                // Name anonymous function with the name if the identifier its
                // initialized to.

                var identifier = syntax.Left as IdentifierSyntax;
                var right = syntax.Right;

                if (identifier != null)
                {
                    var function = right as FunctionSyntax;
                    if (function != null && function.Name == null)
                    {
                        right = new FunctionSyntax(
                            identifier.Name,
                            function.Parameters,
                            function.Body,
                            function.Target,
                            function.Location
                        );
                    }
                }

                var builder = new BlockBuilder(this);

                var temporary = builder.CreateTemporary();

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildExpression(right)
                ));

                builder.Add(BuildSet(
                    syntax.Left,
                    new BoundGetVariable(temporary)
                ));

                return builder.BuildExpression(temporary);
            }
            else
            {
                var builder = new BlockBuilder(this);

                var temporary = builder.CreateTemporary();

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Left)
                ));

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildBinary(
                        AssignmentSyntax.GetSyntaxType(syntax.Operation),
                        new BoundGetVariable(temporary),
                        BuildExpression(syntax.Right)
                    )
                ));

                builder.Add(BuildSet(
                    syntax.Left,
                    new BoundGetVariable(temporary)
                ));

                return builder.BuildExpression(temporary);
            }
        }

        public BoundNode VisitBinary(BinarySyntax syntax)
        {
            return BuildBinary(syntax.Operation, BuildExpression(syntax.Left), BuildExpression(syntax.Right));
        }

        private BoundExpression BuildBinary(SyntaxExpressionType operation, BoundExpression left, BoundExpression right)
        {
            BoundExpressionType type;

            switch (operation)
            {
                case SyntaxExpressionType.And:
                case SyntaxExpressionType.Or:
                    var builder = new BlockBuilder(this);

                    var leftTemporary = builder.CreateTemporary();
                    var resultTemporary = builder.CreateTemporary();

                    if (operation == SyntaxExpressionType.And)
                    {
                        builder.Add(new BoundSetVariable(
                            leftTemporary,
                            left
                        ));

                        builder.Add(new BoundIf(
                            new BoundGetVariable(leftTemporary),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                right
                            )),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                new BoundGetVariable(leftTemporary)
                            ))
                        ));
                    }
                    else
                    {
                        builder.Add(new BoundSetVariable(
                            leftTemporary,
                            left
                        ));

                        builder.Add(new BoundIf(
                            new BoundGetVariable(leftTemporary),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                new BoundGetVariable(leftTemporary)
                            )),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                right
                            ))
                        ));
                    }

                    return builder.BuildExpression(resultTemporary);

                case SyntaxExpressionType.Add: type = BoundExpressionType.Add; break;
                case SyntaxExpressionType.BitwiseAnd: type = BoundExpressionType.BitwiseAnd; break;
                case SyntaxExpressionType.BitwiseExclusiveOr: type = BoundExpressionType.BitwiseExclusiveOr; break;
                case SyntaxExpressionType.BitwiseOr: type = BoundExpressionType.BitwiseOr; break;
                case SyntaxExpressionType.Divide: type = BoundExpressionType.Divide; break;
                case SyntaxExpressionType.LeftShift: type = BoundExpressionType.LeftShift; break;
                case SyntaxExpressionType.RightShift: type = BoundExpressionType.RightShift; break;
                case SyntaxExpressionType.UnsignedRightShift: type = BoundExpressionType.UnsignedRightShift; break;
                case SyntaxExpressionType.Modulo: type = BoundExpressionType.Modulo; break;
                case SyntaxExpressionType.Multiply: type = BoundExpressionType.Multiply; break;
                case SyntaxExpressionType.Subtract: type = BoundExpressionType.Subtract; break;
                case SyntaxExpressionType.Equal: type = BoundExpressionType.Equal; break;
                case SyntaxExpressionType.NotEqual: type = BoundExpressionType.NotEqual; break;
                case SyntaxExpressionType.Same: type = BoundExpressionType.Same; break;
                case SyntaxExpressionType.NotSame: type = BoundExpressionType.NotSame; break;
                case SyntaxExpressionType.LessThan: type = BoundExpressionType.LessThan; break;
                case SyntaxExpressionType.LessThanOrEqual: type = BoundExpressionType.LessThanOrEqual; break;
                case SyntaxExpressionType.GreaterThan: type = BoundExpressionType.GreaterThan; break;
                case SyntaxExpressionType.GreaterThanOrEqual: type = BoundExpressionType.GreaterThanOrEqual; break;
                case SyntaxExpressionType.In: type = BoundExpressionType.In; break;
                case SyntaxExpressionType.InstanceOf: type = BoundExpressionType.InstanceOf; break;
                default: throw new InvalidOperationException();
            }

            return new BoundBinary(type, left, right);
        }

        public BoundNode VisitBlock(BlockSyntax syntax)
        {
            throw new InvalidOperationException("Call BuildBlock instead");
        }

        public BoundNode VisitBreak(BreakSyntax syntax)
        {
            return new BoundBreak(syntax.Target);
        }

        public BoundNode VisitCommaOperator(CommaOperatorSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var resultTemporary = builder.CreateTemporary();

            for (int i = 0; i < syntax.Expressions.Count; i++)
            {
                var expression = syntax.Expressions[i];

                if (i == syntax.Expressions.Count - 1)
                {
                    builder.Add(new BoundSetVariable(
                        resultTemporary,
                        (BoundExpression)expression.Accept(this)
                    ));
                }
                else
                {
                    builder.Add(new BoundExpressionStatement(
                        (BoundExpression)expression.Accept(this)
                    ));
                }
            }

            return builder.BuildExpression(resultTemporary);
        }

        public BoundNode VisitContinue(ContinueSyntax syntax)
        {
            return new BoundContinue(syntax.Target);
        }

        public BoundNode VisitDoWhile(DoWhileSyntax syntax)
        {
            return new BoundDoWhile(
                BuildExpression(syntax.Test),
                BuildBlock(syntax.Body)
            );
        }

        public BoundNode VisitEmpty(EmptySyntax syntax)
        {
            return new BoundEmpty();
        }

        public BoundNode VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            return BuildBlock(syntax.Expression);
        }

        public BoundNode VisitFor(ForSyntax syntax)
        {
            return new BoundFor(
                syntax.Initialization != null ? BuildBlock(syntax.Initialization) : null,
                syntax.Test != null ? BuildExpression(syntax.Test) : null,
                syntax.Increment != null ? BuildBlock(syntax.Increment) : null,
                BuildBlock(syntax.Body)
            );
        }

        public BoundNode VisitForEachIn(ForEachInSyntax syntax)
        {
            return new BoundForEachIn(
                _scope.GetWritable(syntax.Target),
                BuildExpression(syntax.Expression),
                BuildBlock(syntax.Body)
            );
        }

        public BoundNode VisitFunction(FunctionSyntax syntax)
        {
            var function = DeclareFunction(syntax);

            _functions.Add(function);

            var functionObject = new BoundCreateFunction(function);

            if (syntax.Target == null)
                return functionObject;

            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                functionObject
            ));

            builder.Add(BuildSet(
                syntax.Target,
                new BoundGetVariable(temporary)
            ));

            return builder.BuildExpression(temporary);
        }

        public BoundFunction DeclareFunction(FunctionSyntax syntax)
        {
            _scope = new Scope(_scope, syntax.Body);

            var function = new BoundFunction(
                syntax.Name,
                syntax.Parameters.ToReadOnlyArray(),
                VisitBody(syntax.Body)
            );

            _scope = _scope.Parent;

            return function;
        }

        private BoundBody VisitBody(BodySyntax syntax)
        {
            return new BoundBody(
                BuildBlock(syntax),
                _scope.Closure,
                _scope.GetArguments().ToReadOnlyArray(),
                _scope.GetLocals().ToReadOnlyArray(),
                _scope.TypeManager
            );
        }

        public BoundNode VisitIdentifier(IdentifierSyntax syntax)
        {
            return BuildGet(syntax);
        }

        public BoundNode VisitIf(IfSyntax syntax)
        {
            return new BoundIf(
                BuildExpression(syntax.Test),
                BuildBlock(syntax.Then),
                syntax.Else == null ? null : BuildBlock(syntax.Else)
            );
        }

        public BoundNode VisitIndexer(IndexerSyntax syntax)
        {
            return BuildGet(syntax);
        }

        public BoundNode VisitJsonExpression(JsonExpressionSyntax syntax)
        {
            var creation = new BoundNewBuiltIn(BoundNewBuiltInType.Object);

            if (syntax.Properties.Count == 0)
                return creation;

            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(temporary, creation));

            foreach (var property in syntax.Properties)
            {
                var dataProperty = property as JsonDataProperty;
                if (dataProperty != null)
                {
                    builder.Add(new BoundSetMember(
                        new BoundGetVariable(temporary),
                        BoundConstant.Create(dataProperty.Name),
                        BuildExpression(dataProperty.Expression)
                    ));
                }
                else
                {
                    var accessorProperty = (JsonAccessorProperty)property;

                    builder.Add(new BoundSetAccessor(
                        new BoundGetVariable(temporary),
                        BoundConstant.Create(accessorProperty.Name),
                        accessorProperty.GetExpression != null ? BuildExpression(accessorProperty.GetExpression) : null,
                        accessorProperty.SetExpression != null ? BuildExpression(accessorProperty.SetExpression) : null
                    ));
                }
            }

            return builder.BuildExpression(temporary);
        }

        public BoundNode VisitLabel(LabelSyntax syntax)
        {
            return new BoundLabel(
                syntax.Label,
                (BoundStatement)syntax.Expression.Accept(this)
            );
        }

        public BoundNode VisitMethodCall(MethodCallSyntax syntax)
        {
            var builder = new BlockBuilder(this);
            BoundExpression target;
            BoundExpression getter;

            var memberSyntax = syntax.Expression as MemberSyntax;
            if (memberSyntax != null)
            {
                // We need to get a hold on the object we need to execute on.
                // This applies to an index and property. The target is stored in
                // a local and both the getter and the ExecuteFunction is this
                // local.

                var targetAssignment = BuildExpression(memberSyntax.Expression);

                if (targetAssignment is BoundGetVariable)
                {
                    target = targetAssignment;
                }
                else
                {
                    var targetTemporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        targetTemporary,
                        targetAssignment
                    ));

                    target = new BoundGetVariable(targetTemporary);
                }

                getter = BuildGetMember(
                    target,
                    memberSyntax.Type == SyntaxType.Property
                        ? BoundConstant.Create(((PropertySyntax)memberSyntax).Name)
                        : BuildGet(((IndexerSyntax)memberSyntax).Index)
                );
            }
            else
            {
                var identifierSyntax = syntax.Expression as IdentifierSyntax;

                if (
                    identifierSyntax != null &&
                    identifierSyntax.Target.Type == VariableType.WithScope
                )
                {
                    // With a with scope, the target depends on how the variable
                    // is resolved.

                    var withTarget = builder.CreateTemporary();
                    var method = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        method,
                        BuildGet(identifierSyntax, withTarget)
                    ));

                    getter = new BoundGetVariable(method);

                    target = new BoundGetVariable(withTarget);
                }
                else
                {
                    // Else we execute the function against the global scope.

                    target = new BoundGetVariable(BoundMagicVariable.Global);

                    getter = BuildGet(syntax.Expression);
                }
            }

            var resultTemporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                resultTemporary,
                new BoundCall(
                    target,
                    getter,
                    syntax.Arguments.Select(p =>
                        new BoundCallArgument(
                            BuildExpression(p.Expression),
                            p.IsRef
                        )
                    ).ToReadOnlyArray(),
                    syntax.Generics.Select(BuildExpression).ToReadOnlyArray()
                )
            ));

            return builder.BuildExpression(resultTemporary);
        }

        public BoundNode VisitNew(NewSyntax syntax)
        {
            var methodCall = syntax.Expression as MethodCallSyntax;

            ExpressionSyntax expression = syntax.Expression;
            var arguments = ReadOnlyArray<BoundCallArgument>.Empty;
            var generics = ReadOnlyArray<BoundExpression>.Empty;

            if (methodCall != null)
            {
                expression = methodCall.Expression;

                arguments = methodCall.Arguments.Select(p =>
                    new BoundCallArgument(
                        BuildExpression(p.Expression),
                        p.IsRef
                    )
                ).ToReadOnlyArray();

                generics = methodCall.Generics.Select(BuildExpression).ToReadOnlyArray();
            }

            return new BoundNew(
                BuildExpression(expression),
                arguments,
                generics
            );
        }

        public BoundNode VisitProgram(ProgramSyntax syntax)
        {
            Debug.Assert(Program == null);

            _scope = new Scope(_scope, syntax.Body);

            Program = new BoundProgram(
                VisitBody(syntax.Body)
            );

            _scope = _scope.Parent;

            return null;
        }

        public BoundNode VisitProperty(PropertySyntax syntax)
        {
            return BuildGet(syntax);
        }

        public BoundNode VisitRegexp(RegexpSyntax syntax)
        {
            return new BoundRegex(syntax.Regexp, syntax.Options);
        }

        public BoundNode VisitReturn(ReturnSyntax syntax)
        {
            return new BoundReturn(
                syntax.Expression != null ? BuildExpression(syntax.Expression) : null
            );
        }

        public BoundNode VisitSwitch(SwitchSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                BuildExpression(syntax.Expression)
            ));

            builder.Add(new BoundSwitch(
                temporary,
                syntax.Cases.Select(p => new BoundSwitchCase(
                    p.Expression != null ? BuildExpression(p.Expression) : null,
                    p.Body != null ? BuildBlock(p.Body) : null
                )).ToReadOnlyArray()
            ));

            return builder.BuildBlock();
        }

        public BoundNode VisitTernary(TernarySyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundIf(
                BuildExpression(syntax.Test),
                BuildBlock(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Then)
                )),
                BuildBlock(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Else)
                ))
            ));

            return builder.BuildExpression(temporary);
        }

        public BoundNode VisitThrow(ThrowSyntax syntax)
        {
            return new BoundThrow(
                BuildExpression(syntax.Expression)
            );
        }

        public BoundNode VisitTry(TrySyntax syntax)
        {
            return new BoundTry(
                BuildBlock(syntax.Body),
                syntax.Catch == null ? null : new BoundCatch(
                    _scope.GetWritable(syntax.Catch.Target),
                    BuildBlock(syntax.Catch.Body)
                ),
                syntax.Finally == null ? null : new BoundFinally(
                    BuildBlock(syntax.Finally.Body)
                )
            );
        }

        public BoundNode VisitUnary(UnarySyntax syntax)
        {
            switch (syntax.Operation)
            {
                case SyntaxExpressionType.PreIncrementAssign:
                case SyntaxExpressionType.PreDecrementAssign:
                case SyntaxExpressionType.PostIncrementAssign:
                case SyntaxExpressionType.PostDecrementAssign:
                    var builder = new BlockBuilder(this);

                    var temporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        temporary,
                        BuildExpression(syntax.Operand)
                    ));

                    bool before =
                        syntax.Operation == SyntaxExpressionType.PreIncrementAssign ||
                        syntax.Operation == SyntaxExpressionType.PreDecrementAssign;

                    int offset =
                        syntax.Operation == SyntaxExpressionType.PreIncrementAssign ||
                        syntax.Operation == SyntaxExpressionType.PostIncrementAssign
                        ? 1
                        : -1;

                    if (before)
                    {
                        builder.Add(new BoundSetVariable(
                            temporary,
                            new BoundBinary(
                                BoundExpressionType.Add,
                                new BoundGetVariable(temporary),
                                BoundConstant.Create(offset)
                            )
                        ));

                        builder.Add(BuildSet(
                            syntax.Operand,
                            new BoundGetVariable(temporary)
                        ));
                    }
                    else
                    {
                        builder.Add(BuildSet(
                            syntax.Operand,
                            new BoundBinary(
                                BoundExpressionType.Add,
                                new BoundGetVariable(temporary),
                                BoundConstant.Create(offset)
                            )
                        ));
                    }

                    return builder.BuildExpression(temporary);

                case SyntaxExpressionType.Delete:
                    return BuildDeleteMember(syntax.Operand);

                default:
                    BoundExpressionType type;

                    switch (syntax.Operation)
                    {
                        case SyntaxExpressionType.BitwiseNot: type = BoundExpressionType.BitwiseNot; break;
                        case SyntaxExpressionType.Negate: type = BoundExpressionType.Negate; break;
                        case SyntaxExpressionType.UnaryPlus: type = BoundExpressionType.UnaryPlus; break;
                        case SyntaxExpressionType.Not: type = BoundExpressionType.Not; break;
                        case SyntaxExpressionType.Delete:
                        case SyntaxExpressionType.TypeOf: type = BoundExpressionType.TypeOf; break;
                        case SyntaxExpressionType.Void: type = BoundExpressionType.Void; break;
                        default: throw new InvalidOperationException();
                    }

                    return new BoundUnary(
                        type,
                        BuildExpression(syntax.Operand)
                    );
            }
        }

        private BoundNode BuildDeleteMember(ExpressionSyntax syntax)
        {
            switch (syntax.Type)
            {
                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return new BoundDeleteMember(
                        BuildExpression(property.Expression),
                        BoundConstant.Create(property.Name)
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return new BoundDeleteMember(
                        BuildExpression(indexer.Expression),
                        BuildExpression(indexer.Index)
                    );

                case SyntaxType.Identifier:
                    var identifier = (IdentifierSyntax)syntax;

                    if (identifier.Target.Type == VariableType.Global)
                    {
                        return new BoundDeleteMember(
                            new BoundGetVariable(BoundMagicVariable.Global),
                            BoundConstant.Create(identifier.Name)
                        );
                    }
                    else
                    {
                        // Locals are never configurable.

                        return BoundConstant.Create(false);
                    }

                default:
                    var builder = new BlockBuilder(this);

                    builder.Add(new BoundExpressionStatement(
                        BuildExpression(syntax)
                    ));

                    var temporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        temporary,
                        BoundConstant.Create(true)
                    ));

                    return builder.BuildExpression(temporary);
            }
        }

        public BoundNode VisitValue(ValueSyntax syntax)
        {
            return BoundConstant.Create(syntax.Value);
        }

        public BoundNode VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            foreach (var declaration in syntax.Declarations)
            {
                if (declaration.Expression != null)
                {
                    builder.Add(BuildSet(
                        declaration.Target,
                        BuildExpression(declaration.Expression)
                    ));
                }
            }

            return builder.BuildBlock();
        }

        public BoundNode VisitWhile(WhileSyntax syntax)
        {
            return new BoundWhile(
                BuildExpression(syntax.Test),
                BuildBlock(syntax.Body)
            );
        }

        public BoundNode VisitWith(WithSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                BuildExpression(syntax.Expression)
            ));

            _withTemporaries.Add(syntax.Target, temporary);

            builder.Add(BuildBlock(syntax.Expression));

            _withTemporaries.Remove(syntax.Target);

            return builder.BuildBlock();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Compiler;
using Jint.Expressions;

namespace Jint.Bound
{
    internal partial class BindingVisitor : ISyntaxVisitor<BoundNode>
    {
#if DEBUG
        private const string WithPrefix = "__with";
#else
        private const string WithPrefix = "<>with";
#endif

        private readonly IScriptBuilder _scriptBuilder;
        private readonly List<BoundFunction> _functions = new List<BoundFunction>();
        private readonly Dictionary<Variable, IBoundReadable> _withVariables = new Dictionary<Variable, IBoundReadable>();
        private readonly Dictionary<FunctionSyntax, string> _functionNameHints = new Dictionary<FunctionSyntax, string>();

        private Scope _scope;

        public BoundProgram Program { get; private set; }

        public BindingVisitor(IScriptBuilder scriptBuilder)
        {
            if (scriptBuilder == null)
                throw new ArgumentNullException("scriptBuilder");

            _scriptBuilder = scriptBuilder;
        }

        public BoundNode VisitArrayDeclaration(ArrayDeclarationSyntax syntax)
        {
            var creation = new BoundNewBuiltIn(BoundNewBuiltInType.Array);

            if (syntax.Parameters.Count == 0)
                return creation;

            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                creation,
                SourceLocation.Missing
            ));

            for (int i = 0; i < syntax.Parameters.Count; i++)
            {
                builder.Add(BuildSetMember(
                    new BoundGetVariable(temporary),
                    BoundConstant.Create(i),
                    BuildExpression(syntax.Parameters[i])
                ));
            }

            return builder.BuildExpression(temporary, SourceLocation.Missing);
        }

        public BoundNode VisitAssignment(AssignmentSyntax syntax)
        {
            var identifier = syntax.Left as IdentifierSyntax;
            if (identifier != null)
            {
                switch (identifier.Target.Type)
                {
                    case VariableType.Undefined:
                    case VariableType.Arguments:
                        var builder = new BlockBuilder(this);

                        var temporary = builder.CreateTemporary();

                        builder.Add((BoundStatement)VisitExpressionStatement(
                            new ExpressionStatementSyntax(syntax.Right, SourceLocation.Missing)
                        ));

                        builder.Add(new BoundSetVariable(
                            temporary,
                            new BoundGetVariable(BoundMagicVariable.Undefined),
                            SourceLocation.Missing
                        ));

                        return builder.BuildExpression(temporary, SourceLocation.Missing);

                    case VariableType.This:
                    case VariableType.Null:
                        return BuildThrow("ReferenceError", "Invalid left-hand side in assignment");
                }
            }

            if (syntax.Operation == AssignmentOperator.Assign)
            {
                // Name anonymous function with the name if the identifier its
                // initialized to.

                if (identifier != null)
                {
                    var function = syntax.Right as FunctionSyntax;
                    if (function != null && function.Name == null)
                        _functionNameHints.Add(function, identifier.Name);
                }

                var builder = new BlockBuilder(this);

                var temporary = builder.CreateTemporary();

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Right),
                    SourceLocation.Missing
                ));

                builder.Add(BuildSet(
                    syntax.Left,
                    new BoundGetVariable(temporary)
                ));

                return builder.BuildExpression(temporary, SourceLocation.Missing);
            }
            else
            {
                var builder = new BlockBuilder(this);

                var temporary = builder.CreateTemporary();

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Left),
                    SourceLocation.Missing
                ));

                builder.Add(new BoundSetVariable(
                    temporary,
                    BuildBinary(
                        AssignmentSyntax.GetSyntaxType(syntax.Operation),
                        new BoundGetVariable(temporary),
                        BuildExpression(syntax.Right)
                    ),
                    SourceLocation.Missing
                ));

                builder.Add(BuildSet(
                    syntax.Left,
                    new BoundGetVariable(temporary)
                ));

                return builder.BuildExpression(temporary, SourceLocation.Missing);
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
                            left,
                            SourceLocation.Missing
                        ));

                        builder.Add(new BoundIf(
                            new BoundGetVariable(leftTemporary),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                right,
                                SourceLocation.Missing
                            )),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                new BoundGetVariable(leftTemporary),
                                SourceLocation.Missing
                            )),
                            SourceLocation.Missing
                        ));
                    }
                    else
                    {
                        builder.Add(new BoundSetVariable(
                            leftTemporary,
                            left,
                            SourceLocation.Missing
                        ));

                        builder.Add(new BoundIf(
                            new BoundGetVariable(leftTemporary),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                new BoundGetVariable(leftTemporary),
                                SourceLocation.Missing
                            )),
                            BuildBlock(new BoundSetVariable(
                                resultTemporary,
                                right,
                                SourceLocation.Missing
                            )),
                            SourceLocation.Missing
                        ));
                    }

                    return builder.BuildExpression(resultTemporary, SourceLocation.Missing);

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
            return BuildBlock(syntax);
        }

        public BoundNode VisitBreak(BreakSyntax syntax)
        {
            return new BoundBreak(syntax.Target, syntax.Location);
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
                        (BoundExpression)expression.Accept(this),
                        SourceLocation.Missing
                    ));
                }
                else
                {
                    builder.Add(new BoundExpressionStatement(
                        (BoundExpression)expression.Accept(this),
                        SourceLocation.Missing
                    ));
                }
            }

            return builder.BuildExpression(resultTemporary, SourceLocation.Missing);
        }

        public BoundNode VisitContinue(ContinueSyntax syntax)
        {
            return new BoundContinue(syntax.Target, syntax.Location);
        }

        public BoundNode VisitDoWhile(DoWhileSyntax syntax)
        {
            return new BoundDoWhile(
                BuildExpression(syntax.Test),
                BuildBlock(syntax.Body),
                syntax.Location
            );
        }

        public BoundNode VisitEmpty(EmptySyntax syntax)
        {
            return new BoundEmpty(SourceLocation.Missing);
        }

        public BoundNode VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            return BuildBlock(syntax.Expression, syntax.Location);
        }

        public BoundNode VisitFor(ForSyntax syntax)
        {
            return new BoundFor(
                syntax.Initialization != null ? BuildBlock(syntax.Initialization) : null,
                syntax.Test != null ? BuildExpression(syntax.Test) : null,
                syntax.Increment != null ? BuildBlock(syntax.Increment) : null,
                BuildBlock(syntax.Body),
                // Don't emit the location because the separate parts of the for
                // statement are the locations.
                SourceLocation.Missing
            );
        }

        private SourceLocation GetLocation(SyntaxNode syntax)
        {
            var hasLocation = syntax as ISourceLocation;
            if (hasLocation != null)
                return hasLocation.Location;
            return SourceLocation.Missing;
        }

        public BoundNode VisitForEachIn(ForEachInSyntax syntax)
        {
            return new BoundForEachIn(
                _scope.GetWritable(syntax.Target),
                BuildExpression(syntax.Expression),
                BuildBlock(syntax.Body),
                syntax.Location
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
                functionObject,
                SourceLocation.Missing
            ));

            builder.Add(BuildSet(
                syntax.Target,
                new BoundGetVariable(temporary)
            ));

            return builder.BuildExpression(temporary, syntax.Location);
        }

        public BoundFunction DeclareFunction(FunctionSyntax syntax)
        {
            _scope = new Scope(_scope, syntax.Body, _scriptBuilder);

            string name = syntax.Name;
            if (name == null)
                _functionNameHints.TryGetValue(syntax, out name);

            var function = new BoundFunction(
                name,
                syntax.Parameters.ToReadOnlyArray(),
                VisitBody(syntax.Body),
                syntax.Location ?? SourceLocation.Missing
            );

            _scope = _scope.Parent;

            return function;
        }

        private BoundBody VisitBody(BodySyntax syntax)
        {
            // Find the scoped closure; the function is going to be built on
            // that.

            BoundClosure scopedClosure = null;
            var scope = _scope;

            while (scope != null)
            {
                if (scope.Closure != null)
                {
                    scopedClosure = scope.Closure;
                    break;
                }

                scope = scope.Parent;
            }

            var body = BuildBlock(syntax);

            var flags = BoundBodyFlags.None;
            if (syntax.IsStrict)
                flags |= BoundBodyFlags.Strict;

            var mappedArguments = ReadOnlyArray<BoundMappedArgument>.Null;

            if (_scope.IsArgumentsReferenced)
            {
                flags |= BoundBodyFlags.ArgumentsReferenced;

                // Is the arguments closed over?
                if (_scope.GetArguments().Any(p => p.Closure != null))
                {
                    _scope.Closure.AddField(
                        _scope.TypeManager.CreateType(Closure.ArgumentsFieldName, BoundTypeKind.ClosureField)
                    );
                }
            }
            else
            {
                var builder = new ReadOnlyArray<BoundMappedArgument>.Builder();

                foreach (var argument in _scope.GetArguments())
                {
                    BoundVariable writable;

                    if (argument.Closure != null)
                    {
                        writable = _scope.Closure.AddField(
                            _scope.TypeManager.CreateType(argument.Name, BoundTypeKind.ClosureField)
                        );
                    }
                    else
                    {
                        writable = new BoundLocal(
                            true,
                            _scope.TypeManager.CreateType(argument.Name, BoundTypeKind.Local)
                        );
                    }

                    builder.Add(new BoundMappedArgument(argument, writable));
                }

                if (builder.Count > 0)
                    mappedArguments = builder.ToReadOnly();
            }

            return new BoundBody(
                body,
                _scope.Closure,
                scopedClosure,
                _scope.GetArguments().ToReadOnlyArray(),
                _scope.GetLocals().ToReadOnlyArray(),
                mappedArguments,
                flags,
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
                syntax.Else == null ? null : BuildBlock(syntax.Else),
                syntax.Location
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

            builder.Add(new BoundSetVariable(temporary, creation, SourceLocation.Missing));

            foreach (var property in syntax.Properties)
            {
                var dataProperty = property as JsonDataProperty;
                if (dataProperty != null)
                {
                    builder.Add(new BoundSetMember(
                        new BoundGetVariable(temporary),
                        BoundConstant.Create(dataProperty.Name),
                        BuildExpression(dataProperty.Expression), SourceLocation.Missing));
                }
                else
                {
                    var accessorProperty = (JsonAccessorProperty)property;

                    builder.Add(new BoundSetAccessor(
                        new BoundGetVariable(temporary),
                        accessorProperty.Name,
                        accessorProperty.GetExpression != null ? BuildExpression(accessorProperty.GetExpression) : null,
                        accessorProperty.SetExpression != null ? BuildExpression(accessorProperty.SetExpression) : null,
                        SourceLocation.Missing
                    ));
                }
            }

            return builder.BuildExpression(temporary, SourceLocation.Missing);
        }

        public BoundNode VisitLabel(LabelSyntax syntax)
        {
            return new BoundLabel(
                syntax.Label,
                (BoundStatement)syntax.Expression.Accept(this),
                SourceLocation.Missing
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
                        targetAssignment,
                        SourceLocation.Missing
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

                    builder.Add(new BoundSetVariable(
                        withTarget,
                        new BoundGetVariable(BoundMagicVariable.Global),
                        SourceLocation.Missing
                    ));

                    var method = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        method,
                        BuildGet(identifierSyntax, withTarget),
                        SourceLocation.Missing
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
                ),
                SourceLocation.Missing
            ));

            return builder.BuildExpression(resultTemporary, SourceLocation.Missing);
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

            _scope = new Scope(_scope, syntax.Body, _scriptBuilder);

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
            return new BoundRegEx(syntax.Regexp, syntax.Options);
        }

        public BoundNode VisitReturn(ReturnSyntax syntax)
        {
            return new BoundReturn(
                syntax.Expression != null ? BuildExpression(syntax.Expression) : null,
                syntax.Location
            );
        }

        public BoundNode VisitSwitch(SwitchSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundSetVariable(
                temporary,
                BuildExpression(syntax.Expression),
                SourceLocation.Missing
            ));

            builder.Add(new BoundSwitch(
                temporary,
                syntax.Cases.Select(p => new BoundSwitchCase(
                    p.Expression != null ? BuildExpression(p.Expression) : null,
                    p.Body != null ? BuildBlock(p.Body) : null,
                    syntax.Location
                )).ToReadOnlyArray(),
                syntax.Location
            ));

            return builder.BuildBlock(SourceLocation.Missing);
        }

        public BoundNode VisitTernary(TernarySyntax syntax)
        {
            var builder = new BlockBuilder(this);

            var temporary = builder.CreateTemporary();

            builder.Add(new BoundIf(
                BuildExpression(syntax.Test),
                BuildBlock(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Then),
                    SourceLocation.Missing
                )),
                BuildBlock(new BoundSetVariable(
                    temporary,
                    BuildExpression(syntax.Else),
                    SourceLocation.Missing
                )),
                SourceLocation.Missing
            ));

            return builder.BuildExpression(temporary, SourceLocation.Missing);
        }

        public BoundNode VisitThrow(ThrowSyntax syntax)
        {
            return new BoundThrow(
                BuildExpression(syntax.Expression),
                syntax.Location
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
                ),
                SourceLocation.Missing
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

                    bool before =
                        syntax.Operation == SyntaxExpressionType.PreIncrementAssign ||
                        syntax.Operation == SyntaxExpressionType.PreDecrementAssign;

                    int offset =
                        syntax.Operation == SyntaxExpressionType.PreIncrementAssign ||
                        syntax.Operation == SyntaxExpressionType.PostIncrementAssign
                        ? 1
                        : -1;

                    var resultTemporary = builder.CreateTemporary();

                    if (before)
                    {
                        builder.Add(new BoundSetVariable(
                            resultTemporary,
                            new BoundBinary(
                                BoundExpressionType.Add,
                                BuildExpression(syntax.Operand),
                                BoundConstant.Create(offset)
                            ),
                            SourceLocation.Missing
                        ));

                        builder.Add(BuildSet(
                            syntax.Operand,
                            new BoundGetVariable(resultTemporary)
                        ));
                    }
                    else
                    {
                        builder.Add(new BoundSetVariable(
                            resultTemporary,
                            BuildExpression(syntax.Operand),
                            SourceLocation.Missing
                        ));

                        builder.Add(BuildSet(
                            syntax.Operand,
                            new BoundBinary(
                                BoundExpressionType.Add,
                                new BoundGetVariable(resultTemporary),
                                BoundConstant.Create(offset)
                            )
                        ));
                    }

                    return builder.BuildExpression(resultTemporary, SourceLocation.Missing);

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
                        BuildExpression(syntax),
                        SourceLocation.Missing
                    ));

                    var temporary = builder.CreateTemporary();

                    builder.Add(new BoundSetVariable(
                        temporary,
                        BoundConstant.Create(true),
                        SourceLocation.Missing
                    ));

                    return builder.BuildExpression(temporary, SourceLocation.Missing);
            }
        }

        public BoundNode VisitValue(ValueSyntax syntax)
        {
            return BoundConstant.Create(syntax.Value);
        }

        public BoundNode VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var builder = new BlockBuilder(this);
            bool hadOne = false;

            foreach (var declaration in syntax.Declarations)
            {
                if (declaration.Expression != null)
                {
                    var function = declaration.Expression as FunctionSyntax;
                    if (function != null && function.Name == null)
                        _functionNameHints.Add(function, declaration.Identifier);

                    hadOne = true;
                    builder.Add(BuildSet(
                        declaration.Target,
                        BuildExpression(declaration.Expression)
                    ));
                }
            }

            if (!hadOne)
                return new BoundEmpty(SourceLocation.Missing);

            return builder.BuildBlock(syntax.Location);
        }

        public BoundNode VisitWhile(WhileSyntax syntax)
        {
            return new BoundWhile(
                BuildExpression(syntax.Test),
                BuildBlock(syntax.Body),
                syntax.Location
            );
        }

        public BoundNode VisitWith(WithSyntax syntax)
        {
            var builder = new BlockBuilder(this);

            IBoundWritable variable;
            if (syntax.Target.Closure != null)
                variable = _scope.GetClosureField(syntax.Target);
            else
                variable = _scope.GetLocal(syntax.Target);

            builder.Add(new BoundSetVariable(
                variable,
                BuildExpression(syntax.Expression),
                SourceLocation.Missing
            ));

            _withVariables.Add(syntax.Target, variable);

            builder.Add(BuildBlock(syntax.Body));

            _withVariables.Remove(syntax.Target);

            return builder.BuildBlock(syntax.Location);
        }
    }
}

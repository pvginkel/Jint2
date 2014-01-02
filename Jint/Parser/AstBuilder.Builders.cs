using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Parser
{
    partial class AstBuilder
    {
        public ArrayDeclarationSyntax BuildArrayDeclaration(ReadOnlyArray<SyntaxNode> parameters)
        {
            return new ArrayDeclarationSyntax(parameters);
        }

        public AssignmentSyntax BuildAssignment(AssignmentOperator operation, ExpressionSyntax left, ExpressionSyntax right)
        {
            return new AssignmentSyntax(operation, left, right);
        }

        public BinarySyntax BuildBinary(SyntaxExpressionType operation, ExpressionSyntax left, ExpressionSyntax right)
        {
            return new BinarySyntax(operation, left, right);
        }

        public BlockSyntax BuildBlock(ReadOnlyArray<SyntaxNode> statements)
        {
            return new BlockSyntax(statements);
        }

        public BreakSyntax BuildBreak(string target, SourceLocation location)
        {
            return new BreakSyntax(target, location);
        }

        public CommaOperatorSyntax BuildCommaOperator(ReadOnlyArray<ExpressionSyntax> expressions)
        {
            return new CommaOperatorSyntax(expressions);
        }

        public ExpressionSyntax BuildConstant(object value)
        {
            return new ValueSyntax(value);
        }

        public ContinueSyntax BuildContinue(string target, SourceLocation location)
        {
            return new ContinueSyntax(target, location);
        }

        public DoWhileSyntax BuildDoWhile(ExpressionSyntax condition, SyntaxNode statement, SourceLocation location)
        {
            return new DoWhileSyntax(condition, statement, location);
        }

        public EmptySyntax BuildEmpty()
        {
            return new EmptySyntax();
        }

        public ExpressionStatementSyntax BuildExpressionStatement(ExpressionSyntax expression, SourceLocation location)
        {
            return new ExpressionStatementSyntax(expression, location);
        }

        public FinallyClause BuildFinallyClause(SyntaxNode body)
        {
            return new FinallyClause(body);
        }

        public SyntaxNode BuildFor(SyntaxNode initialization, SyntaxNode test, SyntaxNode increment, SyntaxNode body, SourceLocation location)
        {
            return new ForSyntax(initialization, test, increment, body, location);
        }

        public SyntaxNode BuildForEachIn(IIdentifier identifier, ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
        {
            return new ForEachInSyntax(identifier, expression, body, location);
        }

        public FunctionSyntax BuildFunction(string name, ReadOnlyArray<string> parameters, BodySyntax body, SourceLocation location)
        {
            IIdentifier identifier = null;

            if (name != null)
            {
                identifier = _scope.CreateIdentifier(name);
                _scope.DeclareIdentifier(name);
            }

            return new FunctionSyntax(
                identifier,
                parameters,
                body,
                location
            );
        }

        public ExpressionSyntax BuildIdentifier(string name)
        {
            return new IdentifierSyntax(_scope.CreateIdentifier(name));
        }

        public IfSyntax BuildIf(ExpressionSyntax test, SyntaxNode then, SyntaxNode @else, SourceLocation location)
        {
            return new IfSyntax(test, then, @else, location);
        }

        public IndexerSyntax BuildIndexer(ExpressionSyntax expression, ExpressionSyntax index)
        {
            return new IndexerSyntax(expression, index);
        }

        public JsonExpressionSyntax BuildJsonExpression(ReadOnlyArray<JsonProperty> properties)
        {
            return new JsonExpressionSyntax(properties);
        }

        public LabelSyntax BuildLabel(string label, SyntaxNode expression)
        {
            return new LabelSyntax(label, expression);
        }

        public MethodArgument BuildMethodArgument(ExpressionSyntax expression, bool isRef)
        {
            return new MethodArgument(expression, isRef);
        }

        public ExpressionSyntax BuildMethodCall(ExpressionSyntax expression, ReadOnlyArray<MethodArgument> arguments, ReadOnlyArray<ExpressionSyntax> generics)
        {
            return new MethodCallSyntax(expression, arguments, generics);
        }

        public ExpressionSyntax BuildNew(ExpressionSyntax expression)
        {
            return new NewSyntax(expression);
        }

        public ProgramSyntax BuildProgram(BodySyntax body)
        {
            return new ProgramSyntax(body);
        }

        public PropertySyntax BuildProperty(ExpressionSyntax expression, string name)
        {
            return new PropertySyntax(expression, name);
        }

        public ExpressionSyntax BuildRegularExpression(string regexp, string options)
        {
            return new RegexpSyntax(regexp, options);
        }

        public ReturnSyntax BuildReturn(ExpressionSyntax expression, SourceLocation location)
        {
            return new ReturnSyntax(expression, location);
        }

        public SwitchSyntax BuildSwitch(SyntaxNode expression, ReadOnlyArray<SwitchCase> cases, SourceLocation location)
        {
            return new SwitchSyntax(expression, cases, location);
        }

        public SwitchCase BuildSwitchCase(ExpressionSyntax expression, BlockSyntax body, SourceLocation location)
        {
            return new SwitchCase(expression, body, location);
        }

        public TernarySyntax BuildTernary(ExpressionSyntax test, ExpressionSyntax then, ExpressionSyntax @else)
        {
            return new TernarySyntax(test, then, @else);
        }

        public ThrowSyntax BuildThrow(ExpressionSyntax expression, SourceLocation location)
        {
            return new ThrowSyntax(expression, location);
        }

        public TrySyntax BuildTry(SyntaxNode body, CatchClause @catch, FinallyClause @finally)
        {
            return new TrySyntax(body, @catch, @finally);
        }

        public UnarySyntax BuildUnary(SyntaxExpressionType operation, ExpressionSyntax operand)
        {
            return new UnarySyntax(operation, operand);
        }

        public VariableDeclarationSyntax BuildVariableDeclaration(ReadOnlyArray<VariableDeclaration> declarations, SourceLocation location)
        {
            return new VariableDeclarationSyntax(declarations, location);
        }

        public VariableDeclaration BuildVariableDeclaration(string identifier, ExpressionSyntax expression, bool global)
        {
            _scope.DeclareIdentifier(identifier);

            return new VariableDeclaration(
                _scope.CreateIdentifier(identifier),
                expression,
                global
            );
        }

        public WhileSyntax BuildWhile(ExpressionSyntax test, SyntaxNode body, SourceLocation location)
        {
            return new WhileSyntax(test, body, location);
        }
    }
}

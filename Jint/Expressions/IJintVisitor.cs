using System.Collections.Generic;
using Jint.Native;

namespace Jint.Expressions
{
    public interface ISyntaxVisitor
    {
        void VisitProgram(ProgramSyntax syntax);
        void VisitAssignment(AssignmentSyntax syntax);
        void VisitBlock(BlockSyntax syntax);
        void VisitBreak(BreakSyntax syntax);
        void VisitContinue(ContinueSyntax syntax);
        void VisitDoWhile(DoWhileSyntax syntax);
        void VisitEmpty(EmptySyntax syntax);
        void VisitExpressionStatement(ExpressionStatementSyntax syntax);
        void VisitForEachIn(ForEachInSyntax syntax);
        void VisitFor(ForSyntax syntax);
        void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax);
        void VisitIf(IfSyntax syntax);
        void VisitReturn(ReturnSyntax syntax);
        void VisitSwitch(SwitchSyntax syntax);
        void VisitWith(WithSyntax syntax);
        void VisitThrow(ThrowSyntax syntax);
        void VisitTry(TrySyntax syntax);
        void VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        void VisitWhile(WhileSyntax syntax);
        void VisitArrayDeclaration(ArrayDeclarationSyntax syntax);
        void VisitCommaOperator(CommaOperatorSyntax syntax);

        void VisitFunction(FunctionSyntax syntax);
        void VisitMethodCall(MethodCallSyntax syntax);
        void VisitIndexer(IndexerSyntax syntax);
        void VisitProperty(PropertySyntax syntax);
        void VisitIdentifier(IdentifierSyntax syntax);

        void VisitJsonExpression(JsonExpressionSyntax syntax);
        void VisitNew(NewSyntax syntax);
        void VisitBinaryExpression(BinaryExpressionSyntax syntax);
        void VisitTernary(TernarySyntax syntax);
        void VisitUnaryExpression(UnaryExpressionSyntax syntax);
        void VisitValue(ValueSyntax syntax);
        void VisitRegexp(RegexpSyntax syntax);

        void VisitLabel(LabelSyntax syntax);
    }

    public interface ISyntaxVisitor<out T>
    {
        T VisitProgram(ProgramSyntax syntax);
        T VisitAssignment(AssignmentSyntax syntax);
        T VisitBlock(BlockSyntax syntax);
        T VisitBreak(BreakSyntax syntax);
        T VisitContinue(ContinueSyntax syntax);
        T VisitDoWhile(DoWhileSyntax syntax);
        T VisitEmpty(EmptySyntax syntax);
        T VisitExpressionStatement(ExpressionStatementSyntax syntax);
        T VisitForEachIn(ForEachInSyntax syntax);
        T VisitFor(ForSyntax syntax);
        T VisitFunctionDeclaration(FunctionDeclarationSyntax syntax);
        T VisitIf(IfSyntax syntax);
        T VisitReturn(ReturnSyntax syntax);
        T VisitSwitch(SwitchSyntax syntax);
        T VisitWith(WithSyntax syntax);
        T VisitThrow(ThrowSyntax syntax);
        T VisitTry(TrySyntax syntax);
        T VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        T VisitWhile(WhileSyntax syntax);
        T VisitArrayDeclaration(ArrayDeclarationSyntax syntax);
        T VisitCommaOperator(CommaOperatorSyntax syntax);

        T VisitFunction(FunctionSyntax syntax);
        T VisitMethodCall(MethodCallSyntax syntax);
        T VisitIndexer(IndexerSyntax syntax);
        T VisitProperty(PropertySyntax syntax);
        T VisitIdentifier(IdentifierSyntax syntax);

        T VisitJsonExpression(JsonExpressionSyntax syntax);
        T VisitNew(NewSyntax syntax);
        T VisitBinaryExpression(BinaryExpressionSyntax syntax);
        T VisitTernary(TernarySyntax syntax);
        T VisitUnaryExpression(UnaryExpressionSyntax syntax);
        T VisitValue(ValueSyntax syntax);
        T VisitRegexp(RegexpSyntax syntax);
        T VisitLabel(LabelSyntax syntax);
    }

    public interface IJintVisitor
    {
        JsInstance Result { get; set; }
        JsObject CallTarget { get; }

        JsGlobal Global { get; }

        JsInstance Returned { get; }

        JsInstance Return(JsInstance result);

        void ExecuteFunction(JsObject function, JsInstance @this, JsInstance[] parameters);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    internal interface ISyntaxVisitor
    {
        void VisitArrayDeclaration(ArrayDeclarationSyntax syntax);
        void VisitAssignment(AssignmentSyntax syntax);
        void VisitBinary(BinarySyntax syntax);
        void VisitBlock(BlockSyntax syntax);
        void VisitBreak(BreakSyntax syntax);
        void VisitCommaOperator(CommaOperatorSyntax syntax);
        void VisitContinue(ContinueSyntax syntax);
        void VisitDoWhile(DoWhileSyntax syntax);
        void VisitEmpty(EmptySyntax syntax);
        void VisitExpressionStatement(ExpressionStatementSyntax syntax);
        void VisitFor(ForSyntax syntax);
        void VisitForEachIn(ForEachInSyntax syntax);
        void VisitFunction(FunctionSyntax syntax);
        void VisitIdentifier(IdentifierSyntax syntax);
        void VisitIf(IfSyntax syntax);
        void VisitIndexer(IndexerSyntax syntax);
        void VisitJsonExpression(JsonExpressionSyntax syntax);
        void VisitLabel(LabelSyntax syntax);
        void VisitMethodCall(MethodCallSyntax syntax);
        void VisitNew(NewSyntax syntax);
        void VisitProgram(ProgramSyntax syntax);
        void VisitProperty(PropertySyntax syntax);
        void VisitRegexp(RegexpSyntax syntax);
        void VisitReturn(ReturnSyntax syntax);
        void VisitSwitch(SwitchSyntax syntax);
        void VisitTernary(TernarySyntax syntax);
        void VisitThrow(ThrowSyntax syntax);
        void VisitTry(TrySyntax syntax);
        void VisitUnary(UnarySyntax syntax);
        void VisitValue(ValueSyntax syntax);
        void VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        void VisitWhile(WhileSyntax syntax);
        void VisitWith(WithSyntax syntax);
    }

    internal interface ISyntaxVisitor<out T>
    {
        T VisitArrayDeclaration(ArrayDeclarationSyntax syntax);
        T VisitAssignment(AssignmentSyntax syntax);
        T VisitBinaryExpression(BinarySyntax syntax);
        T VisitBlock(BlockSyntax syntax);
        T VisitBreak(BreakSyntax syntax);
        T VisitCommaOperator(CommaOperatorSyntax syntax);
        T VisitContinue(ContinueSyntax syntax);
        T VisitDoWhile(DoWhileSyntax syntax);
        T VisitEmpty(EmptySyntax syntax);
        T VisitExpressionStatement(ExpressionStatementSyntax syntax);
        T VisitFor(ForSyntax syntax);
        T VisitForEachIn(ForEachInSyntax syntax);
        T VisitFunction(FunctionSyntax syntax);
        T VisitIdentifier(IdentifierSyntax syntax);
        T VisitIf(IfSyntax syntax);
        T VisitIndexer(IndexerSyntax syntax);
        T VisitJsonExpression(JsonExpressionSyntax syntax);
        T VisitLabel(LabelSyntax syntax);
        T VisitMethodCall(MethodCallSyntax syntax);
        T VisitNew(NewSyntax syntax);
        T VisitProgram(ProgramSyntax syntax);
        T VisitProperty(PropertySyntax syntax);
        T VisitRegexp(RegexpSyntax syntax);
        T VisitReturn(ReturnSyntax syntax);
        T VisitSwitch(SwitchSyntax syntax);
        T VisitTernary(TernarySyntax syntax);
        T VisitThrow(ThrowSyntax syntax);
        T VisitTry(TrySyntax syntax);
        T VisitUnaryExpression(UnarySyntax syntax);
        T VisitValue(ValueSyntax syntax);
        T VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        T VisitWhile(WhileSyntax syntax);
        T VisitWith(WithSyntax syntax);
    }
}

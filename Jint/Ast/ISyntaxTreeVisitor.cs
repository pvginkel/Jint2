using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal interface ISyntaxTreeVisitor<out T>
    {
        T VisitArrayDeclaration(ArrayDeclarationSyntax syntax);
        T VisitAssignment(AssignmentSyntax syntax);
        T VisitBinary(BinarySyntax syntax);
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
        T VisitUnary(UnarySyntax syntax);
        T VisitValue(ValueSyntax syntax);
        T VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        T VisitWhile(WhileSyntax syntax);
        T VisitWith(WithSyntax syntax);
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Jint.Expressions;

//namespace Jint.Backend.Dlr
//{
//    internal class TypeMarkerPhase : SyntaxVisitor
//    {
//        public override void VisitAssignment(AssignmentSyntax syntax)
//        {
//            var identifier = syntax.Left as IdentifierSyntax;

//            if (identifier != null)
//                MarkAssign(identifier.Target, DetermineType(syntax.Right));

//            base.VisitAssignment(syntax);
//        }

//        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
//        {
//            MarkAssign(syntax.Target, VariableType.Js);

//            base.VisitFunctionDeclaration(syntax);
//        }

//        public override void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
//        {
//            if (syntax.Expression != null)
//                MarkAssign(syntax.Target, VariableType.Js);

//            base.VisitVariableDeclaration(syntax);
//        }

//        private VariableType DetermineType(ExpressionSyntax syntax)
//        {
//            switch (syntax.Type)
//            {
//                case SyntaxType.ArrayDeclaration: return VariableType.Array;
//                case SyntaxType.Assignment: return DetermineType(((AssignmentSyntax)syntax).Right);
//                case SyntaxType.Binary:
//                    var binaryExpression = (BinaryExpressionSyntax)syntax;

//                    return CombineType(
//                        DetermineType(binaryExpression.Left),
//                        DetermineType(binaryExpression.Right),

//            }
//        }

//        private void MarkAssign(Variable variable, VariableType variableType)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

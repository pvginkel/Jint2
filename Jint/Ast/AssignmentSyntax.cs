using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class AssignmentSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Assignment; }
        }

        public AssignmentOperator Operation { get; private set; }
        public ExpressionSyntax Left { get; private set; }
        public ExpressionSyntax Right { get; private set; }

        public AssignmentSyntax(AssignmentOperator operation, ExpressionSyntax left, ExpressionSyntax right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            Operation = operation;
            Left = left;
            Right = right;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitAssignment(this);
        }

        public static SyntaxExpressionType GetSyntaxType(AssignmentOperator operation)
        {
            switch (operation)
            {
                case AssignmentOperator.Add: return SyntaxExpressionType.Add;
                case AssignmentOperator.BitwiseAnd: return SyntaxExpressionType.BitwiseAnd;
                case AssignmentOperator.Divide: return SyntaxExpressionType.Divide;
                case AssignmentOperator.Modulo: return SyntaxExpressionType.Modulo;
                case AssignmentOperator.Multiply: return SyntaxExpressionType.Multiply;
                case AssignmentOperator.BitwiseOr: return SyntaxExpressionType.BitwiseOr;
                case AssignmentOperator.LeftShift: return SyntaxExpressionType.LeftShift;
                case AssignmentOperator.RightShift: return SyntaxExpressionType.RightShift;
                case AssignmentOperator.Subtract: return SyntaxExpressionType.Subtract;
                case AssignmentOperator.UnsignedRightShift: return SyntaxExpressionType.UnsignedRightShift;
                case AssignmentOperator.BitwiseExclusiveOr: return SyntaxExpressionType.BitwiseExclusiveOr;
                default: throw new InvalidOperationException();
            }
        }
    }
}

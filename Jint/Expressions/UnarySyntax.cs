using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class UnarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Unary; }
        }

        public ExpressionSyntax Operand { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

        public override ValueType ValueType
        {
            get
            {
                switch (Operation)
                {
                    case SyntaxExpressionType.BitwiseNot:
                    case SyntaxExpressionType.Negate:
                    case SyntaxExpressionType.UnaryPlus:
                    case SyntaxExpressionType.PreIncrementAssign:
                    case SyntaxExpressionType.PreDecrementAssign:
                        return ValueType.Double;

                    case SyntaxExpressionType.Not:
                    case SyntaxExpressionType.Delete:
                        return ValueType.Boolean;

                    case SyntaxExpressionType.TypeOf:
                        return ValueType.String;

                    case SyntaxExpressionType.PostIncrementAssign:
                    case SyntaxExpressionType.PostDecrementAssign:
                        return Operand.ValueType;

                    case SyntaxExpressionType.Void:
                        return ValueType.Unknown;

                    default:
                        throw new ArgumentOutOfRangeException("operand");
                }
            }
        }

        public UnarySyntax(SyntaxExpressionType operation, ExpressionSyntax operand)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");

            Operation = operation;
            Operand = operand;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitUnary(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
}

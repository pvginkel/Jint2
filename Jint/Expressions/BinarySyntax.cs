using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class BinarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Binary; }
        }

        public ExpressionSyntax Left { get; private set; }
        public ExpressionSyntax Right { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

        internal override ValueType ValueType
        {
            get { return ResolveValueType(Operation, Left.ValueType, Right.ValueType); }
        }

        internal static ValueType ResolveValueType(SyntaxExpressionType operation, ValueType left, ValueType right)
        {
            switch (operation)
            {
                case SyntaxExpressionType.And:
                case SyntaxExpressionType.Or:
                    if (left == right)
                        return left;

                    return ValueType.Unknown;

                case SyntaxExpressionType.Add:
                    if (left == ValueType.String || right == ValueType.String)
                        return ValueType.String;

                    return ValueType.Double;

                case SyntaxExpressionType.BitwiseAnd:
                case SyntaxExpressionType.BitwiseExclusiveOr:
                case SyntaxExpressionType.BitwiseOr:
                case SyntaxExpressionType.Divide:
                case SyntaxExpressionType.LeftShift:
                case SyntaxExpressionType.RightShift:
                case SyntaxExpressionType.UnsignedRightShift:
                case SyntaxExpressionType.Modulo:
                case SyntaxExpressionType.Multiply:
                case SyntaxExpressionType.Power:
                case SyntaxExpressionType.Subtract:
                    return ValueType.Double;

                case SyntaxExpressionType.Equal:
                case SyntaxExpressionType.NotEqual:
                case SyntaxExpressionType.Same:
                case SyntaxExpressionType.NotSame:
                case SyntaxExpressionType.LessThan:
                case SyntaxExpressionType.LessThanOrEqual:
                case SyntaxExpressionType.GreaterThan:
                case SyntaxExpressionType.GreaterThanOrEqual:
                case SyntaxExpressionType.In:
                case SyntaxExpressionType.InstanceOf:
                    return ValueType.Boolean;

                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }

        public BinarySyntax(SyntaxExpressionType operation, ExpressionSyntax left, ExpressionSyntax right)
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
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitBinary(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        public override string ToString()
        {
            return Operation + " (" + Left + ", " + Right + ")";
        }
    }
}

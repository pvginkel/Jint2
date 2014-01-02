using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class BinarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Binary; }
        }

        public ExpressionSyntax Left { get; private set; }
        public ExpressionSyntax Right { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override string ToString()
        {
            return Operation + " (" + Left + ", " + Right + ")";
        }
    }
}

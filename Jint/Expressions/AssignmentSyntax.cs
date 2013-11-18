using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class AssignmentSyntax : ExpressionSyntax
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
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitAssignment(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitAssignment(this);
        }
    }
}

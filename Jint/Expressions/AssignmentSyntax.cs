using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class AssignmentSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; set; }
        public ExpressionSyntax Right { get; set; }
        public AssignmentOperator AssignmentOperator { get; set; }

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

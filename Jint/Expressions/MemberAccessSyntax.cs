using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    /// <summary>
    /// A MemberExpression represents an elements which applies on a previous Expression
    /// </summary>
    [Serializable]
    public class MemberAccessSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Member { get; set; }
        public ExpressionSyntax Previous { get; set; }

        public MemberAccessSyntax(ExpressionSyntax member, ExpressionSyntax previous)
        {
            Member = member;
            Previous = previous;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitMemberAccess(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitMemberAccess(this);
        }
    }
}

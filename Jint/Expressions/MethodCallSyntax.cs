using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class MethodCallSyntax : ExpressionSyntax, IGenericExpression
    {
        public MethodCallSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
            Arguments = new List<ExpressionSyntax>();
            Generics = new List<ExpressionSyntax>();
        }

        public MethodCallSyntax(ExpressionSyntax expression, IEnumerable<ExpressionSyntax> arguments)
            : this(expression)
        {
            Arguments.AddRange(arguments);
        }

        public ExpressionSyntax Expression { get; set; }
        public List<ExpressionSyntax> Arguments { get; set; }
        public List<ExpressionSyntax> Generics { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitMethodCall(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitMethodCall(this);
        }
    }
}

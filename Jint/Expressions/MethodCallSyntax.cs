using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Jint.Expressions
{
    public class MethodCallSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.MethodCall; }
        }

        public ExpressionSyntax Expression { get; private set; }
        public IList<MethodArgument> Arguments { get; private set; }
        public IList<ExpressionSyntax> Generics { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

        public MethodCallSyntax(ExpressionSyntax expression, IEnumerable<MethodArgument> arguments, IEnumerable<ExpressionSyntax> generics)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (generics == null)
                throw new ArgumentNullException("generics");

            Expression = expression;
            Arguments = arguments.ToReadOnly();
            Generics = generics.ToReadOnly();
        }

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

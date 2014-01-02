using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class MethodCallSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.MethodCall; }
        }

        public ExpressionSyntax Expression { get; private set; }
        public ReadOnlyArray<MethodArgument> Arguments { get; private set; }
        public ReadOnlyArray<ExpressionSyntax> Generics { get; private set; }

        public MethodCallSyntax(ExpressionSyntax expression, ReadOnlyArray<MethodArgument> arguments, ReadOnlyArray<ExpressionSyntax> generics)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (generics == null)
                throw new ArgumentNullException("generics");

            Expression = expression;
            Arguments = arguments;
            Generics = generics;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitMethodCall(this);
        }
    }
}

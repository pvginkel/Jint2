using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class FunctionSyntax : ExpressionSyntax, IFunctionDeclaration
    {
        public List<string> Parameters { get; set; }
        public SyntaxNode Body { get; set; }
        public string Name { get; set; }

        public FunctionSyntax()
        {
            Parameters = new List<string>();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitFunction(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunction(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class FunctionSyntax : ExpressionSyntax, IFunctionDeclaration
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Function; }
        }

        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }
        public BlockSyntax Body { get; private set; }

        public FunctionSyntax(string name, IEnumerable<string> parameters, BlockSyntax body)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Parameters = parameters.ToReadOnly();
            Body = body;
            Name = name;
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class FunctionSyntax : ExpressionSyntax, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Function; }
        }

        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }
        public BodySyntax Body { get; private set; }
        public Variable Target { get; private set; }
        public SourceLocation Location { get; private set; }

        public FunctionSyntax(string name, IEnumerable<string> parameters, BodySyntax body, Variable target, SourceLocation location)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Parameters = parameters.ToReadOnly();
            Body = body;
            Name = name;
            Target = target;
            Location = location;
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

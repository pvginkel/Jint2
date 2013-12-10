using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class FunctionSyntax : ExpressionSyntax, IFunctionDeclaration
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Function; }
        }

        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }
        public BlockSyntax Body { get; private set; }
        internal Variable Target { get; set; }
        public SourceLocation Location { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

        public FunctionSyntax(string name, IEnumerable<string> parameters, BlockSyntax body, SourceLocation location)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Parameters = parameters.ToReadOnly();
            Body = body;
            Name = name;
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

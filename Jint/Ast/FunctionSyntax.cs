using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class FunctionSyntax : ExpressionSyntax, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Function; }
        }

        public IIdentifier Identifier { get; private set; }
        public ReadOnlyArray<string> Parameters { get; private set; }
        public BodySyntax Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public FunctionSyntax(IIdentifier identifier, ReadOnlyArray<string> parameters, BodySyntax body, SourceLocation location)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Parameters = parameters;
            Body = body;
            Identifier = identifier;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitFunction(this);
        }
    }
}

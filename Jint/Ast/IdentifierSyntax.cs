using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class IdentifierSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Identifier; }
        }

        public override bool IsAssignable
        {
            get { return true; }
        }

        public IIdentifier Identifier { get; private set; }

        public IdentifierSyntax(IIdentifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            Identifier = identifier;
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}

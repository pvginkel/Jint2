using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Native;

namespace Jint.Expressions
{
    internal class IdentifierSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Identifier; }
        }

        public override bool IsAssignable
        {
            get
            {
                return
                    Name != JsNames.TypeUndefined &&
                    Name != JsNames.This &&
                    Name != JsNames.Arguments;
            }
        }

        public string Name { get; private set; }
        public Variable Target { get; set; }

        public IdentifierSyntax(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitIdentifier(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}

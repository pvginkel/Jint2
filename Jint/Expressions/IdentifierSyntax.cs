using System;
using System.Diagnostics;
using Jint.Native;

namespace Jint.Expressions
{
    [Serializable]
    public class IdentifierSyntax : ExpressionSyntax
    {
        public IdentifierSyntax(string name)
        {
            Name = name;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Identifier; }
        }

        internal override bool IsAssignable
        {
            get
            {
                return
                    Name != JsInstance.TypeUndefined &&
                    Name != JsScope.This &&
                    Name != JsScope.Arguments;
            }
        }

        public string Name { get; set; }
        public Variable Target { get; set; }

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

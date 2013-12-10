using System;
using System.Diagnostics;
using Jint.Native;

namespace Jint.Expressions
{
    public class IdentifierSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Identifier; }
        }

        internal override bool IsAssignable
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
        internal Variable Target { get; set; }

        internal override ValueType ValueType
        {
            get { return Target.ValueType; }
        }

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

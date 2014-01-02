using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class BodySyntax : BlockSyntax
    {
        public BodyType BodyType { get; private set; }
        public ReadOnlyArray<IIdentifier> Identifiers { get; private set; }
        public Closure Closure { get; private set; }
        public bool IsStrict { get; private set; }

        public BodySyntax(BodyType bodyType, ReadOnlyArray<SyntaxNode> statements, ReadOnlyArray<IIdentifier> identifiers, bool isStrict, Closure closure)
            : base(statements)
        {
            if (identifiers == null)
                throw new ArgumentNullException("identifiers");

            BodyType = bodyType;
            Identifiers = identifiers;
            IsStrict = isStrict;
            Closure = closure;
        }
    }
}

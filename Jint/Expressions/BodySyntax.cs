using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class BodySyntax : BlockSyntax
    {
        public BodyType BodyType { get; private set; }
        public VariableCollection DeclaredVariables { get; private set; }
        public Closure Closure { get; set; }
        public Closure ParentClosure { get; set; }

        public BodySyntax(BodyType bodyType, IEnumerable<SyntaxNode> statements, VariableCollection declaredVariables)
            : base(statements)
        {
            if (declaredVariables == null)
                throw new ArgumentNullException("declaredVariables");

            BodyType = bodyType;
            DeclaredVariables = declaredVariables;
        }
    }
}

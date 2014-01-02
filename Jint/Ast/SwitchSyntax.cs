using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class SwitchSyntax : SyntaxNode, ISourceLocation
    {
        public SyntaxNode Expression { get; private set; }
        public ReadOnlyArray<SwitchCase> Cases { get; private set; }
        public SourceLocation Location { get; private set; }

        public SwitchSyntax(SyntaxNode expression, ReadOnlyArray<SwitchCase> cases, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Cases = cases;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Switch; }
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitSwitch(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class SwitchSyntax : SyntaxNode, ISourceLocation
    {
        public SyntaxNode Expression { get; private set; }
        public IList<SwitchCase> Cases { get; private set; }
        public SourceLocation Location { get; private set; }

        public SwitchSyntax(SyntaxNode expression, IEnumerable<SwitchCase> cases, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Cases = cases.ToReadOnly();
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Switch; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitSwitch(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitSwitch(this);
        }
    }
}

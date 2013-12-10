using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class SwitchSyntax : SyntaxNode, ISourceLocation
    {
        public SyntaxNode Expression { get; private set; }
        public IList<CaseClause> Cases { get; private set; }
        public DefaultClause Default { get; private set; }
        public SourceLocation Location { get; private set; }

        public SwitchSyntax(SyntaxNode expression, IEnumerable<CaseClause> cases, DefaultClause @default, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Cases = cases.ToReadOnly();
            Default = @default;
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

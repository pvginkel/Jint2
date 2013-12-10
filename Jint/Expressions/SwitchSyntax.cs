using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class SwitchSyntax : SyntaxNode
    {
        public SyntaxNode Expression { get; private set; }
        public IList<CaseClause> Cases { get; private set; }
        public SyntaxNode Default { get; private set; }

        public SwitchSyntax(SyntaxNode expression, IEnumerable<CaseClause> cases, SyntaxNode @default)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
            Cases = cases.ToReadOnly();
            Default = @default;
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

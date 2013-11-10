using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class SwitchSyntax : SyntaxNode
    {
        public SyntaxNode Expression { get; set; }
        public List<CaseClause> Cases { get; private set; }
        public SyntaxNode Default { get; set; }

        public SwitchSyntax()
        {
            Cases = new List<CaseClause>();
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

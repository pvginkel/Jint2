using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class TrySyntax : SyntaxNode
    {
        public SyntaxNode Body { get; set; }
        public CatchClause Catch { get; set; }
        public FinallyClause Finally { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitTry(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitTry(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public abstract class MemberSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Expression { get; set; }
        internal override bool IsAssignable { get { return true; } }
    }
}

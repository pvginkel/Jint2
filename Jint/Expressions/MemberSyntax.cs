using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public abstract class MemberSyntax : ExpressionSyntax, IAssignable
    {
        public ExpressionSyntax Expression { get; set; }
    }
}

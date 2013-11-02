using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class RegexpSyntax : ExpressionSyntax
    {
        public string Regexp { get; set; }
        public string Options { get; set; }

        public RegexpSyntax(string regexp)
        {
            Regexp = regexp;
        }

        public RegexpSyntax(string regexp, string options)
            : this(regexp)
        {
            Options = options;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitRegexp(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitRegexp(this);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Native;

namespace Jint.Expressions
{
    public class RegexpSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Regexp; }
        }

        public string Regexp { get; private set; }
        public string Options { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Object; }
        }

        public RegexpSyntax(string regexp, string options)
        {
            if (regexp == null)
                throw new ArgumentNullException("regexp");
            if (options == null)
                throw new ArgumentNullException("options");

            Regexp = regexp;
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

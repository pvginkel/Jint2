using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundRegEx : BoundExpression
    {
        public string Regex { get; private set; }
        public string Options { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.RegEx; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Object; }
        }

        public BoundRegEx(string regex, string options)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");

            Regex = regex;
            Options = options;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitRegex(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitRegex(this);
        }

        public BoundRegEx Update(string regex, string options)
        {
            if (
                regex == Regex &&
                options == Options
            )
                return this;

            return new BoundRegEx(regex, options);
        }
    }
}

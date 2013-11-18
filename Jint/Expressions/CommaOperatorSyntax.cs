using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class CommaOperatorSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.CommaOperator; }
        }

        public IList<SyntaxNode> Expressions { get; private set; }

        public CommaOperatorSyntax(IEnumerable<SyntaxNode> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            Expressions = expressions.ToReadOnly();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitCommaOperator(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommaOperator(this);
        }
    }
}

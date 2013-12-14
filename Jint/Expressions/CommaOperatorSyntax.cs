using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class CommaOperatorSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.CommaOperator; }
        }

        public override bool IsLiteral
        {
            get { return Expressions.Count == 0 || (Expressions.Count == 1 && Expressions[0].IsLiteral); }
        }

        public IList<ExpressionSyntax> Expressions { get; private set; }

        public override ValueType ValueType
        {
            get { return Expressions[Expressions.Count - 1].ValueType; }
        }

        public CommaOperatorSyntax(IEnumerable<ExpressionSyntax> expressions)
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

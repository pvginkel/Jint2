using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class CommaOperatorSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.CommaOperator; }
        }

        public ReadOnlyArray<ExpressionSyntax> Expressions { get; private set; }

        public CommaOperatorSyntax(ReadOnlyArray<ExpressionSyntax> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            Expressions = expressions;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitCommaOperator(this);
        }
    }
}

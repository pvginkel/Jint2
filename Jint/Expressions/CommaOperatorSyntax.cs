﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class CommaOperatorSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.CommaOperator; }
        }

        internal override bool IsLiteral
        {
            get { return Expressions.Count == 0 || (Expressions.Count == 1 && Expressions[0].IsLiteral); }
        }

        public IList<SyntaxNode> Expressions { get; private set; }

        internal override ValueType ValueType
        {
            get
            {
                var expression = Expressions[Expressions.Count - 1] as ExpressionSyntax;

                return expression != null ? expression.ValueType : ValueType.Unknown;
            }
        }

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

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class ArrayDeclarationSyntax : ExpressionSyntax
    {
        private bool? _isLiteral;

        public override SyntaxType Type
        {
            get { return SyntaxType.ArrayDeclaration; }
        }

        internal override bool IsLiteral
        {
            get
            {
                if (!_isLiteral.HasValue)
                    _isLiteral = Parameters.All(p => p.IsLiteral);

                return _isLiteral.Value;
            }
        }

        public IList<SyntaxNode> Parameters { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Object; }
        }

        public ArrayDeclarationSyntax(IEnumerable<SyntaxNode> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Parameters = parameters.ToReadOnly();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitArrayDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitArrayDeclaration(this);
        }
    }
}

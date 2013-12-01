﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class PropertyDeclarationSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.PropertyDeclaration; }
        }

        internal override bool IsLiteral
        {
            get { return Mode == PropertyExpressionType.Data && Expression.IsLiteral; }
        }

        public string Name { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public PropertyExpressionType Mode { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

        public PropertyDeclarationSyntax(string name, ExpressionSyntax expression, PropertyExpressionType mode)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (expression == null)
                throw new ArgumentNullException("expression");

            Name = name;
            Expression = expression;
            Mode = mode;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitPropertyDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitPropertyDeclaration(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class ArrayDeclarationSyntax : ExpressionSyntax
    {
        private bool? _isLiteral;

        public override SyntaxType Type
        {
            get { return SyntaxType.ArrayDeclaration; }
        }

        public override bool IsLiteral
        {
            get
            {
                if (!_isLiteral.HasValue)
                    _isLiteral = Parameters.All(p => p.IsLiteral);

                return _isLiteral.Value;
            }
        }

        public IList<SyntaxNode> Parameters { get; private set; }

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

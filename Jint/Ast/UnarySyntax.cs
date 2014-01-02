using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class UnarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Unary; }
        }

        public ExpressionSyntax Operand { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

        public UnarySyntax(SyntaxExpressionType operation, ExpressionSyntax operand)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");

            Operation = operation;
            Operand = operand;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
}

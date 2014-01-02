using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class DoWhileSyntax : SyntaxNode, ISourceLocation
    {
        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.DoWhile; }
        }

        public DoWhileSyntax(ExpressionSyntax condition, SyntaxNode statement, SourceLocation location)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (statement == null)
                throw new ArgumentNullException("statement");
            if (location == null)
                throw new ArgumentNullException("location");

            Test = condition;
            Body = statement;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitDoWhile(this);
        }
    }
}

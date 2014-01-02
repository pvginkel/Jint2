using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class TrySyntax : SyntaxNode
    {
        public SyntaxNode Body { get; private set; }
        public CatchClause Catch { get; private set; }
        public FinallyClause Finally { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.Try; }
        }

        public TrySyntax(SyntaxNode body, CatchClause @catch, FinallyClause @finally)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
            Catch = @catch;
            Finally = @finally;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitTry(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Antlr.Runtime;
using Jint.Expressions;

namespace Jint.Parser
{
    partial class ES3Lexer
    {
        public override void ReportError(RecognitionException e)
        {
            throw e;
        }

        protected override object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
        {
            throw new MismatchedTokenException(ttype, input);
        }

        public override object RecoverFromMismatchedSet(IIntStream input, RecognitionException e, BitSet follow)
        {
            throw e;
        }
    }

    partial class ES3Parser
    {
        public override void ReportError(RecognitionException e)
        {
            throw e;
        }

        protected override object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
        {
            throw new MismatchedTokenException(ttype, input);
        }

        public override object RecoverFromMismatchedSet(IIntStream input, RecognitionException e, BitSet follow)
        {
            throw e;
        }

        public ProgramSyntax Execute()
        {
            return program().value;
        }
    }
}

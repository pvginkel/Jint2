using System;
using System.Collections.Generic;
using System.Text;
using Antlr.Runtime;

namespace Jint.Parser
{
    partial class EcmaScriptLexer
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

        private IToken last;

        private bool AreRegularExpressionsEnabled()
        {
            if (last == null)
            {
                return true;
            }
            switch (last.Type)
            {
                    // identifier
                case Identifier:
                    // literals
                case NULL:
                case TRUE:
                case FALSE:
                case THIS:
                case OctalIntegerLiteral:
                case DecimalLiteral:
                case HexIntegerLiteral:
                case StringLiteral:
                    // member access ending 
                case RBRACK:
                    // function call or nested expression ending
                case RPAREN:
                    return false;
                    // otherwise OK
                default:
                    return true;
            }
        }

        private void ConsumeIdentifierUnicodeStart()
        {
            int ch = input.LA(1);
            if (IsIdentifierStartUnicode(ch))
            {
                MatchAny();
                do
                {
                    ch = input.LA(1);
                    if (ch == '$' || (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || ch == '\\' || ch == '_' || (ch >= 'a' && ch <= 'z') || IsIdentifierPartUnicode(ch))
                        mIdentifierPart();
                    else
                        return;
                }
                while (true);
            }

            throw new NoViableAltException();
        }

        private bool IsIdentifierPartUnicode(int ch)
        {
            return char.IsLetterOrDigit((char)ch);
        }

        private bool IsIdentifierStartUnicode(int ch)
        {
            return char.IsLetter((char)ch);
        }

        public override IToken NextToken()
        {
            var result = base.NextToken();
            if (result.Channel == DefaultTokenChannel)
                last = result;
            return result;
        }

        private string ExtractIdentifier(string text)
        {
            return EcmaScriptUtil.ExtractString(text, false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr.Runtime;
using Jint.Ast;

namespace Jint.Parser
{
    partial class EcmaScriptParser
    {
        private readonly string _sourceCode;
        private readonly AstBuilder _builder = new AstBuilder();

        private static readonly NumberFormatInfo _numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public EcmaScriptParser(ITokenStream input, string sourceCode)
            : base(input)
        {
            _sourceCode = sourceCode;
        }

        public EcmaScriptParser(ITokenStream input, RecognizerSharedState state, string sourceCode)
            : base(input, state)
        {
            _sourceCode = sourceCode;
        }

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
            return program();
        }

        public BodySyntax ExecuteBlockStatements(ReadOnlyArray<string> parameters)
        {
            _builder.EnterFunctionBody(parameters);

            blockStatements();

            return _builder.ExitBody();
        }

        private bool IsLeftHandSideAssign(ExpressionSyntax lhs)
        {
            if (!IsLeftHandSideExpression(lhs))
                return false;

            switch (input.LA(1))
            {
                case ASSIGN:
                case MULASS:
                case DIVASS:
                case MODASS:
                case ADDASS:
                case SUBASS:
                case SHLASS:
                case SHRASS:
                case SHUASS:
                case ANDASS:
                case XORASS:
                case ORASS:
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsLeftHandSideExpression(ExpressionSyntax lhs)
        {
            return lhs == null || lhs.IsAssignable;
        }

        private bool IsLeftHandSideIn(ExpressionSyntax lhs)
        {
            return IsLeftHandSideExpression(lhs) && input.LA(1) == IN;
        }

        private IToken PromoteEol()
        {
            // Get current token and its type (the possibly offending token).
            IToken lt = input.LT(1);
            int la = lt.Type;

            // We only need to promote an EOL when the current token is offending (not a SEMIC, EOF, RBRACE, EOL or MultiLineComment).
            // EOL and MultiLineComment are not offending as they're already promoted in a previous call to this method.
            // Promoting an EOL means switching it from off channel to on channel.
            // A MultiLineComment gets promoted when it contains an EOL.
            if (!(la == SEMIC || la == EOF || la == RBRACE || la == EOL || la == MultiLineComment))
            {
                // Start on the possition before the current token and scan backwards off channel tokens until the previous on channel token.
                for (int ix = lt.TokenIndex - 1; ix > 0; ix--)
                {
                    lt = input.Get(ix);
                    if (lt.Channel == DefaultTokenChannel)
                    {
                        // On channel token found: stop scanning.
                        break;
                    }
                    if (lt.Type == EOL || (lt.Type == MultiLineComment && (lt.Text.EndsWith("\r") || lt.Text.EndsWith("\n"))))
                    {
                        // We found our EOL: promote the token to on channel, position the input on it and reset the rule start.
                        lt.Channel = DefaultTokenChannel;
                        input.Seek(lt.TokenIndex);
                        return lt;
                    }
                }
            }

            return null;
        }

        private string ExtractRegExpPattern(string text)
        {
            return text.Substring(1, text.LastIndexOf('/') - 1);
        }

        private string ExtractRegExpOption(string text)
        {
            if (text[text.Length - 1] != '/')
                return text.Substring(text.LastIndexOf('/') + 1);
            return String.Empty;
        }

        private string ExtractString(string text)
        {
            return EcmaScriptUtil.ExtractString(text, true);
        }

        public List<string> Errors { get; private set; }

        public override void DisplayRecognitionError(String[] tokenNames, RecognitionException e)
        {

            base.DisplayRecognitionError(tokenNames, e);

            if (Errors == null)
                Errors = new List<string>();

            var hdr = GetErrorHeader(e);
            var msg = GetErrorMessage(e, tokenNames);
            Errors.Add(msg + " at " + hdr);
        }

        private SourceLocation GetLocation(IToken start, IToken stop)
        {
            return new SourceLocation(
                start.StartIndex,
                start.Line,
                start.CharPositionInLine,
                stop.StopIndex + stop.Text.Length,
                stop.Line,
                stop.CharPositionInLine + stop.Text.Length,
                _sourceCode
            );
        }

        public AssignmentOperator ResolveAssignmentOperator(string op)
        {
            switch (op)
            {
                case "=": return AssignmentOperator.Assign;
                case "+=": return AssignmentOperator.Add;
                case "-=": return AssignmentOperator.Subtract;
                case "*=": return AssignmentOperator.Multiply;
                case "%=": return AssignmentOperator.Modulo;
                case "<<=": return AssignmentOperator.LeftShift;
                case ">>=": return AssignmentOperator.RightShift;
                case ">>>=": return AssignmentOperator.UnsignedRightShift;
                case "&=": return AssignmentOperator.BitwiseAnd;
                case "|=": return AssignmentOperator.BitwiseOr;
                case "^=": return AssignmentOperator.BitwiseExclusiveOr;
                case "/=": return AssignmentOperator.Divide;
                default: throw new NotSupportedException("Invalid assignment operator: " + op);
            }
        }
    }
}
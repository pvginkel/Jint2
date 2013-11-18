﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr.Runtime;
using Jint.Debugger;
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
    			    {
    				    mIdentifierPart();
    			    }
    			    else
    			    {
    				    return;
    			    }
    		    }
    		    while (true);
    	    }
    	    else
    	    {
    		    throw new NoViableAltException();
    	    }
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
    	    IToken result = base.NextToken();
    	    if (result.Channel == DefaultTokenChannel)
    	    {
    		    last = result;
    	    }
    	    return result;		
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

        public BlockSyntax ExecuteBlockStatements()
        {
            return blockStatements().value;
        }

		// References the upper level block currently parsed. 
		// This is used to add variable declarations at the top of the body while parsing.
		private BlockBuilder _currentBody;
		
		private const char BS = '\\';
		private bool IsLeftHandSideAssign(ExpressionSyntax lhs, ref bool? cached)
		{
            if (cached.HasValue)
                return cached.Value;
	    	
    		bool result;
    		if(IsLeftHandSideExpression(lhs))
    		{
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
    					result = true;
    					break;
    				default:
    					result = false;
    					break;
    			}
    		}
    		else
    		{
    			result = false;
    		}
	    	
    		cached = result;
    		return result;
		}

		private static bool IsLeftHandSideExpression(ExpressionSyntax lhs)
		{
            return lhs == null || lhs.IsAssignable;
		}
	    	
		private bool IsLeftHandSideIn(ExpressionSyntax lhs, ref bool? cached)
		{
            if (cached.HasValue)
                return cached.Value;
	    	
    		bool result = IsLeftHandSideExpression(lhs) && (input.LA(1) == IN);
    		cached = result;
    		return result;
		}

		private void PromoteEOL(ParserRuleReturnScope<IToken> rule)
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
    				else if (lt.Type == EOL || (lt.Type == MultiLineComment && (lt.Text.EndsWith("\r") || lt.Text.EndsWith("\n"))))
    				{
    					// We found our EOL: promote the token to on channel, position the input on it and reset the rule start.
    					lt.Channel = DefaultTokenChannel;
    					input.Seek(lt.TokenIndex);
    					if (rule != null)
    					{
    						rule.Start = lt;
    					}
    					break;
    				}
    			}
    		}
		}	
	    
		private static NumberFormatInfo numberFormatInfo = new NumberFormatInfo();

		private string extractRegExpPattern(string text) {
			return text.Substring(1, text.LastIndexOf('/')-1);
		}

		private string extractRegExpOption(string text) {
			if(text[text.Length-1] != '/')
			{
			return text.Substring(text.LastIndexOf('/')+1);
			}
			return String.Empty;
		}
    
		private static Encoding Latin1 = Encoding.GetEncoding("iso-8859-1");
    
	    private string extractString(string text) {
	    
	        // https://developer.mozilla.org/en/Core_JavaScript_1.5_Guide/Literals#String Literals    
	        StringBuilder sb = new StringBuilder(text.Length);
	        int startIndex = 1; // Skip initial quote
	        int slashIndex = -1;

	        while ((slashIndex = text.IndexOf(BS, startIndex)) != -1)
	        {
                sb.Append(text.Substring(startIndex, slashIndex - startIndex));
	            char escapeType = text[slashIndex + 1];
	            switch (escapeType)
	            {
	                case '0':
	                case '1':
	                case '2':
	                case '3':
	                case '4':
	                case '5':
	                case '6':
	                case '7':
	                case '8':
	                case '9':
                        string octalCode = text.Substring(slashIndex + 1, 3);   
                        char octalChar = Latin1.GetChars(new byte[] { System.Convert.ToByte(octalCode, 8) } )[0]; 
                        // insert decoded char
                        sb.Append(octalChar);
                        // skip encoded char
                        slashIndex += 4;
			          break;                 
	                case 'x':
                        string asciiCode = text.Substring(slashIndex + 2, 2); ;
                        char asciiChar = Latin1.GetChars(new byte[] { System.Convert.ToByte(asciiCode, 16) } )[0];
                        sb.Append(asciiChar);
                        slashIndex += 4;
                        break;   	
	                case 'u':
                        char unicodeChar = System.Convert.ToChar(Int32.Parse(text.Substring(slashIndex + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier));
                        sb.Append(unicodeChar);
                        slashIndex += 6;
                        break;
                    case 'b': sb.Append('\b'); slashIndex += 2; break;
                    case 'f': sb.Append('\f'); slashIndex += 2; break;
                    case 'n': sb.Append('\n'); slashIndex += 2; break;
                    case 'r': sb.Append('\r'); slashIndex += 2; break;
                    case 't': sb.Append('\t'); slashIndex += 2; break;
                    case 'v': sb.Append('\v'); slashIndex += 2; break;
                    case '\'': sb.Append('\''); slashIndex += 2; break;
                    case '"': sb.Append('"'); slashIndex += 2; break;
                    case '\\': sb.Append('\\'); slashIndex += 2; break;
                    case '\r': if (text[slashIndex + 2] == '\n') slashIndex += 3; break;
                    case '\n': slashIndex += 2; break;
                    default: sb.Append(escapeType); slashIndex += 2; break;
	            }

                startIndex = slashIndex;
	        }

            if (sb.Length == 0)
                return text.Substring(1, text.Length - 2);

            sb.Append(text.Substring(startIndex, text.Length - startIndex - 1));
	        return sb.ToString();
	    }
	    
		public List<string> Errors { get; private set; }

		public override void DisplayRecognitionError(String[] tokenNames, RecognitionException e) {
	        
			base.DisplayRecognitionError(tokenNames, e);
	        
			if(Errors == null)
			{
        		Errors = new List<string>();
			}
	        
			String hdr = GetErrorHeader(e);
			String msg = GetErrorMessage(e, tokenNames);
			Errors.Add(msg + " at " + hdr);
		}    

        // TODO: Why is this there? Did we remove something that should use this?
		private string[] script = new string[0];
	    
		private SourceCodeDescriptor ExtractSourceCode(CommonToken start, CommonToken stop)
		{
            return new SourceCodeDescriptor(start.Line, start.CharPositionInLine, stop.Line, stop.CharPositionInLine, "No source code available.");
		}

		public AssignmentOperator ResolveAssignmentOperator(string op)
		{
    		switch(op)
    		{
    			case "=" : return AssignmentOperator.Assign;
    			case "+=" : return AssignmentOperator.Add;
    			case "-=" : return AssignmentOperator.Subtract;
    			case "*=" : return AssignmentOperator.Multiply;
    			case "\\%=" : return AssignmentOperator.Modulo;
    			case "<<=" : return AssignmentOperator.LeftShift;
    			case ">>=" : return AssignmentOperator.RightShift;
    			case ">>>=" : return AssignmentOperator.UnsignedRightShift;
    			case "&=" : return AssignmentOperator.BitwiseAnd;
    			case "|=" : return AssignmentOperator.BitwiseOr;
    			case "^=" : return AssignmentOperator.BitwiseExclusiveOr;
    			case "/=" : return AssignmentOperator.Divide;
    			default : throw new NotSupportedException("Invalid assignment operator: " + op);
    		}
		}
    }
}

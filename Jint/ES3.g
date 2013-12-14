/*

Copyrights 2008-2009 Xebic Reasearch BV. All rights reserved (see license.txt).
Original work by Patrick Hulsmeijer.

This ANTLR 3 LL(*) grammar is based on Ecma-262 3rd edition (JavaScript 1.5, JScript 5.5). 
The annotations refer to the "A Grammar Summary" section (e.g. A.1 Lexical Grammar) and the numbers in parenthesis to the paragraph numbers (e.g. (7.8) ).
This document is best viewed with ANTLRWorks (www.antlr.org).


The major challenges faced in defining this grammar were:

-1- Ambiguity surrounding the DIV sign in relation to the multiplicative expression and the regular expression literal.
This is solved with some lexer driven magic: a gated semantical predicate turns the recognition of regular expressions on or off, based on the
value of the RegularExpressionsEnabled property. When regular expressions are enabled they take precedence over division expressions. The decision whether
regular expressions are enabled is based on the heuristics that the previous token can be considered as last token of a left-hand-side operand of a division.

-2- Automatic semicolon insertion.
This is solved within the parser. The semicolons are not physically inserted but the situations in which they should are recognized and treated as if they were.
The physical insertion of semicolons would be undesirable because of several reasons:
- performance degration because of how ANTLR handles tokens in token streams
- the alteration of the input, which we need to have unchanged
- it is superfluous being of no interest to AST construction

-3- Unicode identifiers
Because ANTLR couldn't handle the unicode tables defined in the specification well and for performance reasons unicode identifiers are implemented as an action 
driven alternative to ASCII identifiers. First the ASCII version is tried that is defined in detail in this grammar and then the unicode alternative is tried action driven.
Because of the fact that the ASCII version is defined in detail the mTokens switch generation in the lexer can predict identifiers appropriately.
For details see the identifier rules.


The minor challenges were related to converting the grammar to an ANTLR LL(*) grammar:
- Resolving the ambiguity between functionDeclaration vs functionExpression and block vs objectLiteral stemming from the expressionStatement production.
- Left recursive nature of the left hand side expressions.
- The assignmentExpression production.
- The forStatement production.
The grammar was kept as close as possible to the grammar in the "A Grammar Summary" section of Ecma-262.

*/

grammar ES3 ;

options
{
	language = CSharp3 ;
}


tokens
{
// Reserved words
	NULL		= 'null' ;
	TRUE		= 'true' ;
	FALSE		= 'false' ;

// Keywords
	BREAK		= 'break' ;
	CASE		= 'case' ;
	CATCH 		= 'catch' ;
	CONTINUE 	= 'continue' ;
	DEFAULT		= 'default' ;
	DELETE		= 'delete' ;
	DO 		= 'do' ;
	ELSE 		= 'else' ;
	FINALLY 	= 'finally' ;
	FOR 		= 'for' ;
	FUNCTION 	= 'function' ;
	IF 		= 'if' ;
	IN 		= 'in' ;
	INSTANCEOF 	= 'instanceof' ;
	NEW 		= 'new' ;
	RETURN 		= 'return' ;
	SWITCH 		= 'switch' ;
	THIS 		= 'this' ;
	THROW 		= 'throw' ;
	TRY 		= 'try' ;
	TYPEOF 		= 'typeof' ;
	VAR 		= 'var' ;
	VOID 		= 'void' ;
	WHILE 		= 'while' ;
	WITH 		= 'with' ;

// Future reserved words
	ABSTRACT	= 'abstract' ;
	BOOLEAN 	= 'boolean' ;
	BYTE 		= 'byte' ;
	CHAR 		= 'char' ;
	CLASS 		= 'class' ;
	CONST 		= 'const' ;
	DEBUGGER 	= 'debugger' ;
	DOUBLE		= 'double' ;
	ENUM 		= 'enum' ;
	EXPORT 		= 'export' ;
	EXTENDS		= 'extends' ;
	FINAL 		= 'final' ;
	FLOAT 		= 'float' ;
	GOTO 		= 'goto' ;
	IMPLEMENTS 	= 'implements' ;
	IMPORT		= 'import' ;
	INT 		= 'int' ;
	INTERFACE 	= 'interface' ;
	LONG 		= 'long' ;
	NATIVE 		= 'native' ;
	PACKAGE 	= 'package' ;
	PRIVATE 	= 'private' ;
	PROTECTED 	= 'protected' ;
	PUBLIC		= 'public' ;
	SHORT 		= 'short' ;
	STATIC 		= 'static' ;
	SUPER 		= 'super' ;
	SYNCHRONIZED 	= 'synchronized' ;
	THROWS 		= 'throws' ;
	TRANSIENT 	= 'transient' ;
	VOLATILE 	= 'volatile' ;
    REF         = 'ref' ;

// Punctuators
	LBRACE		= '{' ;
	RBRACE		= '}' ;
	LPAREN		= '(' ;
	RPAREN		= ')' ;
	LBRACK		= '[' ;
	RBRACK		= ']' ;
	DOT		= '.' ;
	SEMIC		= ';' ;
	COMMA		= ',' ;
	LT		= '<' ;
	GT		= '>' ;
	LTE		= '<=' ;
	GTE		= '>=' ;
	EQ		= '==' ;
	NEQ		= '!=' ;
	SAME		= '===' ;
	NSAME		= '!==' ;
	ADD		= '+' ;
	SUB		= '-' ;
	MUL		= '*' ;
	MOD		= '%' ;
	INC		= '++' ;
	DEC		= '--' ;
	SHL		= '<<' ;
	SHR		= '>>' ;
	SHU		= '>>>' ;
	AND		= '&' ;
	OR		= '|' ;
	XOR		= '^' ;
	NOT		= '!' ;
	INV		= '~' ;
	LAND		= '&&' ;
	LOR		= '||' ;
	QUE		= '?' ;
	COLON		= ':' ;
	ASSIGN		= '=' ;
	ADDASS		= '+=' ;
	SUBASS		= '-=' ;
	MULASS		= '*=' ;
	MODASS		= '%=' ;
	SHLASS		= '<<=' ;
	SHRASS		= '>>=' ;
	SHUASS		= '>>>=' ;
	ANDASS		= '&=' ;
	ORASS		= '|=' ;
	ORASS		= '|=' ;
	XORASS		= '^=' ;
	DIV		= '/' ;
	DIVASS		= '/=' ;
	
// Imaginary
	ARGS ;
	ARRAY ;
	BLOCK ;
	BYFIELD ;
	BYINDEX ;
	CALL ;
	CEXPR ;
	EXPR ;
	FORITER ;
	FORSTEP ;
	ITEM ;
	LABELLED ;
	NAMEDVALUE ;
	NEG ;
	OBJECT ;
	PAREXPR ;
	PDEC ;
	PINC ;
	POS ;

}

@lexer::namespace { Jint.Parser }
@parser::namespace { Jint.Parser }
/*
 * These have no effect. Instead, the templates have been changed.
 *
@lexer::modifier { internal }
@parser::modifier { internal }
 */

@header {
using System;
using System.Text;
using System.Globalization;
using Jint.Expressions;
}

//
// $<	A.1 Lexical Grammar (7)
//

// Added for lexing purposes

fragment BSLASH
	: '\\'
	;
	
fragment DQUOTE
	: '"'
	;
	
fragment SQUOTE
	: '\''
	;

// $<	Whitespace (7.2)

fragment TAB
	: '\u0009'
	;

fragment VT // Vertical TAB
	: '\u000b'
	;

fragment FF // Form Feed
	: '\u000c'
	;

fragment SP // Space
	: '\u0020'
	;

fragment NBSP // Non-Breaking Space
	: '\u00a0'
	;

fragment USP // Unicode Space Separator (rest of Unicode category Zs)
	: '\u1680'  // OGHAM SPACE MARK
	| '\u180E'  // MONGOLIAN VOWEL SEPARATOR
	| '\u2000'  // EN QUAD
	| '\u2001'  // EM QUAD
	| '\u2002'  // EN SPACE
	| '\u2003'  // EM SPACE
	| '\u2004'  // THREE-PER-EM SPACE
	| '\u2005'  // FOUR-PER-EM SPACE
	| '\u2006'  // SIX-PER-EM SPACE
	| '\u2007'  // FIGURE SPACE
	| '\u2008'  // PUNCTUATION SPACE
	| '\u2009'  // THIN SPACE
	| '\u200A'  // HAIR SPACE
	| '\u202F'  // NARROW NO-BREAK SPACE
	| '\u205F'  // MEDIUM MATHEMATICAL SPACE
	| '\u3000'  // IDEOGRAPHIC SPACE
	;

WhiteSpace
	: ( TAB | VT | FF | SP | NBSP | USP )+ { $channel = Hidden; }
	;

// $>

// $<	Line terminators (7.3)

fragment LF // Line Feed
	: '\n'
	;

fragment CR // Carriage Return
	: '\r'
	;

fragment LS // Line Separator
	: '\u2028'
	;

fragment PS // Paragraph Separator
	: '\u2029'
	;

fragment LineTerminator
	: CR | LF | LS | PS
	;
		
EOL
	: ( ( CR LF? ) | LF | LS | PS ) { $channel = Hidden; }
	;
// $>

// $<	Comments (7.4)

MultiLineComment
	: '/*' ( options { greedy = false; } : . )* '*/' { $channel = Hidden; }
	;

SingleLineComment
	: '//' ( ~( LineTerminator ) )* { $channel = Hidden; }
	;

// $>

// $<	Tokens (7.5)

token
	: reservedWord
	| Identifier
	| punctuator
	| numericLiteral
	| StringLiteral
	;

// $<	Reserved words (7.5.1)

reservedWord
	: keyword
	| futureReservedWord
	| NULL
	| booleanLiteral
	;

// $>
	
// $<	Keywords (7.5.2)

keyword
	: BREAK
	| CASE
	| CATCH
	| CONTINUE
	| DEFAULT
	| DELETE
	| DO
	| ELSE
	| FINALLY
	| FOR
	| FUNCTION
	| IF
	| IN
	| INSTANCEOF
	| NEW
	| RETURN
	| SWITCH
	| THIS
	| THROW
	| TRY
	| TYPEOF
	| VAR
	| VOID
	| WHILE
	| WITH
	;

// $>

// $<	Future reserved words (7.5.3)

futureReservedWord
	: ABSTRACT
	| BOOLEAN
	| BYTE
	| CHAR
	| CLASS
	| CONST
	| DEBUGGER
	| DOUBLE
	| ENUM
	| EXPORT
	| EXTENDS
	| FINAL
	| FLOAT
	| GOTO
	| IMPLEMENTS
	| IMPORT
	| INT
	| INTERFACE
	| LONG
	| NATIVE
	| PACKAGE
	| PRIVATE
	| PROTECTED
	| PUBLIC
	| SHORT
	| STATIC
	| SUPER
	| SYNCHRONIZED
	| THROWS
	| TRANSIENT
	| VOLATILE
	;

// $>

// $>
	
// $<	Identifiers (7.6)

fragment IdentifierStartASCII
	: 'a'..'z' | 'A'..'Z'
	| '$'
	| '_'
	| BSLASH 'u' HexDigit HexDigit HexDigit HexDigit // UnicodeEscapeSequence
	;

/*
The first two alternatives define how ANTLR can match ASCII characters which can be considered as part of an identifier.
The last alternative matches other characters in the unicode range that can be sonsidered as part of an identifier.
*/
fragment IdentifierPart
	: DecimalDigit
	| IdentifierStartASCII
	| { IsIdentifierPartUnicode(input.LA(1)) }? { MatchAny(); }
	;

fragment IdentifierNameASCIIStart
	: IdentifierStartASCII IdentifierPart*
	;

/*
The second alternative acts as an action driven fallback to evaluate other
characters in the unicode range than the ones in the ASCII subset.
Due to the first alternative this grammar defines enough so that ANTLR can
generate a lexer that correctly predicts identifiers with characters in the ASCII range.
In that way keywords, other reserved words and ASCII identifiers are recognized
with standard ANTLR driven logic. When the first character for an identifier fails to 
match this ASCII definition, the lexer calls ConsumeIdentifierUnicodeStart because
of the action in the alternative. This method checks whether the character matches 
as first character in ranges other than ASCII and consumes further characters
belonging to the identifier with help of mIdentifierPart generated out of the 
IdentifierPart rule above.
*/
Identifier
@after {
    $text = ExtractIdentifier($text);
}
	: IdentifierNameASCIIStart
	| { ConsumeIdentifierUnicodeStart(); }
	;

// $>

// $<	Punctuators (7.7)

punctuator
	: LBRACE
	| RBRACE
	| LPAREN
	| RPAREN
	| LBRACK
	| RBRACK
	| DOT
	| SEMIC
	| COMMA
	| LT
	| GT
	| LTE
	| GTE
	| EQ
	| NEQ
	| SAME
	| NSAME
	| ADD
	| SUB
	| MUL
	| MOD
	| INC
	| DEC
	| SHL
	| SHR
	| SHU
	| AND
	| OR
	| XOR
	| NOT
	| INV
	| LAND
	| LOR
	| QUE
	| COLON
	| ASSIGN
	| ADDASS
	| SUBASS
	| MULASS
	| MODASS
	| SHLASS
	| SHRASS
	| SHUASS
	| ANDASS
	| ORASS
	| XORASS
	| DIV
	| DIVASS
	;

// $>

// $<	Literals (7.8)

literal returns [ExpressionSyntax value]
	: exp1=NULL { $value = new IdentifierSyntax(exp1.Text); }
	| exp2=booleanLiteral { $value = new ValueSyntax(exp2); }
	| exp3=numericLiteral { $value = new ValueSyntax(exp3); }
	| exp4=StringLiteral  { $value = new ValueSyntax(ExtractString(exp4.Text)); }
	| exp5=RegularExpressionLiteral { $value = new RegexpSyntax(ExtractRegExpPattern(exp5.Text), ExtractRegExpOption(exp5.Text)); }
	;

booleanLiteral returns [bool value]
	: TRUE { $value = true; }
	| FALSE { $value = false; }
	;

// $<	Numeric literals (7.8.3)

/*
Note: octal literals are described in the B Compatibility section.
These are removed from the standards but are here for backwards compatibility with earlier ECMAScript definitions.
*/

fragment DecimalDigit
	: '0'..'9'
	;

fragment HexDigit
	: DecimalDigit | 'a'..'f' | 'A'..'F'
	;

fragment OctalDigit
	: '0'..'7'
	;

fragment ExponentPart
	: ( 'e' | 'E' ) ( '+' | '-' )? DecimalDigit+
	;

fragment DecimalIntegerLiteral
	: '0'
	| '1'..'9' DecimalDigit*
	;

DecimalLiteral
	: DecimalIntegerLiteral '.' DecimalDigit* ExponentPart?
	| '.' DecimalDigit+ ExponentPart?
	| DecimalIntegerLiteral ExponentPart?
	;

OctalIntegerLiteral
	: '0' OctalDigit+
	;

HexIntegerLiteral
	: ( '0x' | '0X' ) HexDigit+
	;

numericLiteral returns [double value]
	: ex1=DecimalLiteral { $value = double.Parse(ex1.Text, NumberStyles.Float, _numberFormatInfo); }
	| ex2=OctalIntegerLiteral { $value = System.Convert.ToInt64(ex2.Text, 8); }
	| ex3=HexIntegerLiteral { $value = System.Convert.ToInt64(ex3.Text, 16); }
	;

// $>

// $<	String literals (7.8.4)

/*
Note: octal escape sequences are described in the B Compatibility section.
These are removed from the standards but are here for backwards compatibility with earlier ECMAScript definitions.
*/
	
fragment CharacterEscapeSequence
	: ~( DecimalDigit | 'x' | 'u' | LineTerminator ) // Concatenation of SingleEscapeCharacter and NonEscapeCharacter
	;

fragment ZeroToThree
	: '0'..'3'
	;
	
fragment OctalEscapeSequence
	: OctalDigit
	| ZeroToThree OctalDigit
	| '4'..'7' OctalDigit
	| ZeroToThree OctalDigit OctalDigit
	;
	
fragment HexEscapeSequence
	: 'x' HexDigit HexDigit
	;
	
fragment UnicodeEscapeSequence
	: 'u' HexDigit HexDigit HexDigit HexDigit
	;

fragment EscapeSequence
	:
	BSLASH 
	(
		CharacterEscapeSequence 
		| OctalEscapeSequence
		| HexEscapeSequence
		| UnicodeEscapeSequence
		| CR LF?
        | LF // allow string continuations over a new line
	)
	;

StringLiteral
	: SQUOTE ( ~( SQUOTE | BSLASH | LineTerminator ) | EscapeSequence )* SQUOTE
	| DQUOTE ( ~( DQUOTE | BSLASH | LineTerminator ) | EscapeSequence )* DQUOTE
	;

// $>

// $<	Regular expression literals (7.8.5)

fragment BackslashSequence
	: BSLASH ~( LineTerminator )
	;

fragment RegularExpressionFirstChar
	: ~ ( LineTerminator | MUL | BSLASH | DIV )
	| BackslashSequence
	;

fragment RegularExpressionChar
	: ~ ( LineTerminator | BSLASH | DIV )
	| BackslashSequence
	;

RegularExpressionLiteral
	: { AreRegularExpressionsEnabled() }?=> DIV RegularExpressionFirstChar RegularExpressionChar* DIV IdentifierPart*
	;

// $>

// $>

// $>

//
// $<	A.3 Expressions (11)
//

// $<Primary expressions (11.1)

primaryExpression returns [ExpressionSyntax value]
	: ex1=THIS { $value = new IdentifierSyntax(ex1.Text); }
	| ex2=Identifier { $value = new IdentifierSyntax(ex2.Text); }
	| ex3=literal { $value = ex3; }
	| ex4=arrayLiteral { $value = ex4; }
	| ex5=objectLiteral { $value = ex5; }
	| lpar=LPAREN ex6=expression  RPAREN  { $value = ex6; } 
	;

arrayLiteral returns [ArrayDeclarationSyntax value]
@init {
    var parameters = new List<SyntaxNode>();
}
@after {
	$value = new ArrayDeclarationSyntax(parameters);
}
	:
        lb=LBRACK
		(
            first=arrayItem
            { if(first != null) parameters.Add(first); }
            (
                COMMA follow=arrayItem
                { if(follow != null) parameters.Add(follow); }
            )*
        )?
        RBRACK
	;

arrayItem returns [SyntaxNode value]
	: ( expr=assignmentExpression  { $value = expr; } | { input.LA(1) == COMMA }? { $value = new IdentifierSyntax("undefined"); } | { input.LA(1) == RBRACK }? { $value = null; }  )
	
	;

objectLiteral returns [JsonExpressionSyntax value]
@init {
    var builder = new JsonPropertyBuilder();
}
@after {
	$value = new JsonExpressionSyntax(builder.GetProperties());
}
	:
      lb=LBRACE (
        first=propertyAssignment { builder.AddProperty(first); } (
          COMMA
          follow=propertyAssignment { builder.AddProperty(follow); }
        )*
      )?
      RBRACE
	;
	
propertyAssignment returns [PropertyDeclaration value]
	:
        func=propertyFunctionAssignment
        { $value = func; }
	|
        data=propertyValueAssignment
        { $value = data; }
	;

propertyFunctionAssignment returns [PropertyDeclaration value]
@init {
    PropertyExpressionType mode;
    BodySyntax body;
    List<string> parameters = null;
    string name;
    IToken start = null;
}
@after {
    $value = new PropertyDeclaration(
        name,
        new FunctionSyntax(
            name,
            parameters,
            body,
            null,
            GetLocation(start, input.LT(-1))
        ),
        mode
    );
}
    :
        acc=accessor
        { mode = acc; }
        prop2=propertyName
        { name = prop2; }
        (
            { start = input.LT(1); }
            parms=formalParameterList
            { parameters = parms; }
        )?
        {
            if (start == null)
                start = input.LT(1);
        }
        statements=functionBody
        { body = statements; } 
    ;

propertyValueAssignment returns [PropertyDeclaration value]
@init {
    string name;
    ExpressionSyntax expression;
}
@after {
    $value = new PropertyDeclaration(
        name,
        expression,
        PropertyExpressionType.Data
    );
}
    :
        prop1=propertyName
        { name = prop1; }
        COLON
        ass=assignmentExpression
        { expression = ass; }
    ;        

accessor returns [PropertyExpressionType value]
	: ex1=Identifier { ex1.Text=="get" || ex1.Text=="set" }?=> { if(ex1.Text=="get") $value= PropertyExpressionType.Get; if(ex1.Text=="set") $value=PropertyExpressionType.Set; }
	;

propertyName returns [string value]
	: ex1=Identifier { $value = ex1.Text; }
	| ex2=StringLiteral { $value = ExtractString(ex2.Text); }
	| ex3=numericLiteral { $value = ex3.ToString(); }
	;

// $>

// $<Left-hand-side expressions (11.2)

/*
Refactored some rules to make them LL(*) compliant:
all the expressions surrounding member selection and calls have been moved to leftHandSideExpression to make them right recursive
*/

memberExpression returns [ExpressionSyntax value]
	: prim=primaryExpression { $value = prim; }
	| func=functionExpression { $value = func; }
	;
	
arguments returns [List<MethodArgument> value]
@init {
	$value = new List<MethodArgument>();
}
	:
        LPAREN
        (
            first=argument
            { $value.Add(first); }
            (
                COMMA
                follow=argument
                { $value.Add(follow); }
            )*
        )?
        RPAREN
	;

argument returns [MethodArgument value]
@init {
    bool isRef = false;
}
    :
        ( REF { isRef = true; } )?
        ex=assignmentExpression
        { $value = new MethodArgument(ex, isRef); }
    ;

generics returns [List<ExpressionSyntax> value]
@init {
	$value = new List<ExpressionSyntax>();
}
	: LBRACE ( first=assignmentExpression { $value.Add(first); } ( COMMA follow=assignmentExpression { $value.Add(follow); })* )? RBRACE
	
	;
	
leftHandSideExpression returns [ExpressionSyntax value]
@init {
	List<ExpressionSyntax> gens = new List<ExpressionSyntax>();
    bool isNew = false;
}
@after{
    if (isNew)
        $value = new NewSyntax($value);
}
	:
        (
            NEW
            { isNew = true; }
        )?
	    mem=memberExpression
        { $value = mem; }
	    (
		    (
                gen=generics
                { gens = gen; }
            )?
            arg=arguments
            {
                $value = new MethodCallSyntax(
                    $value,
                    arg,
                    gens
                );

                if (isNew)
                {
                    isNew = false;
                    $value = new NewSyntax($value);
                }
            } 
	
		|
            LBRACK exp=expression RBRACK
            {
                $value = new IndexerSyntax(
                    $value,
                    exp
                );
            } 
			
		|
            DOT id=Identifier
            {
                $value = new PropertySyntax(
                    $value,
                    $id.Text
                );
            }
	    )* 
	;

// $>

// $<Postfix expressions (11.3)

/*
The specification states that there are no line terminators allowed before the postfix operators.
This is enforced by the call to PromoteEOL in the action before ( INC | DEC ).
We only must promote EOLs when the la is INC or DEC because this production is chained as all expression rules.
In other words: only promote EOL when we are really in a postfix expression. A check on the la will ensure this.
*/
postfixExpression returns [ExpressionSyntax value]
	: left=leftHandSideExpression { $value = left; if (input.LA(1) == INC || input.LA(1) == DEC) PromoteEol();  } ( post=postfixOperator { $value = new UnarySyntax(post, $value); })?
	;
	
postfixOperator returns [SyntaxExpressionType value]
	: op=INC { $op.Type = PINC; $value = SyntaxExpressionType.PostIncrementAssign; }
	| op=DEC { $op.Type = PDEC; $value = SyntaxExpressionType.PostDecrementAssign; }
	;

// $>

// $<Unary operators (11.4)

unaryExpression returns [ExpressionSyntax value]
	: post=postfixExpression { $value = post; }
	| op=unaryOperator exp=unaryExpression { $value = new UnarySyntax(op, exp); }
	;
	
unaryOperator returns [SyntaxExpressionType value]
	: DELETE { $value = SyntaxExpressionType.Delete; }
	| VOID { $value = SyntaxExpressionType.Void; }
	| TYPEOF { $value = SyntaxExpressionType.TypeOf; }
	| INC { $value = SyntaxExpressionType.PreIncrementAssign; }
	| DEC { $value = SyntaxExpressionType.PreDecrementAssign; }
	| op=ADD { $op.Type = POS; $value = SyntaxExpressionType.UnaryPlus; }
	| op=SUB { $op.Type = NEG; $value = SyntaxExpressionType.Negate; }
	| INV { $value = SyntaxExpressionType.BitwiseNot; }
	| NOT { $value = SyntaxExpressionType.Not; }
	;

// $>

// $<Multiplicative operators (11.5)

multiplicativeExpression returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=unaryExpression { $value = left; } ( 
		( MUL { type= SyntaxExpressionType.Multiply; } 
		| DIV { type= SyntaxExpressionType.Divide; }
		| MOD { type= SyntaxExpressionType.Modulo; }) 
		right=unaryExpression { $value = new BinarySyntax(type, $value, right); })*
	;

// $>

// $<Additive operators (11.6)

additiveExpression returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=multiplicativeExpression { $value = left; } ( 
		( ADD { type= SyntaxExpressionType.Add; }
		| SUB { type= SyntaxExpressionType.Subtract; }) 
		right=multiplicativeExpression { $value = new BinarySyntax(type, $value, right); })*
	;

// $>
	
// $<Bitwise shift operators (11.7)

shiftExpression returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=additiveExpression { $value = left; } ( 
		( SHL { type= SyntaxExpressionType.LeftShift; }
		| SHR { type= SyntaxExpressionType.RightShift; }
		| SHU { type= SyntaxExpressionType.UnsignedRightShift; }) 
		right=additiveExpression { $value = new BinarySyntax(type, $value, right); })*
	;

// $>
	
// $<Relational operators (11.8)

relationalExpression returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=shiftExpression { $value = left; } ( 
		( LT { type= SyntaxExpressionType.LessThan; }
		| GT { type= SyntaxExpressionType.GreaterThan; }
		| LTE { type= SyntaxExpressionType.LessThanOrEqual; }
		| GTE { type= SyntaxExpressionType.GreaterThanOrEqual; }
		| INSTANCEOF { type= SyntaxExpressionType.InstanceOf;  }
		| IN { type= SyntaxExpressionType.In;  }) 
		right=shiftExpression { $value = new BinarySyntax(type, $value, right); })*
	;

relationalExpressionNoIn returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=shiftExpression { $value = left; } ( 
		( LT { type= SyntaxExpressionType.LessThan; }
		| GT { type= SyntaxExpressionType.GreaterThan; }
		| LTE { type= SyntaxExpressionType.LessThanOrEqual; }
		| GTE { type= SyntaxExpressionType.GreaterThanOrEqual; }
		| INSTANCEOF { type= SyntaxExpressionType.InstanceOf;  } ) 
		right=shiftExpression { $value = new BinarySyntax(type, $value, right); })*
	;

// $>
	
// $<Equality operators (11.9)

equalityExpression returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=relationalExpression { $value = left; } ( 
		( EQ { type= SyntaxExpressionType.Equal; }
		| NEQ { type= SyntaxExpressionType.NotEqual; }
		| SAME { type= SyntaxExpressionType.Same; }
		| NSAME { type= SyntaxExpressionType.NotSame; }) 
		right=relationalExpression { $value = new BinarySyntax(type, $value, right); })*
	;

equalityExpressionNoIn returns [ExpressionSyntax value]
@init {
	SyntaxExpressionType type = SyntaxExpressionType.Unknown;
}
	: left=relationalExpressionNoIn { $value = left; } ( 
		( EQ { type= SyntaxExpressionType.Equal; }
		| NEQ { type= SyntaxExpressionType.NotEqual; }
		| SAME { type= SyntaxExpressionType.Same; }
		| NSAME { type= SyntaxExpressionType.NotSame; }) 
		right=relationalExpressionNoIn { $value = new BinarySyntax(type, $value, right); })*
	;

// $>
		
// $<Binary bitwise operators (11.10)

bitwiseANDExpression returns [ExpressionSyntax value]
	: left=equalityExpression { $value = left; } ( AND right=equalityExpression { $value = new BinarySyntax(SyntaxExpressionType.BitwiseAnd, $value, right); })*
	;

bitwiseANDExpressionNoIn returns [ExpressionSyntax value]
	: left=equalityExpressionNoIn { $value = left; } ( AND right=equalityExpressionNoIn { $value = new BinarySyntax(SyntaxExpressionType.BitwiseAnd, $value, right); })*
	;
		
bitwiseXORExpression returns [ExpressionSyntax value]
	: left=bitwiseANDExpression { $value = left; } ( XOR right=bitwiseANDExpression { $value = new BinarySyntax(SyntaxExpressionType.BitwiseExclusiveOr, $value, right); })*
	;
		
bitwiseXORExpressionNoIn returns [ExpressionSyntax value]
	: left=bitwiseANDExpressionNoIn { $value = left; } ( XOR right=bitwiseANDExpressionNoIn { $value = new BinarySyntax(SyntaxExpressionType.BitwiseExclusiveOr, $value, right); })*
	;
	
bitwiseORExpression returns [ExpressionSyntax value]
	: left=bitwiseXORExpression { $value = left; } ( OR right=bitwiseXORExpression { $value = new BinarySyntax(SyntaxExpressionType.BitwiseOr, $value, right); })*
	;
	
bitwiseORExpressionNoIn returns [ExpressionSyntax value]
	: left=bitwiseXORExpressionNoIn { $value = left; } ( OR right=bitwiseXORExpressionNoIn { $value = new BinarySyntax(SyntaxExpressionType.BitwiseOr, $value, right); })*
	;

// $>
	
// $<Binary logical operators (11.11)

logicalANDExpression returns [ExpressionSyntax value]
	:left= bitwiseORExpression { $value = left; } ( LAND right=bitwiseORExpression { $value = new BinarySyntax(SyntaxExpressionType.And, $value, right); })*
	;

logicalANDExpressionNoIn returns [ExpressionSyntax value]
	:left= bitwiseORExpressionNoIn { $value = left; } ( LAND right=bitwiseORExpressionNoIn { $value = new BinarySyntax(SyntaxExpressionType.And, $value, right); })*
	;
	
logicalORExpression returns [ExpressionSyntax value]
	: left=logicalANDExpression { $value = left; } ( LOR right=logicalANDExpression { $value = new BinarySyntax(SyntaxExpressionType.Or, $value, right); })*
	;
	
logicalORExpressionNoIn returns [ExpressionSyntax value]
	: left=logicalANDExpressionNoIn { $value = left; } ( LOR right=logicalANDExpressionNoIn { $value = new BinarySyntax(SyntaxExpressionType.Or, $value, right); } )*
	;

// $>
	
// $<Conditional operator (11.12)

conditionalExpression returns [ExpressionSyntax value]
	: expr1=logicalORExpression { $value = expr1; } ( QUE expr2=assignmentExpression COLON expr3=assignmentExpression { $value = new TernarySyntax(expr1, expr2, expr3); })?
	;

conditionalExpressionNoIn returns [ExpressionSyntax value]
	: expr1=logicalORExpressionNoIn { $value = expr1; } ( QUE expr2=assignmentExpressionNoIn COLON expr3=assignmentExpressionNoIn { $value = new TernarySyntax(expr1, expr2, expr3); })?
	;
	
// $>

// $<Assignment operators (11.13)

/*
The specification defines the AssignmentSyntax rule as follows:
AssignmentSyntax :
	ConditionalExpression 
	LeftHandSideExpression AssignmentOperator AssignmentSyntax
This rule has a LL(*) conflict. Resolving this with a syntactical predicate will yield something like this:

assignmentExpression
	: ( leftHandSideExpression assignmentOperator )=> leftHandSideExpression assignmentOperator assignmentExpression
	| conditionalExpression
	;
assignmentOperator
	: ASSIGN | MULASS | DIVASS | MODASS | ADDASS | SUBASS | SHLASS | SHRASS | SHUASS | ANDASS | XORASS | ORASS
	;
	
But that didn't seem to work. Terence Par writes in his book that LL(*) conflicts in general can best be solved with auto backtracking. But that would be 
a performance killer for such a heavy used rule.
The solution I came up with is to always invoke the conditionalExpression first and than decide what to do based on the result of that rule.
When the rule results in a Tree that can't be coming from a left hand side expression, then we're done.
When it results in a Tree that is coming from a left hand side expression and the LA(1) is an assignment operator then parse the assignment operator
followed by the right recursive call.
*/
assignmentExpression returns [ExpressionSyntax value]
@init {
    bool isLhs;
}
	:
        lhs=conditionalExpression
        { $value = lhs; isLhs = IsLeftHandSideAssign(lhs); }
	    (
            { isLhs }?
            ass=assignmentOperator
            exp=assignmentExpression
            {
                $value = new AssignmentSyntax(
                    ResolveAssignmentOperator($ass.text),
                    $value,
                    exp
                );
            }
        )?
	;

assignmentOperator 
	: ASSIGN
	| MULASS
	| DIVASS
	| MODASS
	| ADDASS
	| SUBASS
	| SHLASS
	| SHRASS
	| SHUASS
	| ANDASS
	| XORASS
	| ORASS
	;
	
assignmentExpressionNoIn returns [ExpressionSyntax value]
@init
{
	bool isLhs;
}
	:
        lhs=conditionalExpressionNoIn
        { $value = lhs; isLhs = IsLeftHandSideAssign(lhs); } 
	    (
            { isLhs }?
            ass=assignmentOperator
            exp=assignmentExpressionNoIn
            {
                $value = new AssignmentSyntax(
                    ResolveAssignmentOperator($ass.text),
                    $value,
                    exp
                );
            }
        )?
	;
	
// $>
	
// $<Comma operator (11.14)

expression returns [ExpressionSyntax value]
@init {
    List<ExpressionSyntax> nodes = null;
}
@after {
    if (nodes != null)
        $value = new CommaOperatorSyntax(nodes);
}
	:
        first=assignmentExpression
        { $value = first; }
        (
            COMMA
            follow=assignmentExpression
            {
                if (nodes == null)
                    nodes = new List<ExpressionSyntax> { $value };

                nodes.Add(follow);
            }
        )*
	;

expressionNoIn returns [ExpressionSyntax value]
@init {
    List<ExpressionSyntax> nodes = null;
}
@after {
    if (nodes != null)
        $value = new CommaOperatorSyntax(nodes);
}
	:
        first=assignmentExpressionNoIn
        { $value = first; }
        (
            COMMA
            follow=assignmentExpressionNoIn
            {
                if (nodes == null)
                    nodes = new List<ExpressionSyntax> { $value };

                nodes.Add(follow);
            }
        )* 
	;

// $>

// $>
	
//
// $<	A.4 Statements (12)
//

/*
This rule handles semicolons reported by the lexer and situations where the ECMA 3 specification states there should be semicolons automaticly inserted.
The auto semicolons are not actually inserted but this rule behaves as if they were.

In the following situations an ECMA 3 parser should auto insert absent but grammaticly required semicolons:
- the current token is a right brace
- the current token is the end of file (EOF) token
- there is at least one end of line (EOL) token between the current token and the previous token.

The RBRACE is handled by matching it but not consuming it.
The EOF needs no further handling because it is not consumed by default.
The EOL situation is handled by promoting the EOL or MultiLineComment with an EOL present from off channel to on channel
and thus making it parseable instead of handling it as white space. This promoting is done in the action PromoteEOL.
*/
semic
@init
{
	// Mark current position so we can unconsume a RBRACE.
	int marker = input.Mark();
	// Promote EOL if appropriate
	PromoteEol();
}
	: SEMIC
	| EOF
	| RBRACE { input.Rewind(marker); }
	| EOL
    | MultiLineComment // (with EOL in it)
	;

/*
To solve the ambiguity between block and objectLiteral via expressionStatement all but the block alternatives have been moved to statementTail.
Now when k = 1 and a semantical predicate is defined ANTLR generates code that always will prefer block when the LA(1) is a LBRACE.
This will result in the same behaviour that is described in the specification under 12.4 on the expressionStatement rule.
*/
statement returns [SyntaxNode value]
options
{
	k = 1 ;
}

	: { input.LA(1) == LBRACE }? b=block { $value = b; }
	| { input.LA(1) == FUNCTION }? func=functionDeclaration { $value = func; }
	| st=statementTail { $value = st; }
	;
	
statementTail returns [SyntaxNode value] 
	: vst=variableStatement { $value = vst; }
	| est=emptyStatement { $value = est; }
	| exst=expressionStatement { $value = exst; }
	| ifst=ifStatement { $value = ifst; }
	| itst=iterationStatement { $value = itst; }
	| cost=continueStatement { $value = cost; }
	| brst=breakStatement { $value = brst; }
	| rst=returnStatement { $value = rst; }
	| wist=withStatement { $value = wist; }
	| last=labelledStatement { $value = last; }
	| swst=switchStatement { $value = swst; }
	| thst=throwStatement { $value = thst; }
	| trst=tryStatement { $value = trst; }
	;

// $<Block (12.1)

block returns [BlockSyntax value] 
@init {
    var statements = new List<SyntaxNode>();
}
@after{
    $value = new BlockSyntax(statements);
}
	:
        lb=LBRACE
        (
            st=statement
            { statements.Add(st); }
        )*
        RBRACE
	;

// Used for the Function constructor, because it doesn't have braces.

blockStatements returns [BodySyntax value]
@init{
    var tempBody = _currentBody;
    _currentBody = new BodyBuilder();
}
@after{
    $value = _currentBody.CreateBody(BodyType.Function);
    _currentBody = tempBody;
}
	:
        (
            st=statement
            { _currentBody.Statements.Add(st); }
        )*
	;


// $>
	
// $<Variable statement 12.2)

variableStatement returns [SyntaxNode value]
@init {
    var declarations = new List<VariableDeclaration>();
    var start = input.LT(1);
}
@after {
    $value = new VariableDeclarationSyntax(declarations, GetLocation(start, input.LT(-1)));
}
	:
        VAR first=variableDeclaration
        { declarations.Add(first); }
        (
            COMMA follow=variableDeclaration
            { declarations.Add(follow); }
        )*
        semic
	;

variableDeclaration returns [VariableDeclaration value]
@init {
	ExpressionSyntax expression = null;
}
	:
        id=Identifier
        (
            ASSIGN ass=assignmentExpression
            { expression = ass; }
        )?
        { $value = new VariableDeclaration($id.Text, expression, true, _currentBody.DeclaredVariables.AddOrGet($id.Text, true)); }
	;
	
variableDeclarationNoIn returns [VariableDeclaration value]
@init {
	ExpressionSyntax expression = null;
}
	:
        id=Identifier
        (
            ASSIGN ass=assignmentExpressionNoIn
            { expression = ass; }
        )?
        { $value = new VariableDeclaration($id.Text, expression, true, _currentBody.DeclaredVariables.AddOrGet($id.Text, true)); }
	;

// $>
	
// $<Empty statement (12.3)

emptyStatement returns [SyntaxNode value]
	: SEMIC { $value = new EmptySyntax(); }
	;

// $>
	
// $<ExpressionSyntax statement (12.4)

/*
The look ahead check on LBRACE and FUNCTION the specification mentions has been left out and its function, resolving the ambiguity between:
- functionExpression and functionDeclaration
- block and objectLiteral
are moved to the statement and sourceElement rules.
*/
expressionStatement returns [SyntaxNode value]
@init {
    var start = input.LT(1);
}
	:
        e=expression semic
        { $value = new ExpressionStatementSyntax(e, GetLocation(start, input.LT(-1))); }
	;

// $>
	
// $<The if statement (12.5)

ifStatement returns [SyntaxNode value]
@init {
    SyntaxNode elseStatement = null;
    var start = input.LT(1);
    IToken end;
}
// The predicate is there just to get rid of the warning. ANTLR will handle the dangling else just fine.
	:
        IF LPAREN e=expression RPAREN
        { end = input.LT(-1); }
        then=statement
        (
            { input.LA(1) == ELSE }?
            ELSE els=statement
            { elseStatement = els; }
        )?
        { $value = new IfSyntax(e, then, elseStatement, GetLocation(start, end)); }
	;

// $>
	
// $<Iteration statements (12.6)

iterationStatement returns [SyntaxNode value]
	: dos=doStatement { $value = dos; }
	| wh=whileStatement  { $value = wh; }
	| fo=forStatement  { $value = fo; }
	;
	
doStatement returns [SyntaxNode value]
@init {
    IToken start;
}
	:
        DO st=statement
        { start = input.LT(1); }
        WHILE LPAREN e=expression RPAREN semic
        { $value = new DoWhileSyntax(e, st, GetLocation(start, input.LT(-1))); }
	;
	
whileStatement returns [SyntaxNode value]
@init {
    var start = input.LT(1);
    IToken end;
}
	:
        WHILE LPAREN e=expression RPAREN
        { end = input.LT(-1); }
        st=statement
        { $value = new WhileSyntax(e, st, GetLocation(start, end)); }
	;

/*
The forStatement production is refactored considerably as the specification
contains a very none LL(*) compliant definition.
The initial version was like this:	

forStatement
	: FOR LPAREN forControl RPAREN statement
	;
forControl
options
{
	backtrack = true ;
	//k = 3 ;
}
	: stepClause
	| iterationClause
	;
stepClause
options
{
	memoize = true ;
}
	: ( ex1=expressionNoIn | var=VAR variableDeclarationNoIn ( COMMA variableDeclarationNoIn )* )? SEMIC ex2=expression? SEMIC ex3=expression?
	-> { $var != null }? ( FORSTEP ( VAR[$var] variableDeclarationNoIn+ ) ( EXPR $ex2? ) ( EXPR $ex3? ) )
	-> ( FORSTEP ( EXPR $ex1? ) ( EXPR $ex2? ) ( EXPR $ex3? ) )
	;
iterationClause
options
{
	memoize = true ;
}
	: ( leftHandSideExpression | var=VAR variableDeclarationNoIn ) IN expression
	-> { $var != null }? ( FORITER ( VAR[$var] variableDeclarationNoIn ) ( EXPR expression ) )
	-> ( FORITER ( EXPR leftHandSideExpression ) ( EXPR expression ) )
	;
	
But this completely relies on the backtrack feature and capabilities of ANTLR. 
Furthermore backtracking seemed to have 3 major drawbacks:
- the performance cost of backtracking is considerably
- didn't seem to work well with ANTLRWorks
- when introducing a k value to optimize the backtracking away, ANTLR runs out of heap space
*/
forStatement returns [SyntaxNode value]
@init {
    ForBuilder builder;
    var start = input.LT(1);
    IToken end;
}
@after {
    $value = builder.CreateFor(this, GetLocation(start, end));
}
	:
        FOR
        LPAREN
        fo=forControl
        { builder = fo; }
        RPAREN
        { end = input.LT(-1); }
        st=statement
        { builder.Body = st; }
	;

forControl returns [ForBuilder value]
	:
        ex1=forControlVar
        { $value = ex1; }
	|
        ex2=forControlExpression
        { $value = ex2; }
	|
        ex3=forControlSemic
        { $value = ex3; }
	;

forControlVar returns [ForBuilder value]
@init {
    $value = new ForBuilder();
    var declarations = new List<VariableDeclaration>();
    IToken start;
    IToken end = null;
}
@after {
    $value.Initialization = new VariableDeclarationSyntax(declarations, GetLocation(start, end));
}
	:
        { start = input.LT(1); }
        VAR first=variableDeclarationNoIn
        { declarations.Add(first); }
	    (
		    (
                { end = input.LT(-1); }
			    IN ex=expression
                { $value.Expression = ex; }
		    )
		    |
		    (
			    (
                    COMMA follow=variableDeclarationNoIn
                    { declarations.Add(follow); }
                )*
                { end = input.LT(-1); }
			    SEMIC
                (
                    ex1=expression
                    { $value.Test = ex1;}
                )?
                SEMIC
                (
                    ex2=expression
                    { $value.Increment = ex2; }
                )?
		    )
	    )
	;

forControlExpression returns [ForBuilder value]
@init
{
    $value = new ForBuilder();
	bool isLhs;
}
	:
        ex1=expressionNoIn
        { $value.Initialization = ex1; isLhs = IsLeftHandSideIn(ex1); }
	    ( 
		    { isLhs }?
            (
			    IN ex2=expression
                { $value.Expression = ex2; }
		    )
		    |
		    (
			    SEMIC
                (
                    ex2=expression
                    { $value.Test = ex2;}
                )?
                SEMIC
                (
                    ex3=expression
                    { $value.Increment = ex3; }
                )?
		    )
	    )
	;

forControlSemic returns [ForBuilder value]
@init{
	$value = new ForBuilder();
}
	:
        SEMIC
        (
            ex1=expression
            { $value.Test = ex1;}
        )?
        SEMIC
        (
            ex2=expression
            { $value.Increment = ex2; }
        )?
	;

// $>
	
// $<The continue statement (12.7)

/*
The action with the call to PromoteEOL after CONTINUE is to enforce the semicolon insertion rule of the specification that there are
no line terminators allowed beween CONTINUE and the optional identifier.
As an optimization we check the la first to decide whether there is an identier following.
*/
continueStatement returns [SyntaxNode value]
@init { 
	string label = null;
    var start = input.LT(1);
}
	:
        CONTINUE
        { if (input.LA(1) == Identifier) PromoteEol(); }
        (
            lb=Identifier
            { label = $lb.Text; }
        )?
        semic
        { $value = new ContinueSyntax(label, GetLocation(start, input.LT(-1))); }
	;

// $>
	
// $<The break statement (12.8)

/*
The action with the call to PromoteEOL after BREAK is to enforce the semicolon insertion rule of the specification that there are
no line terminators allowed beween BREAK and the optional identifier.
As an optimization we check the la first to decide whether there is an identier following.
*/
breakStatement returns [SyntaxNode value]
@init { 
	string label = null;
    var start = input.LT(1);
}
	:
        BREAK
        { if (input.LA(1) == Identifier) PromoteEol(); }
        (
            lb=Identifier { label = $lb.Text; }
        )?
        semic
        { $value = new BreakSyntax(label, GetLocation(start, input.LT(-1))); }
	;

// $>
	
// $<The return statement (12.9)

/*
The action calling PromoteEOL after RETURN ensures that there are no line terminators between RETURN and the optional expression as the specification states.
When there are these get promoted to on channel and thus virtual semicolon wannabees.
So the folowing code:

return
1

will be parsed as:

return;
1;
*/
returnStatement returns [ReturnSyntax value]
@init {
    ExpressionSyntax returnExpression = null;
    var start = input.LT(1);
}
	:
        RETURN
        { PromoteEol(); }
        (
            expr=expression
            { returnExpression = expr; }
        )?
        semic
        { $value = new ReturnSyntax(returnExpression, GetLocation(start, input.LT(-1))); }
	;

// $>
	
// $<The with statement (12.10)

withStatement returns [SyntaxNode value]
@init {
    var start = input.LT(1);
    IToken end;
}
	:
        WITH LPAREN exp=expression RPAREN
        { end = input.LT(-1); }
        smt=statement
        { $value = new WithSyntax(exp, smt, GetLocation(start, end)); }
	;

// $>
	
// $<The switch statement (12.11)

switchStatement returns [SyntaxNode value]
@init {
    bool hadDefault = false;
    var cases = new List<SwitchCase>();
    var start = input.LT(1);
    IToken end;
}
	:
        SWITCH LPAREN e=expression RPAREN
        { end = input.LT(-1); }
        LBRACE
        (
            { !hadDefault }?=>
            def=defaultClause
            { hadDefault = true; cases.Add(def); }
        |
            cc=caseClause
            { cases.Add(cc); }
        )*
        RBRACE
        { $value = new SwitchSyntax(e, cases, GetLocation(start, end)); }
	;

caseClause returns [SwitchCase value]
@init {
    var statements = new List<SyntaxNode>();
    var start = input.LT(1);
    IToken end;
}
	:
        CASE e=expression COLON
        { end = input.LT(-1); }
        (
            st=statement
            { statements.Add(st); }
        )*
        {
            $value = new SwitchCase(
                e,
                new BlockSyntax(statements),
                GetLocation(start, end)
            );
        }
	;
	
defaultClause returns [SwitchCase value]
@init {
    var statements = new List<SyntaxNode>();
    var start = input.LT(1);
    IToken end;
}
	:
        DEFAULT COLON
        { end = input.LT(-1); }
        (
            st=statement
            { statements.Add(st); }
        ) *
        {
            $value = new SwitchCase(
                null,
                new BlockSyntax(statements),
                GetLocation(start, end)
            );
        }
	;

// $>
	
// $<Labelled statements (12.12)

labelledStatement returns [SyntaxNode value]
	:
        lb=Identifier COLON st=statement
        { $value = new LabelSyntax($lb.Text, st); }
	;

// $>
	
// $<The throw statement (12.13)

/*
The action calling PromoteEOL after THROW ensures that there are no line terminators between THROW and the expression as the specification states.
When there are line terminators these get promoted to on channel and thus to virtual semicolon wannabees.
So the folowing code:

throw
new Error()

will be parsed as:

throw;
new Error();

which will yield a recognition exception!
*/
throwStatement returns [SyntaxNode value]
@init {
    var start = input.LT(1);
}
	:
        THROW { PromoteEol(); } exp=expression semic
        { $value = new ThrowSyntax(exp, GetLocation(start, input.LT(-1))); }
	;

// $>
	
// $<The try statement (12.14)

tryStatement returns [TrySyntax value]
@init{
    CatchClause @catch = null;
    FinallyClause @finally = null;
}
	:
        TRY b=block
        (
            c=catchClause
            { @catch = c; }
            (
                first=finallyClause
                { @finally = first; }
            )?
        |
            last=finallyClause
            { @finally = last; }
        )
        { $value = new TrySyntax(b, @catch, @finally); }
	;
	
catchClause returns [CatchClause value]
	:
        CATCH LPAREN id=Identifier RPAREN b=block
        { $value = new CatchClause($id.text, b, _currentBody.DeclaredVariables.AddOrGet($id.text, true)); }
	;
	
finallyClause returns [FinallyClause value]
	:
        FINALLY b=block
        { $value = new FinallyClause(b); }
	;

// $>

// $>

//
// $<	A.5 Functions and Programs (13, 14)
//

// $<	Function Definition (13)

functionDeclaration returns [SyntaxNode value]
@init {
    var start = input.LT(1);
    string name;
    List<string> parameters;
    BodySyntax body;
}
@after {
    _currentBody.FunctionDeclarations.Add(
        new FunctionSyntax(
            name,
            parameters,
            body,
            _currentBody.DeclaredVariables.AddOrGet(name, true),
            GetLocation(start, input.LT(-1))
        )
    );

    $value = new EmptySyntax();
}
	:
        FUNCTION id=Identifier
        { name = $id.Text; } 
		parms=formalParameterList
        { parameters = parms; }
		fb=functionBody
        { body = fb; }
	;

functionExpression returns [FunctionSyntax value]
@init {
    var start = input.LT(1);
    string name = null;
    List<string> parameters;
    BodySyntax body;
}
@after {
	$value = new FunctionSyntax(
        name,
        parameters,
        body,
        name == null ? null : _currentBody.DeclaredVariables.AddOrGet(name, true),
        GetLocation(start, input.LT(-1))
    );
}
	:
        FUNCTION
        (
            id=Identifier
            { name = $id.Text; }
        )?
        fpl=formalParameterList
        { parameters = fpl; }
        fb=functionBody
        { body = fb; }
	;

formalParameterList returns [List<string> value]
@init {
    List<string> identifiers = new List<string>();
    $value = identifiers;
}
	:
        LPAREN
        (
            first=Identifier
            { identifiers.Add($first.text); }
            (
                COMMA follow=Identifier
                { identifiers.Add($follow.text); }
            )*
        )?
        RPAREN
	;

functionBody returns [BodySyntax value]
@init{
    var tempBody = _currentBody;
    _currentBody = new BodyBuilder();
    var start = input.LT(1);
}
@after{
    $value = _currentBody.CreateBody(BodyType.Function);
    _currentBody = tempBody;
}
	:
        lb=LBRACE
        (
            se=sourceElement
            { _currentBody.Statements.Add(se); }
        )*
        RBRACE
	;

// $>
	
// $<	Program (14)

program returns [ProgramSyntax value]
@init{
    _currentBody = new BodyBuilder();
}
	:
        (
            follow=sourceElement
            { _currentBody.Statements.Add(follow); }
        )*
        { $value = new ProgramSyntax(_currentBody.CreateBody(BodyType.Program)); }
	;

/*
By setting k  to 1 for this rule and adding the semantical predicate ANTRL will generate code that will always prefer functionDeclararion over functionExpression
here and therefor remove the ambiguity between these to production.
This will result in the same behaviour that is described in the specification under 12.4 on the expressionStatement rule.
*/
sourceElement returns [SyntaxNode value]
options
{
	k = 1 ;
}

	: { input.LA(1) == FUNCTION }? func=functionDeclaration { $value = func; }
	| stat=statement { $value = stat; }
	;

// $>

// $>


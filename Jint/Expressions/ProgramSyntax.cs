using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ProgramSyntax : SyntaxNode
    {
        private bool? _isLiteral;

        public override SyntaxType Type
        {
            get { return SyntaxType.Program; }
        }

        public BodySyntax Body { get; private set; }

        public override bool IsLiteral
        {
            get
            {
                if (!_isLiteral.HasValue)
                {
                    bool hadLiteral = false;

                    // We specifically support:
                    //
                    // * A single statement that itself is a literal;
                    // * A single return statement that returns a literal;
                    // * And skip over empty statements.
                    //

                    foreach (var statement in Body.Statements)
                    {
                        if (statement.IsLiteral)
                        {
                            if (hadLiteral)
                            {
                                _isLiteral = false;
                                break;
                            }

                            hadLiteral = true;
                        }
                        else if (statement.Type == SyntaxType.Return)
                        {
                            if (((ReturnSyntax)statement).Expression.IsLiteral)
                            {
                                if (hadLiteral)
                                {
                                    _isLiteral = false;
                                    break;
                                }

                                hadLiteral = true;
                            }
                        }
                        else if (statement.Type != SyntaxType.Empty)
                        {
                            _isLiteral = false;
                            break;
                        }
                    }

                    if (!_isLiteral.HasValue)
                        _isLiteral = hadLiteral;
                }

                return _isLiteral.Value;
            }
        }

        public ProgramSyntax(BodySyntax body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitProgram(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitProgram(this);
        }
    }
}

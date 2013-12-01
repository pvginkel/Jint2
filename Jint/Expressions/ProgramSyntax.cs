using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ProgramSyntax : BlockSyntax
    {
        private bool? _isLiteral;

        internal override bool IsLiteral
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

                    foreach (var statement in Statements)
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
                            break;
                        }
                    }

                    if (!_isLiteral.HasValue)
                        _isLiteral = hadLiteral;
                }

                return _isLiteral.Value;
            }
        }

        public ProgramSyntax(IEnumerable<SyntaxNode> statements)
            : this(statements, null)
        {
        }

        internal ProgramSyntax(IEnumerable<SyntaxNode> statements, VariableCollection declaredVariables)
            : base(statements, declaredVariables)
        {
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

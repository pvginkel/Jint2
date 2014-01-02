using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Ast;

namespace Jint.Parser
{
    internal class ForBuilder
    {
        private readonly AstBuilder _builder;

        public SyntaxNode Body { get; set; }
        public SyntaxNode Initialization { get; set; }
        public ExpressionSyntax Expression { get; set; }
        public ExpressionSyntax Test { get; set; }
        public ExpressionSyntax Increment { get; set; }

        public ForBuilder(AstBuilder builder)
        {
            _builder = builder;
        }

        public SyntaxNode BuildFor(EcmaScriptParser parser, SourceLocation location)
        {
            if (Expression != null)
            {
                IIdentifier identifier;

                if (Initialization is IdentifierSyntax)
                {
                    identifier = ((IdentifierSyntax)Initialization).Identifier;
                }
                else
                {
                    var variableDeclaration = (VariableDeclarationSyntax)Initialization;

                    Debug.Assert(variableDeclaration.Declarations.Count == 1);

                    var declaration = variableDeclaration.Declarations[0];

                    identifier = declaration.Identifier;

                    Debug.Assert(declaration.Expression == null);
                }

                return _builder.BuildForEachIn(
                    identifier,
                    Expression,
                    Body,
                    location
                );
            }

            return _builder.BuildFor(
                Initialization,
                Test,
                Increment,
                Body,
                location
            );
        }
    }
}

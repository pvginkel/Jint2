using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Parser
{
    internal partial class AstBuilder
    {
        private Scope _scope;

        public void EnterProgramBody()
        {
            EnterBody(BodyType.Program, ReadOnlyArray<string>.Null);
        }

        public void EnterFunctionBody(ReadOnlyArray<string> parameters)
        {
            EnterBody(BodyType.Function, parameters);
        }

        private void EnterBody(BodyType bodyType, ReadOnlyArray<string> parameters)
        {
            _scope = new Scope(bodyType, _scope, parameters);
        }

        public BodySyntax ExitBody()
        {
            var result = _scope.BuildBody();

            _scope = _scope.Parent;

            return result;
        }

        public void EnterWith()
        {
            _scope.EnterWith();
        }

        public WithSyntax ExitWith(ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
        {
            return _scope.ExitWith(expression, body, location);
        }

        public void EnterCatch(string name)
        {
            _scope.EnterCatch(name);
        }

        public CatchClause ExitCatch(SyntaxNode statement)
        {
            return _scope.ExitCatch(statement);
        }

        public void AddStatement(SyntaxNode node)
        {
            _scope.AddStatement(node);
        }

        public void AddDeclaredFunction(FunctionSyntax function)
        {
            _scope.AddDeclaredFunctions(function);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Backend.Dlr
{
    internal class VariableMarkerPhase : SyntaxVisitor
    {
        private readonly DlrBackend _backend;
        private readonly bool _isStrict;
        private readonly List<BlockSyntax> _blocks = new List<BlockSyntax>();
        private BlockSyntax _main;

        public VariableMarkerPhase(DlrBackend backend)
        {
            _backend = backend;
            _isStrict = backend.Options.HasFlag(Options.Strict);
        }

        public override void VisitProgram(ProgramSyntax syntax)
        {
            // Mark all variables as global.

            foreach (var variable in syntax.DeclaredVariables)
            {
                variable.Type = VariableType.Global;
            }

            // Add the "this" variable.

            syntax.DeclareVariable(JsScope.This).Type = VariableType.This;

            _main = syntax;
            _blocks.Add(syntax);

            base.VisitProgram(syntax);
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            ProcessFunction(syntax);

            _blocks.Add(syntax.Body);

            base.VisitFunctionDeclaration(syntax);

            _blocks.RemoveAt(_blocks.Count - 1);
        }

        public override void VisitFunction(FunctionSyntax syntax)
        {
            ProcessFunction(syntax);

            _blocks.Add(syntax.Body);

            base.VisitFunction(syntax);

            _blocks.RemoveAt(_blocks.Count - 1);
        }

        private void ProcessFunction(IFunctionDeclaration function)
        {
            var body = function.Body;
            var declaredVariables = body.DeclaredVariables;

            // Setup the "arguments" and "this" variables.

            Variable variable;
            if (!declaredVariables.TryGetItem(JsScope.Arguments, out variable))
                body.DeclareVariable(JsScope.Arguments).Type = VariableType.Arguments;
            else if (_isStrict)
                throw new InvalidOperationException("Cannot use 'arguments' as a parameter name in strict mode");

            body.DeclareVariable(JsScope.This).Type = VariableType.This;

            // Check for strict mode.

            if (_isStrict && declaredVariables.Contains("eval"))
                throw new InvalidOperationException("Cannot use 'eval' as a parameter name in strict mode");

            // Add or mark the parameters.

            foreach (var parameter in function.Parameters)
            {
                variable = body.DeclareVariable(parameter);

                if (variable.Type == VariableType.Unknown)
                    variable.Type = VariableType.Parameter;
            }

            // Mark the rest of the declared variables as locals.

            foreach (var item in declaredVariables)
            {
                if (item.Type == VariableType.Unknown)
                    item.Type = VariableType.Local;
            }
        }

        public override void VisitIdentifier(IdentifierSyntax syntax)
        {
            syntax.Target = GetVariable(syntax.Name);

            base.VisitIdentifier(syntax);
        }

        private Variable GetVariable(string identifier)
        {
            Variable variable;

            // Try to find the identifier in a scope other than the global scope.

            int count = _blocks.Count;
            for (int i = count - 1; i > 0; i--)
            {
                if (_blocks[i].DeclaredVariables.TryGetItem(identifier, out variable))
                {
                    if (variable.Type != VariableType.Global && i < count - 1)
                    {
                        Debug.Assert(variable.Type != VariableType.Unknown);
                        Debug.Assert(
                            variable.Type == VariableType.Local ||
                            variable.Type == VariableType.Parameter
                        );

                        variable.IsClosedOver = true;
                    }

                    return variable;
                }
            }

            // Else, it's a reference to a global variable.

            variable = _main.DeclareVariable(identifier);

            if (variable.Type == VariableType.Unknown)
                variable.Type = VariableType.Global;
            else
                Debug.Assert(variable.Type == VariableType.Global);

            return variable;
        }
    }
}

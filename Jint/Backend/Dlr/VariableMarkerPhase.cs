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
        private readonly bool _isStrict;
        private readonly List<BlockManager> _blocks = new List<BlockManager>();
        private readonly List<BlockManager> _pendingClosures = new List<BlockManager>();
        private BlockSyntax _main;

        public VariableMarkerPhase(DlrBackend backend)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

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
            _blocks.Add(new BlockManager(syntax, null));

            base.VisitProgram(syntax);

            // Build all pending closures.

            foreach (var block in _pendingClosures)
            {
                BuildClosure(block);
            }
        }

        private void BuildClosure(BlockManager block)
        {
            if (block.Block.Closure != null)
                return;

            // Find the parent closure.

            var parent = block.Parent;

            while (parent != null)
            {
                if (parent.ClosedOverVariables.Count > 0)
                    break;

                parent = parent.Parent;
            }

            // If the parent hasn't already been built, build it.

            if (parent != null && parent.Block.Closure == null)
                BuildClosure(parent);

            // Build the closure.

            var fields = new Dictionary<string, Type>();

            // Add the variables that were closed over.

            foreach (var variable in block.ClosedOverVariables)
            {
                fields.Add(variable.Name, typeof(JsInstance));
            }

            // If we have a parent closure, add a variable for that.

            if (parent != null)
                fields.Add(Closure.ParentFieldName, parent.Block.Closure.Type);

            // Build the closure.

            var closureType = ClosureManager.BuildClosure(fields);
            var closure = new Closure(
                closureType,
                parent != null ? parent.Block.Closure : null
            );

            block.Block.Closure = closure;
            block.Block.ParentClosure = parent != null ? parent.Block.Closure : null;

            // Fix up all variables.

            foreach (var variable in block.ClosedOverVariables)
            {
                variable.ClosureField = new ClosedOverVariable(
                    closure,
                    variable,
                    closureType.GetField(variable.Name)
                );
            }
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            EnterFunction(syntax);

            base.VisitFunctionDeclaration(syntax);

            ExitFunction(syntax);
        }

        public override void VisitFunction(FunctionSyntax syntax)
        {
            EnterFunction(syntax);

            base.VisitFunction(syntax);

            ExitFunction(syntax);
        }

        private void EnterFunction(IFunctionDeclaration function)
        {
            var body = function.Body;
            var declaredVariables = body.DeclaredVariables;

            // Setup the "arguments" and "this" variables.

            if (_isStrict && declaredVariables.Contains(JsScope.Arguments))
                throw new InvalidOperationException("Cannot use 'arguments' as a parameter name in strict mode");

            // Check for strict mode.

            if (_isStrict && declaredVariables.Contains("eval"))
                throw new InvalidOperationException("Cannot use 'eval' as a parameter name in strict mode");

            // Add or mark the parameters.

            foreach (var parameter in function.Parameters)
            {
                var variable = body.DeclareVariable(parameter);

                if (variable.Type == VariableType.Unknown)
                    variable.Type = VariableType.Parameter;
            }

            // Mark the rest of the declared variables as locals.

            foreach (var item in declaredVariables)
            {
                if (item.Type == VariableType.Unknown)
                    item.Type = VariableType.Local;
            }

            // Add ourselves to the top of the block stack.

            _blocks.Add(new BlockManager(function.Body, _blocks[_blocks.Count - 1]));
        }

        private void ExitFunction(IFunctionDeclaration function)
        {
            // Add ourselves to the pending closures list if we need a closure
            // to be built.

            var block = _blocks[_blocks.Count - 1];

            if (block.ClosedOverVariables.Count != 0)
                _pendingClosures.Add(block);

            // Remove ourselves from the top of the block stack.

            _blocks.RemoveAt(_blocks.Count - 1);
        }

        public override void VisitIdentifier(IdentifierSyntax syntax)
        {
            syntax.Target = GetVariable(syntax.Name);

            base.VisitIdentifier(syntax);
        }

        private Variable GetVariable(string identifier)
        {
            // Arguments can be re-declared (of not strict). Because of this,
            // we check arguments after resolving in a scope.

            if (identifier == JsScope.This)
                return Variable.This;

            Variable variable;

            // Try to find the identifier in a scope other than the global scope.

            int count = _blocks.Count;
            for (int i = count - 1; i > 0; i--)
            {
                if (_blocks[i].Block.DeclaredVariables.TryGetItem(identifier, out variable))
                {
                    if (variable.Type != VariableType.Global && i < count - 1)
                    {
                        Debug.Assert(variable.Type != VariableType.Unknown);
                        Debug.Assert(
                            variable.Type == VariableType.Local ||
                            variable.Type == VariableType.Parameter
                        );

                        _blocks[i].ClosedOverVariables.Add(variable);
                    }

                    return variable;
                }
            }

            // Check for arguments.

            if (identifier == JsScope.Arguments)
                return Variable.Arguments;

            // Else, it's a reference to a global variable.

            variable = _main.DeclareVariable(identifier);

            if (variable.Type == VariableType.Unknown)
                variable.Type = VariableType.Global;
            else
                Debug.Assert(variable.Type == VariableType.Global);

            return variable;
        }

        private class BlockManager
        {
            public BlockSyntax Block { get; private set; }
            public BlockManager Parent { get; private set; }
            public HashSet<Variable> ClosedOverVariables { get; private set; }

            public BlockManager(BlockSyntax block, BlockManager parent)
            {
                Block = block;
                Parent = parent;
                ClosedOverVariables = new HashSet<Variable>();
            }
        }
    }
}

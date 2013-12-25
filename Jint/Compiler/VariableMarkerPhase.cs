using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Compiler
{
    internal class VariableMarkerPhase : SyntaxVisitor
    {
#if DEBUG
        private const string WithPrefix = "__with";
#else
        private const string WithPrefix = "<>with";
#endif

        private readonly bool _isStrict;
        private readonly List<BlockManager> _blocks = new List<BlockManager>();
        private readonly List<BlockManager> _pendingClosures = new List<BlockManager>();
        private BodySyntax _main;
        private MarkerWithScope _withScope;
        private int _nextWithScopeIndex = 1;

        public VariableMarkerPhase(JintEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _isStrict = engine.Options.HasFlag(Options.Strict);
        }

        public override void VisitProgram(ProgramSyntax syntax)
        {
            // Mark all variables as global.

            foreach (var variable in syntax.Body.DeclaredVariables)
            {
                variable.Type = VariableType.Global;
            }

            _main = syntax.Body;
            _blocks.Add(new BlockManager(syntax.Body, null, null));

            base.VisitProgram(syntax);

            if (_blocks[0].ClosedOverVariables.Count != 0)
                _pendingClosures.Add(_blocks[0]);

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

            var fields = new List<string>();

            // Add the variables that were closed over.

            bool requireArguments = false;

            foreach (var variable in block.ClosedOverVariables)
            {
                if (variable.Type == VariableType.Local)
                    fields.Add(variable.Name);
                else
                    requireArguments = true;
            }

            if (requireArguments)
                fields.Add(Closure.ArgumentsFieldName);

            // Build the closure.

            var closure = new Closure(
                parent != null ? parent.Block.Closure : null,
                fields
            );

            block.Block.Closure = closure;
            block.Block.ParentClosure = parent != null ? parent.Block.Closure : null;

            // Fix up all variables.

            foreach (var variable in block.ClosedOverVariables)
            {
                if (variable.Type == VariableType.Parameter)
                {
                    if (block.ArgumentsVariable.ClosureField == null)
                    {
                        block.ArgumentsVariable.ClosureField = new ClosedOverVariable(
                            closure,
                            block.ArgumentsVariable
                        );
                    }

                    variable.ClosureField = block.ArgumentsVariable.ClosureField;
                }
                else
                {
                    variable.ClosureField = new ClosedOverVariable(
                        closure,
                        variable
                    );
                }
            }
        }

        public override void VisitFunction(FunctionSyntax syntax)
        {
            if (_main == null)
                _main = syntax.Body;

            var body = syntax.Body;
            var declaredVariables = body.DeclaredVariables;

            // Setup the "arguments" and "this" variables.

            Variable argumentsVariable;
            if (declaredVariables.TryGetValue(JsNames.Arguments, out argumentsVariable))
            {
                if (_isStrict)
                    throw new InvalidOperationException("Cannot use 'arguments' as a parameter name in strict mode");
            }
            else
            {
                argumentsVariable = new Variable(JsNames.Arguments, -1)
                {
                    Type = VariableType.Arguments
                };

                declaredVariables.Add(argumentsVariable);
            }

            // Check for strict mode.

            if (_isStrict && declaredVariables.Contains("eval"))
                throw new InvalidOperationException("Cannot use 'eval' as a parameter name in strict mode");

            // Add or mark the parameters.

            for (int i = syntax.Parameters.Count - 1; i >= 0; i--)
            {
                var variable = body.DeclaredVariables.AddOrGet(syntax.Parameters[i], i);

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

            BlockManager parentBlock = null;
            if (_blocks.Count > 0)
                parentBlock = _blocks[_blocks.Count - 1];

            _blocks.Add(new BlockManager(syntax.Body, argumentsVariable, parentBlock));

            base.VisitFunction(syntax);

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

        public override void VisitWith(WithSyntax syntax)
        {
            syntax.Expression.Accept(this);

            syntax.Target = new Variable(WithPrefix + (_nextWithScopeIndex++).ToString(CultureInfo.InvariantCulture), -1)
            {
                Type = VariableType.Local
            };

            var block = _blocks[_blocks.Count - 1];

            block.Block.DeclaredVariables.Add(syntax.Target);

            _withScope = new MarkerWithScope(_withScope, syntax.Target, block);

            syntax.Body.Accept(this);

            _withScope = _withScope.Parent;
        }

        private Variable GetVariable(string identifier)
        {
            // Arguments can be re-declared (if not strict). Because of this,
            // we check arguments after resolving in a scope.

            if (identifier == JsNames.This)
                return Variable.This;

            Variable variable;

            // Try to find the identifier in a scope other than the global scope.
            // If we're parsing a function constructor, we don't have a main,
            // so we don't skip over the global scope.

            bool haveMain = _blocks[0].Block.BodyType == BodyType.Program;

            int count = _blocks.Count;
            for (int i = count - 1; i >= (haveMain ? 1 : 0); i--)
            {
                if (_blocks[i].Block.DeclaredVariables.TryGetValue(identifier, out variable))
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

                    if (_withScope != null)
                    {
                        if (i < count - 1)
                            CloseOverWithScope();

                        return new Variable(variable, _withScope.WithScope);
                    }

                    return variable;
                }
            }

            // Check for arguments.

            Debug.Assert(identifier != JsNames.Arguments);

            // Else, it's a reference to a global variable.

            variable = _main.DeclaredVariables.AddOrGet(identifier);

            if (variable.Type == VariableType.Unknown)
                variable.Type = VariableType.Global;
            else
                Debug.Assert(variable.Type == VariableType.Global);

            if (_withScope != null)
            {
                if (_blocks.Count > 1)
                    CloseOverWithScope();

                return new Variable(variable, _withScope.WithScope);
            }

            return variable;
        }

        private void CloseOverWithScope()
        {
            var withScope = _withScope;

            while (withScope != null)
            {
                withScope.Block.ClosedOverVariables.Add(withScope.WithScope.Variable);

                withScope = withScope.Parent;
            }
        }

        private class MarkerWithScope
        {
            public WithScope WithScope { get; private set; }
            public MarkerWithScope Parent { get; private set; }
            public BlockManager Block { get; private set; }

            public MarkerWithScope(MarkerWithScope parent, Variable variable, BlockManager block)
            {
                Parent = parent;
                WithScope = new WithScope(parent != null ? parent.WithScope : null, variable);
                Block = block;
            }
        }

        private class BlockManager
        {
            public BodySyntax Block { get; private set; }
            public Variable ArgumentsVariable { get; private set; }
            public BlockManager Parent { get; private set; }
            public HashSet<Variable> ClosedOverVariables { get; private set; }

            public BlockManager(BodySyntax block, Variable argumentsVariable, BlockManager parent)
            {
                Block = block;
                ArgumentsVariable = argumentsVariable;
                Parent = parent;
                ClosedOverVariables = new HashSet<Variable>();
            }
        }
    }
}

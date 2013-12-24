using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal static class TypeMarkerPhase
    {
        public static void Perform(BoundProgram node)
        {
            Perform(node.Body, false);
        }

        public static void Perform(BoundFunction node)
        {
            Perform(node.Body, true);
        }

        private static void Perform(BoundBody body, bool isFunction)
        {
            new Marker(body.TypeManager, isFunction).Visit(body);
        }

        private class Marker : BoundTreeWalker
        {
            private readonly BoundTypeManager _typeManager;
            private readonly bool _isFunction;
            private BoundTypeManager.TypeMarker _marker;

            public Marker(BoundTypeManager typeManager, bool isFunction)
            {
                _typeManager = typeManager;
                _isFunction = isFunction;
            }

            public override void VisitBody(BoundBody node)
            {
                using (_marker = _typeManager.CreateTypeMarker())
                {
                    // Mark the arguments local as object.

                    if (_isFunction)
                    {
                        var argumentsLocal = (BoundVariable)node.Locals.SingleOrDefault(p => p.Name == "arguments");
                        if (argumentsLocal == null)
                        {
                            Debug.Assert(node.Closure != null);
                            argumentsLocal = node.Closure.Fields[Closure.ArgumentsFieldName];
                        }

                        MarkWrite(argumentsLocal, BoundValueType.Object);
                    }

                    base.VisitBody(node);
                }
            }

            private void MarkRead(IBoundReadable variable)
            {
                MarkClosureUsage(variable);
            }

            private void MarkWrite(IBoundWritable variable, BoundValueType type)
            {
                var hasType = variable as BoundVariable;
                if (hasType != null)
                    _marker.MarkWrite(hasType.Type, type);

                MarkClosureUsage(variable);
            }

            private void MarkClosureUsage(IBoundReadable variable)
            {
                switch (variable.Kind)
                {
                    case BoundVariableKind.ClosureField:
                        _marker.MarkClosureUsage(((BoundClosureField)variable).Closure);
                        break;

                    case BoundVariableKind.Argument:
                        var argument = (BoundArgument)variable;
                        if (argument.ArgumentsClosureField != null)
                            _marker.MarkClosureUsage(argument.ArgumentsClosureField.Closure);
                        break;
                }
            }

            public override void VisitGetVariable(BoundGetVariable node)
            {
                MarkRead(node.Variable);
            }

            public override void VisitExpressionBlock(BoundExpressionBlock node)
            {
                base.VisitExpressionBlock(node);

                MarkRead(node.Result);
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                base.VisitSetVariable(node);

                MarkWrite(node.Variable, node.Value.ValueType);
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                MarkWrite(node.Target, BoundValueType.String);

                base.VisitForEachIn(node);
            }

            public override void VisitCatch(BoundCatch node)
            {
                MarkWrite(node.Target, BoundValueType.Unknown);

                base.VisitCatch(node);
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                // It's the responsibility of the phase to correctly process
                // functions.

                Perform(node.Function.Body, true);
            }
        }
    }
}

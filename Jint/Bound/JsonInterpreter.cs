using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    internal class JsonInterpreter : BoundTreeVisitor<bool>
    {
        private readonly JsGlobal _global;
        private Target _currentTarget;

        public object Result { get; private set; }

        public JsonInterpreter(JsGlobal global)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            _global = global;
        }

        public override bool DefaultVisit(BoundNode node)
        {
            Debug.Assert(Result == null);

            var expression = node as BoundExpression;

            if (expression != null)
            {
                object value;
                if (TryResolveValue(expression, out value))
                {
                    Result = value;
                    return true;
                }
            }

            return false;
        }

        public override bool VisitBody(BoundBody node)
        {
            // Detect whether the body looks like a JSON expression.

            bool hadLiteral = false;

            foreach (var statement in node.Body.Nodes)
            {
                if (statement.Kind == BoundKind.Empty)
                    continue;

                if (
                    statement.Kind == BoundKind.Return ||
                    statement.Kind == BoundKind.ExpressionStatement
                ) {
                    if (hadLiteral)
                        return false;

                    hadLiteral = true;
                }
                else
                {
                    return false;
                }
            }

            if (!hadLiteral)
            {
                Result = JsUndefined.Instance;
                return true;
            }

            foreach (var statement in node.Body.Nodes)
            {
                if (!statement.Accept(this))
                    return false;
            }

            Debug.Assert(Result != null);

            return true;
        }

        public override bool VisitEmpty(BoundEmpty node)
        {
            return true;
        }

        public override bool VisitReturn(BoundReturn node)
        {
            Debug.Assert(Result == null);

            if (node.Expression == null)
            {
                Result = JsUndefined.Instance;
                return true;
            }

            return node.Expression.Accept(this);
        }

        public override bool VisitExpressionStatement(BoundExpressionStatement node)
        {
            return node.Expression.Accept(this);
        }

        public override bool VisitSetVariable(BoundSetVariable node)
        {
            var temporary = node.Variable as BoundTemporary;
            if (temporary == null)
                return false;

            object value;
            if (!TryResolveValue(node.Value, out value))
                return false;

            Debug.Assert(_currentTarget.Temporary == temporary);

            if (_currentTarget.Temporary != temporary)
                return false;

            _currentTarget.Value = value;
            return true;
        }

        public override bool VisitSetMember(BoundSetMember node)
        {
            object target;
            if (!TryResolveValue(node.Expression, out target))
                return false;

            object index;
            if (!TryResolveValue(node.Index, out index))
                return false;

            object value;
            if (!TryResolveValue(node.Value, out value))
                return false;

            ((JsObject)target).DefineProperty(index, value, PropertyAttributes.None);
            return true;
        }

        private bool TryResolveValue(BoundExpression node, out object value)
        {
            switch (node.Kind)
            {
                case BoundKind.Constant:
                    value = ((BoundConstant)node).Value;
                    return true;

                case BoundKind.GetVariable:
                    var getVariable = (BoundGetVariable)node;

                    if (getVariable.Variable.Kind == BoundVariableKind.Magic)
                    {
                        var magic = (BoundMagicVariable)getVariable.Variable;

                        switch (magic.VariableType)
                        {
                            case BoundMagicVariableType.Null:
                                value = JsNull.Instance;
                                return true;

                            case BoundMagicVariableType.Undefined:
                                value = JsUndefined.Instance;
                                return true;
                        }
                    }
                    else if (getVariable.Variable.Kind == BoundVariableKind.Temporary)
                    {
                        return TryGetValue((BoundTemporary)getVariable.Variable, out value);
                    }

                    value = null;
                    return false;

                case BoundKind.NewBuiltIn:
                    switch (((BoundNewBuiltIn)node).NewBuiltInType)
                    {
                        case BoundNewBuiltInType.Array:
                            value = _global.CreateArray();
                            return true;

                        case BoundNewBuiltInType.Object:
                            value = _global.CreateObject();
                            return true;

                        default:
                            throw new InvalidOperationException();
                    }

                case BoundKind.ExpressionBlock:
                    var expressionBlock = (BoundExpressionBlock)node;
                    var target = expressionBlock.Result as BoundTemporary;
                    if (target == null)
                    {
                        value = null;
                        return false;
                    }

                    var lastTarget = _currentTarget;
                    _currentTarget.Temporary = target;

                    foreach (var statement in expressionBlock.Body.Nodes)
                    {
                        if (!statement.Accept(this))
                        {
                            value = null;
                            return false;
                        }
                    }

                    if (!TryGetValue(target, out value))
                        return false;

                    _currentTarget = lastTarget;
                    return true;
                    
                default:
                    value = null;
                    return false;
            }
        }

        private bool TryGetValue(BoundTemporary temporary, out object value)
        {
            Debug.Assert(_currentTarget.Temporary == temporary);

            value = _currentTarget.Value;
            return temporary == _currentTarget.Temporary;
        }

        private struct Target
        {
            public BoundTemporary Temporary { get; set; }
            public object Value { get; set; }
        }
    }
}

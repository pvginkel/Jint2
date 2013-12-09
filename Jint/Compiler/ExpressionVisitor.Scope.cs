﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Compiler
{
    partial class ExpressionVisitor
    {
        private class Scope
        {
            private readonly ExpressionVisitor _visitor;
            private readonly List<Expression> _statements;
            public Scope Parent { get; private set; }
            public Dictionary<Variable, ParameterExpression> Variables { get; private set; }
            public ParameterExpression Runtime { get; private set; }
            public ParameterExpression This { get; private set; }
            public Closure Closure { get; private set; }
            public Variable ArgumentsVariable { get; private set; }
            public ParameterExpression Function { get; private set; }
            public ParameterExpression ClosureLocal { get; private set; }
            public LabelTarget Return { get; private set; }
            public Dictionary<Closure, ParameterExpression> ClosureLocals { get; private set; }
            public Stack<LabelTarget> BreakTargets { get; private set; }
            public Stack<LabelTarget> ContinueTargets { get; private set; }

            public Scope(ExpressionVisitor visitor, ParameterExpression @this, Closure closure, ParameterExpression function, ParameterExpression closureLocal, Variable argumentsVariable, List<Expression> statements, Scope parent)
            {
                if (visitor == null)
                    throw new ArgumentNullException("visitor");
                if (statements == null)
                    throw new ArgumentNullException("statements");

                _visitor = visitor;
                _statements = statements;
                This = @this;
                Closure = closure;
                Function = function;
                ClosureLocal = closureLocal;
                ArgumentsVariable = argumentsVariable;
                Runtime = Expression.Parameter(typeof(JintRuntime), RuntimeParameterName);
                Variables = new Dictionary<Variable, ParameterExpression>();
                Parent = parent;
                Return = Expression.Label(typeof(object), "return");
                ClosureLocals = new Dictionary<Closure, ParameterExpression>();
                BreakTargets = new Stack<LabelTarget>();
                ContinueTargets = new Stack<LabelTarget>();
            }

            private Expression FindVariable(Variable variable)
            {
                Debug.Assert(
                    variable.Type == VariableType.Local ||
                    variable.Type == VariableType.Arguments ||
                    variable.Type == VariableType.This
                );

                switch (variable.Type)
                {
                    case VariableType.This:
                        return This;

                    case VariableType.Local:
                    case VariableType.Arguments:
                        var closureField = variable.ClosureField;

                        if (closureField == null)
                            return Variables[variable];

                        return Expression.Field(
                            BuildClosureAliases(closureField.Closure),
                            closureField.Field
                        );

                    default:
                        throw new InvalidOperationException("Cannot find variable of argument");
                }
            }

            public Expression BuildSet(Variable variable, Expression value)
            {
                switch (variable.Type)
                {
                    case VariableType.Global:
                        return _visitor.BuildSetMember(
                            Expression.Property(Runtime, "GlobalScope"),
                            variable.Name,
                            value
                        );

                    case VariableType.Parameter:
                        return _visitor.BuildSetMember(
                            ResolveArgumentsLocal(variable),
                            variable.Index.ToString(CultureInfo.InvariantCulture),
                            value
                        );

                    case VariableType.WithScope:
                        var resultParameter = Expression.Parameter(
                            variable.FallbackVariable.NativeType,
                            "result"
                        );

                        var result = BuildSet(variable.FallbackVariable, resultParameter);

                        var scope = variable.WithScope;

                        while (scope != null)
                        {
                            var withLocal = Expression.Parameter(typeof(JsObject), "with");

                            result = Expression.Block(
                                new[] { withLocal },
                                Expression.Assign(
                                    withLocal,
                                    Expression.Convert(
                                        BuildGet(scope.Variable, null),
                                        typeof(JsObject)
                                    )
                                ),
                                Expression.Condition(
                                    Expression.Call(
                                        withLocal,
                                        typeof(JsObject).GetMethod("HasProperty", new[] { typeof(string) }),
                                        Expression.Constant(variable.FallbackVariable.Name)
                                    ),
                                    _visitor.BuildSetMember(
                                        withLocal,
                                        variable.FallbackVariable.Name,
                                        resultParameter
                                    ),
                                    result,
                                    typeof(void)
                                )
                            );

                            scope = scope.Parent;
                        }

                        return Expression.Block(
                            new[] { resultParameter },
                            _visitor.BuildAssign(resultParameter, value),
                            result,
                            resultParameter
                        );

                    default:
                        return _visitor.BuildAssign(FindVariable(variable), value);
                }
            }

            private Expression ResolveArgumentsLocal(Variable variable)
            {
                Debug.Assert(variable.Type == VariableType.Parameter);

                if (variable.ClosureField == null)
                    return FindVariable(ArgumentsVariable);

                return Expression.Field(
                    BuildClosureAliases(variable.ClosureField.Closure),
                    variable.ClosureField.Field
                );
            }

            public Expression BuildGet(Variable variable, ParameterExpression withTarget)
            {
                switch (variable.Type)
                {
                    case VariableType.Global:
                        return _visitor.BuildGetMember(
                            Expression.Property(Runtime, "GlobalScope"),
                            variable.Name
                        );

                    case VariableType.Parameter:
                        return _visitor.BuildGetMember(
                            ResolveArgumentsLocal(variable),
                            variable.Index.ToString(CultureInfo.InvariantCulture)
                        );

                    case VariableType.WithScope:
                        var result = BuildGet(variable.FallbackVariable, null);

                        var scope = variable.WithScope;

                        while (scope != null)
                        {
                            var withLocal = Expression.Parameter(typeof(JsObject), "with");

                            Expression getter = _visitor.BuildGetMember(
                                withLocal,
                                variable.FallbackVariable.Name
                            );

                            if (withTarget != null)
                            {
                                getter = Expression.Block(
                                    Expression.Assign(withTarget, _visitor.EnsureJs(withLocal)),
                                    getter
                                );
                            }

                            var getterType = SyntaxUtil.GetValueType(getter.Type);
                            var resultType = SyntaxUtil.GetValueType(result.Type);
                            Type targetType;

                            if (getterType == resultType)
                            {
                                targetType = getterType.ToType();
                            }
                            else
                            {
                                targetType = typeof(object);
                                getter = _visitor.EnsureJs(getter);
                                result = _visitor.EnsureJs(result);
                            }

                            result = Expression.Block(
                                targetType,
                                new[] { withLocal },
                                Expression.Assign(
                                    withLocal,
                                    Expression.Convert(
                                        BuildGet(scope.Variable, null),
                                        typeof(JsObject)
                                    )
                                ),
                                Expression.Condition(
                                    Expression.Call(
                                        Expression.Convert(
                                            withLocal,
                                            typeof(JsObject)
                                        ),
                                        typeof(JsObject).GetMethod("HasProperty", new[] { typeof(string) }),
                                        Expression.Constant(variable.FallbackVariable.Name)
                                    ),
                                    getter,
                                    result
                                )
                            );

                            scope = scope.Parent;
                        }

                        return result;

                    default:
                        return FindVariable(variable);
                }
            }

            private ParameterExpression BuildClosureAliases(Closure targetClosure)
            {
                var closure = Closure;
                ParameterExpression parentParameter = null;

                while (true)
                {
                    ParameterExpression closureParameter;
                    if (!ClosureLocals.TryGetValue(closure, out closureParameter))
                    {
                        // Our closure is always added as a parameter, so we sould
                        // always be able to get it.

                        Debug.Assert(closure != Closure);

                        closureParameter = Expression.Parameter(
                            closure.Type,
                            "closure_" + ClosureLocals.Count.ToString(CultureInfo.InvariantCulture)
                        );

                        _statements.Add(Expression.Assign(
                            closureParameter,
                            Expression.Field(
                                parentParameter,
                                Closure.ParentFieldName
                            )
                        ));

                        ClosureLocals.Add(closure, closureParameter);
                    }

                    if (closure == targetClosure)
                        return closureParameter;

                    parentParameter = closureParameter;
                    closure = closure.Parent;
                }
            }
        }
    }
}
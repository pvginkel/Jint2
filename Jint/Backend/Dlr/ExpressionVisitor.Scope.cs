using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
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
                Runtime = Expression.Parameter(
                    typeof(JintRuntime),
                    RuntimeParameterName
                );

                Variables = new Dictionary<Variable, ParameterExpression>();
                Parent = parent;
                Return = Expression.Label(typeof(JsInstance), "return");
                ClosureLocals = new Dictionary<Closure, ParameterExpression>();
                BreakTargets = new Stack<LabelTarget>();
                ContinueTargets = new Stack<LabelTarget>();
            }

            private Expression FindVariable(Variable variable)
            {
                Debug.Assert(
                    variable.Type == Expressions.VariableType.Local ||
                    variable.Type == Expressions.VariableType.Arguments ||
                    variable.Type == Expressions.VariableType.This
                );

                switch (variable.Type)
                {
                    case Expressions.VariableType.This:
                        return This;

                    case Expressions.VariableType.Local:
                    case Expressions.VariableType.Arguments:
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
                    case Expressions.VariableType.Global:
                        return _visitor.BuildSetMember(
                            Expression.Property(Runtime, JintRuntime.GlobalScopeName),
                            variable.Name,
                            value
                        );

                    case Expressions.VariableType.Parameter:
                        return _visitor.BuildSetMember(
                            ResolveArgumentsLocal(variable),
                            variable.Index.ToString(CultureInfo.InvariantCulture),
                            value
                        );

                    case Expressions.VariableType.WithScope:
                        var resultParameter = Expression.Parameter(typeof(JsInstance), "result");

                        var result = BuildSet(variable.FallbackVariable, resultParameter);

                        var scope = variable.WithScope;

                        while (scope != null)
                        {
                            var withLocal = Expression.Parameter(typeof(JsDictionaryObject), "with");

                            result = Expression.Block(
                                new[] { withLocal },
                                Expression.Assign(
                                    withLocal,
                                    Expression.Convert(
                                        BuildGet(scope.Variable, null),
                                        typeof(JsDictionaryObject)
                                    )
                                ),
                                Expression.Condition(
                                    Expression.Call(
                                        withLocal,
                                        typeof(JsDictionaryObject).GetMethod("HasProperty", new[] { typeof(string) }),
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
                            Expression.Assign(resultParameter, value),
                            result,
                            resultParameter
                        );

                    default:
                        return Expression.Assign(
                            FindVariable(variable),
                            value
                        );
                }
            }

            private Expression ResolveArgumentsLocal(Variable variable)
            {
                Debug.Assert(variable.Type == Expressions.VariableType.Parameter);

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
                    case Expressions.VariableType.Global:
                        return _visitor.BuildGetMember(
                            Expression.Property(Runtime, JintRuntime.GlobalScopeName),
                            variable.Name
                        );

                    case Expressions.VariableType.Parameter:
                        return _visitor.BuildGetMember(
                            ResolveArgumentsLocal(variable),
                            variable.Index.ToString(CultureInfo.InvariantCulture)
                        );

                    case Expressions.VariableType.WithScope:
                        var result = BuildGet(variable.FallbackVariable, null);

                        var scope = variable.WithScope;

                        while (scope != null)
                        {
                            var withLocal = Expression.Parameter(typeof(JsDictionaryObject), "with");

                            Expression getter = _visitor.BuildGetMember(
                                withLocal,
                                variable.FallbackVariable.Name
                            );

                            if (withTarget != null)
                            {
                                getter = Expression.Block(
                                    Expression.Assign(withTarget, withLocal),
                                    getter
                                );
                            }

                            result = Expression.Block(
                                new[] { withLocal },
                                Expression.Assign(
                                    withLocal,
                                    Expression.Convert(
                                        BuildGet(scope.Variable, null),
                                        typeof(JsDictionaryObject)
                                    )
                                ),
                                Expression.Condition(
                                    Expression.Call(
                                        Expression.Convert(
                                            withLocal,
                                            typeof(JsDictionaryObject)
                                        ),
                                        typeof(JsDictionaryObject).GetMethod("HasProperty", new[] { typeof(string) }),
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

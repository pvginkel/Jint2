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
                        return Expression.Convert(
                            Expression.Dynamic(
                                _visitor._context.SetMember(variable.Name),
                                typeof(object),
                                Expression.Property(
                                    Runtime,
                                    JintRuntime.GlobalScopeName
                                ),
                                value
                            ),
                            typeof(JsInstance)
                        );

                    case VariableType.Parameter:
                        return Expression.Convert(
                            Expression.Dynamic(
                                _visitor._context.SetMember(variable.Index.ToString(CultureInfo.InvariantCulture)),
                                typeof(object),
                                ResolveArgumentsLocal(variable),
                                value
                            ),
                            typeof(JsInstance)
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
                Debug.Assert(variable.Type == VariableType.Parameter);

                if (variable.ClosureField == null)
                    return FindVariable(ArgumentsVariable);

                return Expression.Field(
                    BuildClosureAliases(variable.ClosureField.Closure),
                    variable.ClosureField.Field
                );
            }

            public Expression BuildGet(Variable variable)
            {
                switch (variable.Type)
                {
                    case VariableType.Global:
                        return Expression.Convert(
                            Expression.Dynamic(
                                _visitor._context.GetMember(variable.Name),
                                typeof(object),
                                Expression.Property(
                                    Runtime,
                                    JintRuntime.GlobalScopeName
                                )
                            ),
                            typeof(JsInstance)
                        );

                    case VariableType.Parameter:
                        return Expression.Convert(
                            Expression.Dynamic(
                                _visitor._context.GetMember(variable.Index.ToString(CultureInfo.InvariantCulture)),
                                typeof(object),
                                ResolveArgumentsLocal(variable)
                            ),
                            typeof(JsInstance)
                        );

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

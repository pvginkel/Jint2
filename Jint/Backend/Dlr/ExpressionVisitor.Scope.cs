using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            public Scope Parent { get; private set; }
            public Dictionary<Variable, ParameterExpression> Variables { get; private set; }
            public ParameterExpression Runtime { get; private set; }
            public ParameterExpression This { get; private set; }

            public Scope(ParameterExpression runtime, ParameterExpression @this, Scope parent)
            {
                if (runtime == null)
                    throw new ArgumentNullException("runtime");

                This = @this;
                Runtime = runtime;
                Variables = new Dictionary<Variable, ParameterExpression>();
                Parent = parent;
            }

            public Expression ResolveVariable(Variable variable)
            {
                switch (variable.Type)
                {
                    case VariableType.This:
                        return This;

                    //case VariableType.Global:
                    //    return Expression.Dynamic(
                    //        Binder.GetMember(variable.Name),
                    //        typeof(JsInstance),
                    //        Expression.Property(
                    //            Runtime,
                    //            JintRuntime.GlobalScopeName
                    //        )
                    //    );
                }

                var current = this;

                while (current != null)
                {
                    ParameterExpression result;
                    if (Variables.TryGetValue(variable, out result))
                        return result;

                    current = current.Parent;
                }

                throw new InvalidOperationException("Could not find parameter for variable");
            }
        }
    }
}

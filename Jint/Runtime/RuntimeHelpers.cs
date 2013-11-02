using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jint.Runtime
{
    internal static class RuntimeHelpers
    {
        public static DynamicMetaObject CreateThrow(DynamicMetaObject target, DynamicMetaObject[] arguments, BindingRestrictions moreTests, Type exception, params object[] exceptionArgs)
        {
            Expression[] argumentExpressions = null;
            var argTypes = Type.EmptyTypes;

            if (exceptionArgs != null)
            {
                int i = exceptionArgs.Length;
                argumentExpressions = new Expression[i];
                argTypes = new Type[i];
                i = 0;
                foreach (object obj in exceptionArgs)
                {
                    var e = Expression.Constant(obj);
                    argumentExpressions[i] = e;
                    argTypes[i] = e.Type;
                    i += 1;
                }
            }

            var constructor = exception.GetConstructor(argTypes);

            if (constructor == null)
                throw new ArgumentException("Type doesn't have constructor with a given signature");

            return new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(constructor, argumentExpressions),
                    typeof(object)
                ),
                target.Restrictions
                    .Merge(BindingRestrictions.Combine(arguments))
                    .Merge(moreTests)
            );
        }
    }
}

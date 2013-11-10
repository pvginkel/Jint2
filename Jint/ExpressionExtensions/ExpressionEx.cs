using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Jint.ExpressionExtensions
{
    internal static class ExpressionEx
    {
        public static UsingExpression Using(ParameterExpression variable, Expression initializer, Expression body)
        {
            return new UsingExpression(variable, initializer, body);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Expression initializer, Expression body)
        {
            return new ForEachExpression(target, initializer, body);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Expression initializer, Expression body, LabelTarget @break)
        {
            return new ForEachExpression(target, initializer, body, @break);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Expression initializer, Expression body, LabelTarget @break, LabelTarget @continue)
        {
            return new ForEachExpression(target, null, initializer, body, @break, @continue);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Type elementType, Expression initializer, Expression body)
        {
            return new ForEachExpression(target, elementType, initializer, body);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Type elementType, Expression initializer, Expression body, LabelTarget @break)
        {
            return new ForEachExpression(target, elementType, initializer, body, @break);
        }

        public static ForEachExpression ForEach(ParameterExpression target, Type elementType, Expression initializer, Expression body, LabelTarget @break, LabelTarget @continue)
        {
            return new ForEachExpression(target, elementType, initializer, body, @break, @continue);
        }
    }
}

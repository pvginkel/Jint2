using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;

namespace Jint.ExpressionExtensions
{
    internal class ConvertFromJsExpression : Expression
    {
        private static readonly MethodInfo _toBoolean = typeof(JsInstance).GetMethod("ToBoolean");
        private static readonly MethodInfo _toNumber = typeof(JsInstance).GetMethod("ToNumber");
        private static readonly MethodInfo _toString = typeof(object).GetMethod("ToString");
        private static readonly MethodInfo _numberToBoolean = typeof(JsNumber).GetMethod("NumberToBoolean");
        private static readonly MethodInfo _numberToString = typeof(JsNumber).GetMethod("NumberToString");
        private static readonly MethodInfo _stringToBoolean = typeof(JsString).GetMethod("StringToBoolean");
        private static readonly MethodInfo _stringToNumber = typeof(JsString).GetMethod("StringToNumber");
        private static readonly MethodInfo _booleanToNumber = typeof(JsBoolean).GetMethod("BooleanToNumber");
        private static readonly MethodInfo _booleanToString = typeof(JsBoolean).GetMethod("BooleanToString");

        private readonly Type _type;

        public override Type Type
        {
            get { return _type; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override bool CanReduce
        {
            get { return true; }
        }

        public Expression Expression { get; private set; }
        public Expressions.ValueType ValueType { get; private set; }

        public ConvertFromJsExpression(Expression expression, Expressions.ValueType valueType)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
            ValueType = valueType;

            switch (valueType)
            {
                case Expressions.ValueType.Boolean: _type = typeof(bool); break;
                case Expressions.ValueType.Double: _type = typeof(double); break;
                case Expressions.ValueType.String: _type = typeof(string); break;
                default: throw new ArgumentOutOfRangeException("valueType");
            }
        }

        public override Expression Reduce()
        {
            MethodInfo method;

            switch (ValueType)
            {
                case Expressions.ValueType.Boolean:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case Expressions.ValueType.Boolean: return Expression;
                        case Expressions.ValueType.Double: method = _numberToBoolean; break;
                        case Expressions.ValueType.String: method = _stringToBoolean; break;
                        default: method = _toBoolean; break;
                    }
                    break;

                case Expressions.ValueType.Double:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case Expressions.ValueType.Boolean: method = _booleanToNumber; break;
                        case Expressions.ValueType.Double: return Expression;
                        case Expressions.ValueType.String: method = _stringToNumber; break;
                        default: method = _toNumber; break;
                    }
                    break;

                case Expressions.ValueType.String:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case Expressions.ValueType.Boolean: method = _booleanToString; break;
                        case Expressions.ValueType.Double: method = _numberToString; break;
                        case Expressions.ValueType.String: return Expression;
                        default: method = _toString; break;
                    }
                    break;

                default: throw new InvalidOperationException();
            }

            if (method.IsStatic)
                return Call(method, Expression);

            return Call(Expression, method);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.ExpressionExtensions
{
    internal class ConvertFromJsExpression : Expression
    {
        private static readonly MethodInfo _toBoolean = typeof(JsInstance).GetMethod("ToBoolean");
        private static readonly MethodInfo _toNumber = typeof(JsInstance).GetMethod("ToNumber");
        private static readonly MethodInfo _toString = typeof(object).GetMethod("ToString");
        private static readonly MethodInfo _numberToBoolean = typeof(JsConvert).GetMethod("ToBoolean", new[] { typeof(double) });
        private static readonly MethodInfo _numberToString = typeof(JsConvert).GetMethod("ToString", new[] { typeof(double) });
        private static readonly MethodInfo _stringToBoolean = typeof(JsConvert).GetMethod("ToBoolean", new[] { typeof(string) });
        private static readonly MethodInfo _stringToNumber = typeof(JsConvert).GetMethod("ToNumber", new[] { typeof(string) });
        private static readonly MethodInfo _booleanToNumber = typeof(JsConvert).GetMethod("ToNumber", new[] { typeof(bool) });
        private static readonly MethodInfo _booleanToString = typeof(JsConvert).GetMethod("ToString", new[] { typeof(bool) });

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

        public ConvertFromJsExpression(Expression expression, ValueType valueType)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
            ValueType = valueType;

            switch (valueType)
            {
                case ValueType.Boolean: _type = typeof(bool); break;
                case ValueType.Double: _type = typeof(double); break;
                case ValueType.String: _type = typeof(string); break;
                case ValueType.Object: _type = typeof(JsObject); break;
                default: throw new ArgumentOutOfRangeException("valueType");
            }
        }

        public override Expression Reduce()
        {
            MethodInfo method;

            switch (ValueType)
            {
                case ValueType.Boolean:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case ValueType.Boolean: return Expression;
                        case ValueType.Double: method = _numberToBoolean; break;
                        case ValueType.String: method = _stringToBoolean; break;
                        default: method = _toBoolean; break;
                    }
                    break;

                case ValueType.Double:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case ValueType.Boolean: method = _booleanToNumber; break;
                        case ValueType.Double: return Expression;
                        case ValueType.String: method = _stringToNumber; break;
                        default: method = _toNumber; break;
                    }
                    break;

                case ValueType.String:
                    switch (SyntaxUtil.GetValueType(Expression.Type))
                    {
                        case ValueType.Boolean: method = _booleanToString; break;
                        case ValueType.Double: method = _numberToString; break;
                        case ValueType.String: return Expression;
                        default: method = _toString; break;
                    }
                    break;

                case ValueType.Object:
                    throw new InvalidOperationException();

                default: throw new InvalidOperationException();
            }

            if (method.IsStatic)
                return Call(method, Expression);

            return Call(Expression, method);
        }
    }
}

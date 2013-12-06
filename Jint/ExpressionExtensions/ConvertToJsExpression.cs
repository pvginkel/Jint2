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
    internal class ConvertToJsExpression : Expression
    {
        private static readonly MethodInfo _newBoolean = typeof(JsBoolean).GetMethod("Box", new[] { typeof(bool) });
        private static readonly MethodInfo _newNumber = typeof(JsNumber).GetMethod("Box", new[] { typeof(double) });
        private static readonly MethodInfo _newString = typeof(JsString).GetMethod("Box", new[] { typeof(string) });
        private static readonly MethodInfo _newObject = typeof(JsBox).GetMethod("CreateObject", new[] { typeof(JsObject) });

        public override Type Type
        {
            get { return typeof(JsBox); }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override bool CanReduce
        {
            get { return true; }
        }

        public ParameterExpression Runtime { get; private set; }
        public Expression Expression { get; private set; }

        public ConvertToJsExpression(ParameterExpression runtime, Expression expression)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            if (expression == null)
                throw new ArgumentNullException("expression");

            Runtime = runtime;
            Expression = expression;
        }

        public override Expression Reduce()
        {
            MethodInfo method;

            switch (SyntaxUtil.GetValueType(Expression.Type))
            {
                case ValueType.Boolean: method = _newBoolean; break;
                case ValueType.Double: method = _newNumber; break;
                case ValueType.String: method = _newString; break;
                case ValueType.Object: method = _newObject; break;
                default: throw new InvalidOperationException();
            }

            return Call(
                method,
                Expression
            );
        }
    }
}

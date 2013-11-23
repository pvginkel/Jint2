﻿using System;
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
    internal class ConvertToJsExpression : Expression
    {
        private static readonly MethodInfo _newBoolean = typeof(JintRuntime).GetMethod("New_Boolean");
        private static readonly MethodInfo _newNumber = typeof(JintRuntime).GetMethod("New_Number");
        private static readonly MethodInfo _newString = typeof(JintRuntime).GetMethod("New_String");

        public override Type Type
        {
            get { return typeof(JsInstance); }
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
                case Expressions.ValueType.Boolean: method = _newBoolean; break;
                case Expressions.ValueType.Double: method = _newNumber; break;
                case Expressions.ValueType.String: method = _newString; break;
                default: throw new InvalidOperationException();
            }

            return Call(
                Runtime,
                method,
                Expression
            );
        }
    }
}
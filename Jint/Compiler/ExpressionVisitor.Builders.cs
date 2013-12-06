using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Compiler
{
    partial class ExpressionVisitor
    {
        private readonly JsGlobal _global;
        private static readonly MethodInfo _objectGetByIndex = typeof(JsObject).GetMethod("GetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        private static readonly MethodInfo _objectSetByIndex = typeof(JsObject).GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int), typeof(JsBox) }, null);
        private static readonly MethodInfo _runtimeGetByIndex = typeof(JintRuntime).GetMethod("GetMemberByIndex");
        private static readonly MethodInfo _runtimeSetByIndex = typeof(JintRuntime).GetMethod("SetMemberByIndex");

        public ExpressionVisitor(JsGlobal global)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            _global = global;
        }

        public Expression BuildGet(SyntaxNode syntax)
        {
            return BuildGet(syntax, null);
        }

        public Expression BuildGet(SyntaxNode syntax, ParameterExpression withTarget)
        {
            switch (syntax.Type)
            {
                case SyntaxType.VariableDeclaration:
                    return _scope.BuildGet(((VariableDeclarationSyntax)syntax).Target, withTarget);

                case SyntaxType.Identifier:
                    return _scope.BuildGet(((IdentifierSyntax)syntax).Target, withTarget);

                case SyntaxType.MethodCall:
                    return ((MethodCallSyntax)syntax).Accept(this);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return BuildGetMember(
                        property.Expression.Accept(this),
                        property.Name
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return BuildGetIndex(
                        BuildGet(indexer.Expression, withTarget),
                        indexer.Index.Accept(this)
                    );

                default:
                    return syntax.Accept(this);
            }
        }

        public Expression BuildSet(SyntaxNode syntax, Expression value)
        {
            switch (syntax.Type)
            {
                case SyntaxType.VariableDeclaration:
                    return _scope.BuildSet(((VariableDeclarationSyntax)syntax).Target, value);

                case SyntaxType.Identifier:
                    return _scope.BuildSet(((IdentifierSyntax)syntax).Target, value);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return BuildSetMember(
                        property.Expression.Accept(this),
                        property.Name,
                        value
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return BuildSetIndex(
                        BuildGet(indexer.Expression),
                        indexer.Index.Accept(this),
                        value
                    );

                default:
                    throw new NotImplementedException();
            }
        }

        private Expression BuildGetMember(Expression expression, string name)
        {
            return BuildGetMember(expression, _global.ResolveIdentifier(name));
        }

        private Expression BuildGetMember(Expression expression, int index)
        {
            Expression indexExpression;

#if DEBUG
            indexExpression = Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("ResolveIdentifier"),
                Expression.Constant(_global.GetIdentifier(index))
            );
#else
            indexExpression = Expression.Constant(index);
#endif

            if (typeof(JsObject).IsAssignableFrom(expression.Type))
            {
                return Expression.Call(
                    expression,
                    _objectGetByIndex,
                    indexExpression
                );
            }

            return Expression.Call(
                _scope.Runtime,
                _runtimeGetByIndex,
                EnsureJs(expression),
                indexExpression
            );
        }

        private Expression BuildGetIndex(Expression expression, Expression index)
        {
            return BuildOperationCall(
                SyntaxExpressionType.Index,
                expression,
                index
            );
        }

        private Expression BuildSetIndex(Expression expression, Expression index, Expression value)
        {
            return BuildOperationCall(
                SyntaxExpressionType.SetIndex,
                expression,
                index,
                value
            );
        }

        private Expression BuildSetMember(Expression expression, string name, Expression value)
        {
            return BuildSetMember(expression, _global.ResolveIdentifier(name), value);
        }

        private Expression BuildSetMember(Expression expression, int index, Expression value)
        {
            Expression indexExpression;

#if DEBUG
            indexExpression = Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("ResolveIdentifier"),
                Expression.Constant(_global.GetIdentifier(index))
            );
#else
            indexExpression = Expression.Constant(index);
#endif

            if (typeof(JsObject).IsAssignableFrom(expression.Type))
            {
                return Expression.Call(
                    expression,
                    _objectSetByIndex,
                    indexExpression,
                    EnsureJs(value)
                );
            }

            return Expression.Call(
                _runtimeSetByIndex,
                EnsureJs(expression),
                indexExpression,
                EnsureJs(value)
            );
        }

        private Expression BuildDeleteMember(Expression expression, string name)
        {
            return BuildOperationCall(
                SyntaxExpressionType.Delete,
                expression,
                Expression.Constant(name)
            );
        }

        private Expression BuildDeleteIndex(Expression expression, Expression index)
        {
            return BuildOperationCall(
                SyntaxExpressionType.Delete,
                expression,
                index
            );
        }

        private Expression BuildOperationCall(SyntaxExpressionType operation, Expression obj, Expression index, Expression value)
        {
            var indexType = SyntaxUtil.GetValueType(index.Type);

            obj = EnsureJs(obj);
            value = EnsureJs(value);

            var method = FindOperationMethod(operation, ValueType.Unknown, indexType, ValueType.Unknown);
            var builder = FindOperationBuilder(operation, ValueType.Unknown, indexType, ValueType.Unknown);

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, ValueType.Unknown, ValueType.Unknown, ValueType.Unknown);
                builder = FindOperationBuilder(operation, ValueType.Unknown, ValueType.Unknown, ValueType.Unknown);

                index = EnsureJs(index);
            }

            if (builder != null)
                return builder(_scope.Runtime, new[] { obj, index, value });

            return Expression.Call(
                method.IsStatic ? null : _scope.Runtime,
                method,
                obj,
                index,
                value
            );
        }

        private Expression BuildOperationCall(SyntaxExpressionType operation, Expression left, Expression right)
        {
            var leftType = SyntaxUtil.GetValueType(left.Type);
            var rightType = SyntaxUtil.GetValueType(right.Type);

            var method = FindOperationMethod(operation, leftType, rightType);
            var builder = FindOperationBuilder(operation, leftType, rightType);

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, leftType, ValueType.Unknown);
                builder = FindOperationBuilder(operation, leftType, ValueType.Unknown);

                if (method != null || builder != null)
                    right = EnsureJs(right);
            }

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, ValueType.Unknown, rightType);
                builder = FindOperationBuilder(operation, ValueType.Unknown, rightType);

                if (method != null || builder != null)
                    left = EnsureJs(left);
            }

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, ValueType.Unknown, ValueType.Unknown);
                builder = FindOperationBuilder(operation, ValueType.Unknown, ValueType.Unknown);
                left = EnsureJs(left);
                right = EnsureJs(right);
            }

            if (builder != null)
                return builder(_scope.Runtime, new[] { left, right });

            return Expression.Call(
                method.IsStatic ? null : _scope.Runtime,
                method,
                left,
                right
            );
        }

        private Expression BuildOperationCall(SyntaxExpressionType operation, Expression operand)
        {
            var operandType = SyntaxUtil.GetValueType(operand.Type);

            var method = FindOperationMethod(operation, operandType);
            var builder = FindOperationBuilder(operation, operandType);

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, ValueType.Unknown);
                builder = FindOperationBuilder(operation, ValueType.Unknown);
                operand = EnsureJs(operand);
            }

            if (builder != null)
                return builder(_scope.Runtime, new[] { operand });

            return Expression.Call(
                method.IsStatic ? null : _scope.Runtime,
                method,
                operand
            );
        }

        private Expression BuildAssign(Expression target, Expression value)
        {
            var variableType = SyntaxUtil.GetValueType(target.Type);
            var valueType = SyntaxUtil.GetValueType(value.Type);

            if (variableType != valueType)
            {
                switch (variableType)
                {
                    case ValueType.Unknown: value = EnsureJs(value); break;
                    case ValueType.Boolean: value = EnsureBoolean(value); break;
                    case ValueType.String: value = EnsureString(value); break;
                    case ValueType.Double: value = EnsureNumber(value); break;
                    case ValueType.Object: value = EnsureObject(value); break;
                    default: throw new InvalidOperationException();
                }

                return Expression.Assign(target, value);
            }

            return Expression.Assign(
                target,
                value
            );
        }
    }
}

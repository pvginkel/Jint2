using System;
using System.Collections.Generic;
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
        private static readonly MethodInfo _toBoolean = typeof(JsInstance).GetMethod("ToBoolean");
        private static readonly MethodInfo _newBoolean = typeof(JintRuntime).GetMethod("New_Boolean");
        private static readonly MethodInfo _toNumber = typeof(JsInstance).GetMethod("ToNumber");
        private static readonly MethodInfo _newNumber = typeof(JintRuntime).GetMethod("New_Number");
        private static readonly MethodInfo _newString = typeof(JintRuntime).GetMethod("New_String");
        private static readonly PropertyInfo _itemByString = typeof(JsDictionaryObject).GetProperty("Item", new[] { typeof(string) });
        private static readonly PropertyInfo _itemByInstance = typeof(JsDictionaryObject).GetProperty("Item", new[] { typeof(JsInstance) });
        private static readonly MethodInfo _operationIndex = typeof(JintRuntime).GetMethod("Operation_Index");
        private static readonly MethodInfo _deleteByString = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(string) });
        private static readonly MethodInfo _deleteByInstance = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(JsInstance) });

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

        private Expression BuildToBoolean(Expression expression)
        {
            return Expression.Call(
                expression,
                _toBoolean
            );
        }

        private Expression BuildNewBoolean(Expression expression)
        {
            return Expression.Call(
                _scope.Runtime,
                _newBoolean,
                expression
            );
        }

        private Expression BuildToNumber(Expression expression)
        {
            return Expression.Call(
                expression,
                _toNumber
            );
        }

        private Expression BuildNewNumber(Expression expression)
        {
            return Expression.Call(
                _scope.Runtime,
                _newNumber,
                expression
            );
        }

        private Expression BuildNewString(Expression expression)
        {
            return Expression.Call(
                _scope.Runtime,
                _newString,
                expression
            );
        }

        private Expression BuildGetMember(Expression expression, string name)
        {
            return Expression.Property(
                Expression.Convert(expression, typeof(JsDictionaryObject)),
                _itemByString,
                Expression.Constant(name)
            );
        }

        private Expression BuildGetIndex(Expression expression, Expression index)
        {
            return Expression.Call(
                _scope.Runtime,
                _operationIndex,
                expression,
                index
            );
        }

        private Expression BuildSetIndex(Expression expression, Expression index, Expression value)
        {
            return Expression.Assign(
                Expression.Property(
                    Expression.Convert(expression, typeof(JsDictionaryObject)),
                    _itemByInstance,
                    index
                ),
                value
            );
        }

        private Expression BuildSetMember(Expression expression, string name, Expression value)
        {
            return Expression.Assign(
                Expression.Property(
                    Expression.Convert(expression, typeof(JsDictionaryObject)),
                    _itemByString,
                    Expression.Constant(name)
                ),
                value
            );
        }

        private Expression BuildDeleteMember(Expression expression, string name)
        {
            return BuildNewBoolean(
                Expression.Call(
                    Expression.Convert(expression, typeof(JsDictionaryObject)),
                    _deleteByString,
                    Expression.Constant(name)
                )
            );
        }

        private Expression BuildDeleteIndex(Expression expression, Expression index)
        {
            return BuildNewBoolean(
                Expression.Call(
                    Expression.Convert(expression, typeof(JsDictionaryObject)),
                    _deleteByInstance,
                    index
                )
            );
        }
    }
}

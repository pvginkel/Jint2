using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jint.ExpressionExtensions
{
    internal class ForEachExpression : Expression
    {
        private static readonly MethodInfo _moveNext = typeof(IEnumerator).GetMethod("MoveNext");

        public ParameterExpression Target { get; private set; }
        public Type ElementType { get; private set; }
        public Expression Initializer { get; private set; }
        public Expression Body { get; private set; }
        public LabelTarget BreakLabel { get; private set; }
        public LabelTarget ContinueLabel { get; private set; }

        public override Type Type
        {
            get { return Body.Type; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override bool CanReduce
        {
            get { return true; }
        }

        public ForEachExpression(ParameterExpression target, Expression initializer, Expression body)
            : this(target, initializer, body, null)
        {
        }

        public ForEachExpression(ParameterExpression target, Expression initializer, Expression body, LabelTarget @break)
            : this(target, initializer, body, @break, null)
        {
        }

        public ForEachExpression(ParameterExpression target, Expression initializer, Expression body, LabelTarget @break, LabelTarget @continue)
            : this(target, null, initializer, body, @break, @continue)
        {
        }

        public ForEachExpression(ParameterExpression target, Type elementType, Expression initializer, Expression body)
            : this(target, elementType, initializer, body, null)
        {
        }

        public ForEachExpression(ParameterExpression target, Type elementType, Expression initializer, Expression body, LabelTarget @break)
            : this(target, elementType, initializer, body, @break, null)
        {
        }

        public ForEachExpression(ParameterExpression target, Type elementType, Expression initializer, Expression body, LabelTarget @break, LabelTarget @continue)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (initializer == null)
                throw new ArgumentNullException("initializer");
            if (body == null)
                throw new ArgumentNullException("body");

            Target = target;
            ElementType = elementType ?? Target.Type;
            Initializer = initializer;
            Body = body;
            BreakLabel = @break ?? Label("break");
            ContinueLabel = @continue ?? Label("continue");
        }

        public override Expression Reduce()
        {
            var getEnumerator = Initializer.Type.GetMethod("GetEnumerator");
            if (getEnumerator == null)
                throw new InvalidOperationException("Initializer does not have a GetEnumerator method");
            if (!typeof(IEnumerator).IsAssignableFrom(getEnumerator.ReturnType))
                throw new InvalidOperationException("GetEnumerator does not return an IEnumerator");

            var enumeratorType = getEnumerator.ReturnType;

            var current = enumeratorType.GetProperty("Current");

            var enumerator = Parameter(enumeratorType, "enumerator");

            bool returnsVoid = Type == typeof(void);

            ParameterExpression result = null;
            if (!returnsVoid)
                result = Parameter(Type, "result");

            // Loop over all elements of the enumerator.
            var loop = Loop(
                Block(
                    typeof(void),
                    new[] { Target },
                    // Check whether we can move to the next item.
                    IfThen(
                        Not(Call(enumerator, _moveNext)),
                        // Break if there are no more items.
                        Goto(BreakLabel)
                    ),
                    // Assign the current element to the target variable
                    // by casting it to the element type.
                    Assign(
                        Target,
                        Convert(Property(enumerator, current), ElementType)
                    ),
                    // Execute the body and assign the result to the result
                    // variable.
                    returnsVoid ? Body : Assign(result, Body)
                ),
                BreakLabel,
                ContinueLabel
            );

            if (returnsVoid)
            {
                // IEnumerator implements IDisposable, so use a using block.
                return ExpressionEx.Using(
                    enumerator,
                    // Initialize by calling the GetEnumerator.
                    Call(Initializer, getEnumerator),
                    loop
                );
            }

            // IEnumerator implements IDisposable, so use a using block.
            return ExpressionEx.Using(
                enumerator,
                // Initialize by calling the GetEnumerator.
                Call(Initializer, getEnumerator),
                Block(
                    Type,
                    new[] { result },
                    // Initialize the result variable.
                    Assign(result, Default(Type)),
                    loop,
                    // Return the result of the body
                    result
                )
            );
        }
    }
}

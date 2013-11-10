using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jint.ExpressionExtensions
{
    internal class UsingExpression : Expression
    {
        private static readonly MethodInfo _dispose = typeof(IDisposable).GetMethod("Dispose");

        public ParameterExpression Target { get; private set; }
        public Expression Initializer { get; private set; }
        public Expression Body { get; private set; }

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

        public UsingExpression(ParameterExpression target, Expression initializer, Expression body)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (initializer == null)
                throw new ArgumentNullException("initializer");
            if (body == null)
                throw new ArgumentNullException("body");

            Target = target;
            Initializer = initializer;
            Body = body;
        }

        public override Expression Reduce()
        {
            bool returnsVoid = Type == typeof(void);

            // Result is used to store the result of _body so we can return it.
            ParameterExpression result = null;
            if (!returnsVoid)
                result = Parameter(Type, "result");

            // Disposable is used to cast the target to IDisposable so we can
            // dispose it.
            var disposable = Parameter(typeof(IDisposable), "disposable");

            if (returnsVoid)
            {
                return Block(
                    typeof(void),
                    new[] { Target },
                    // Execute the initializer to get the target.
                    Assign(Target, Initializer),
                    TryFinally(
                        // Execute the body.
                        Body,
                        // Dispose the target.
                        Block(
                            typeof(void),
                            new[] { disposable },
                            // Cast the target to an IDisposable.
                            Assign(disposable, TypeAs(Target, typeof(IDisposable))),
                            // Check whether we actually got something from the cast.
                            IfThen(
                                NotEqual(disposable, Constant(null)),
                                // Dispose the target.
                                Call(disposable, _dispose)
                            )
                        )
                    )
                );
            }

            return Block(
                Type,
                new[] { Target, result },
                // Execute the initializer to get the target.
                Assign(Target, Initializer),
                TryFinally(
                    // Execute the body and store the result.
                    Assign(result, Body),
                    // Dispose the target.
                    Block(
                        Type,
                        new[] { disposable },
                        // Cast the target to an IDisposable.
                        Assign(disposable, TypeAs(Target, typeof(IDisposable))),
                        // Check whether we actually got something from the cast.
                        IfThen(
                            NotEqual(disposable, Constant(null)),
                            // Dispose the target.
                            Call(disposable, _dispose)
                        ),
                        // Return the result from the body.
                        result
                    )
                )
            );
        }
    }
}

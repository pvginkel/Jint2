﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundCall : BoundExpression
    {
        public BoundExpression Target { get; private set; }
        public BoundExpression Method { get; private set; }
        public ReadOnlyArray<BoundCallArgument> Arguments { get; private set; }
        public ReadOnlyArray<BoundExpression> Generics { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Call; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Unknown; }
        }

        public BoundCall(BoundExpression target, BoundExpression method, ReadOnlyArray<BoundCallArgument> arguments, ReadOnlyArray<BoundExpression> generics)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (method == null)
                throw new ArgumentNullException("method");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (generics == null)
                throw new ArgumentNullException("generics");

            Target = target;
            Method = method;
            Arguments = arguments;
            Generics = generics;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitCall(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }

        public BoundCall Update(BoundExpression target, BoundExpression method, ReadOnlyArray<BoundCallArgument> arguments, ReadOnlyArray<BoundExpression> generics)
        {
            if (
                target == Target &&
                method == Method &&
                arguments == Arguments &&
                generics == Generics
            )
                return this;

            return new BoundCall(target, method, arguments, generics);
        }
    }
}

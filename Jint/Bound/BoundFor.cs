﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundFor : BoundStatement
    {
        public BoundBlock Initialization { get; private set; }
        public BoundExpression Test { get; private set; }
        public BoundBlock Increment { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.For; }
        }

        public BoundFor(BoundBlock initialization, BoundExpression test, BoundBlock increment, BoundBlock body, SourceLocation location)
            : base(location)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Initialization = initialization;
            Test = test;
            Increment = increment;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitFor(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitFor(this);
        }

        public BoundFor Update(BoundBlock initialization, BoundExpression test, BoundBlock increment, BoundBlock body, SourceLocation location)
        {
            if (
                initialization == Initialization &&
                test == Test &&
                increment == Increment &&
                body == Body &&
                location == Location
            )
                return this;

            return new BoundFor(initialization, test, increment, body, location);
        }
    }
}

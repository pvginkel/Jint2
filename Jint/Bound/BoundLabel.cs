﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundLabel : BoundStatement
    {
        public string Label { get; private set; }
        public BoundStatement Statement { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Label; }
        }

        public BoundLabel(string label, BoundStatement statement, SourceLocation location)
            : base(location)
        {
            if (label == null)
                throw new ArgumentNullException("label");
            if (statement == null)
                throw new ArgumentNullException("statement");

            Label = label;
            Statement = statement;
        }

        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitLabel(this);
        }

        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitLabel(this);
        }

        public BoundLabel Update(string label, BoundStatement statement, SourceLocation location)
        {
            if (
                label == Label &&
                statement == Statement &&
                location == Location
            )
                return this;

            return new BoundLabel(label, statement, location);
        }
    }
}

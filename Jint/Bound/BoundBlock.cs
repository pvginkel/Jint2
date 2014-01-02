using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundBlock : BoundStatement
    {
        public ReadOnlyArray<BoundTemporary> Temporaries { get; private set; }
        public ReadOnlyArray<BoundStatement> Nodes { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Block; }
        }

        public BoundBlock(ReadOnlyArray<BoundTemporary> temporaries, ReadOnlyArray<BoundStatement> nodes, SourceLocation location)
            : base(location)
        {
            if (temporaries == null)
                throw new ArgumentNullException("temporaries");
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            Temporaries = temporaries;
            Nodes = nodes;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitBlock(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }

        public BoundBlock Update(ReadOnlyArray<BoundTemporary> temporaries, ReadOnlyArray<BoundStatement> nodes, SourceLocation location)
        {
            if (
                temporaries == Temporaries &&
                nodes == Nodes &&
                location == Location
            )
                return this;

            return new BoundBlock(temporaries, nodes, location);
        }
    }
}

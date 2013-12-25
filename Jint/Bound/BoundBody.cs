using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundBody : BoundNode
    {
        public BoundBlock Body { get; private set; }
        public BoundClosure Closure { get; private set; }
        public BoundClosure ScopedClosure { get; private set; }
        public ReadOnlyArray<BoundArgument> Arguments { get; private set; }
        public ReadOnlyArray<BoundLocal> Locals { get; private set; }
        public BoundTypeManager TypeManager { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Body; }
        }

        public BoundBody(BoundBlock body, BoundClosure closure, BoundClosure scopedClosure, ReadOnlyArray<BoundArgument> arguments, ReadOnlyArray<BoundLocal> locals, BoundTypeManager typeManager)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (locals == null)
                throw new ArgumentNullException("locals");
            if (typeManager == null)
                throw new ArgumentNullException("typeManager");

            Body = body;
            Closure = closure;
            ScopedClosure = scopedClosure;
            Arguments = arguments;
            Locals = locals;
            TypeManager = typeManager;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitBody(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitBody(this);
        }

        public BoundBody Update(BoundBlock body, BoundClosure closure, BoundClosure scopedClosure, ReadOnlyArray<BoundArgument> arguments, ReadOnlyArray<BoundLocal> locals, BoundTypeManager typeManager)
        {
            if (
                body == Body &&
                closure == Closure &&
                scopedClosure == ScopedClosure &&
                arguments == Arguments &&
                locals == Locals &&
                typeManager == TypeManager
            )
                return this;

            return new BoundBody(body, closure, scopedClosure, arguments, locals, typeManager);
        }
    }
}

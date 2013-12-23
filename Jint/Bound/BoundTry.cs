using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundTry : BoundStatement
    {
        public BoundBlock Try { get; private set; }
        public BoundCatch Catch { get; private set; }
        public BoundFinally Finally { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Try; }
        }

        public BoundTry(BoundBlock @try, BoundCatch @catch, BoundFinally @finally)
        {
            if (@try == null)
                throw new ArgumentNullException("try");

            Debug.Assert(@catch != null || @finally != null);

            Try = @try;
            Catch = @catch;
            Finally = @finally;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitTry(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitTry(this);
        }

        public BoundTry Update(BoundBlock @try, BoundCatch @catch, BoundFinally @finally)
        {
            if (
                @try == Try &&
                @catch == Catch &&
                @finally == Finally
            )
                return this;

            return new BoundTry(@try, @catch, @finally);
        }
    }
}

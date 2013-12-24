using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class FunctionGatherer
    {
        public static ReadOnlyArray<BoundFunction> Gather(BoundBody body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            var functions = new ReadOnlyArray<BoundFunction>.Builder();

            new Gatherer(functions).Visit(body);

            return functions.ToReadOnlyArray();
        }

        private class Gatherer : BoundTreeWalker
        {
            private readonly ReadOnlyArray<BoundFunction>.Builder _functions;

            public Gatherer(ReadOnlyArray<BoundFunction>.Builder functions)
            {
                _functions = functions;
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                _functions.Add(node.Function);

                node.Function.Body.Accept(this);
            }
        }
    }
}

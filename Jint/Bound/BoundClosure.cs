using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Compiler;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundClosure
    {
        public BoundClosure Parent { get; private set; }
        public IKeyedCollection<string, BoundClosureField> Fields { get; private set; }

        public BoundClosure(BoundClosure parent, IEnumerable<IBoundType> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Parent = parent;

            var fieldCollection = new BoundClosureFieldCollection();

            foreach (var field in fields)
            {
                fieldCollection.Add(new BoundClosureField(this, field));
            }

            Fields = ReadOnlyKeyedCollection.Create(fieldCollection);
        }
    }
}

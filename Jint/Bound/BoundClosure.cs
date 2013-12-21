using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundClosure
    {
        public BoundClosure Parent { get; private set; }
        public IKeyedCollection<string, BoundClosureField> Fields { get; private set; }

        public BoundClosure(BoundClosure parent, IEnumerable<KeyValuePair<string, IBoundType>> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Parent = parent;

            var fieldCollection = new BoundClosureFieldCollection();

            foreach (var field in fields)
            {
                fieldCollection.Add(new BoundClosureField(field.Key, this, field.Value));
            }

            Fields = ReadOnlyKeyedCollection.Create(fieldCollection);
        }
    }
}

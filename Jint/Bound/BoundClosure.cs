using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Compiler;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundClosure
    {
        public BoundClosure Parent { get; private set; }
        public IKeyedCollection<string, BoundClosureField> Fields { get; private set; }
        public IClosureBuilder Builder { get; private set; }

        public BoundClosure(BoundClosure parent, IEnumerable<IBoundType> fields, IScriptBuilder scriptBuilder)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");
            if (scriptBuilder == null)
                throw new ArgumentNullException("scriptBuilder");

            Parent = parent;
            Builder = scriptBuilder.CreateClosureBuilder(this);

            var fieldCollection = new BoundClosureFieldCollection();

            foreach (var field in fields)
            {
                fieldCollection.Add(new BoundClosureField(this, field));
            }

            Fields = ReadOnlyKeyedCollection.Create(fieldCollection);
        }
    }
}

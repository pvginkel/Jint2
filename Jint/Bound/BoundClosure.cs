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
        private Dictionary<string, FieldInfo> _fields;

        public BoundClosure Parent { get; private set; }
        public IKeyedCollection<string, BoundClosureField> Fields { get; private set; }

        public Type Type { get; private set; }

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

        public void BuildType()
        {
            if (Type != null)
                throw new InvalidOperationException();

            Type = DynamicAssemblyManager.BuildClosure(
                Fields.ToDictionary(
                    p => p.Name,
                    p => p.ValueType.GetNativeType()
                )
            );

            _fields = Fields.ToDictionary(
                p => p.Name,
                p => Type.GetField(p.Name)
            );
        }

        public FieldInfo GetFieldInfo(string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException("fieldName");

            return _fields[fieldName];
        }
    }
}

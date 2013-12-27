using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    public class JsSchema
    {
        internal const int InitialArraySize = 20;

        private readonly SchemaHashSet _schema = new SchemaHashSet();
        private SchemaTransformationHashSet _transformations;
        private FreeEntry _freeList;
        private int _arraySize;

        public JsSchema()
        {
            GrowFreeEntries();
        }

        private JsSchema(JsSchema schema)
        {
            _schema = new SchemaHashSet(schema._schema);
            _freeList = schema._freeList;
            _arraySize = schema._arraySize;
        }

        private void GrowFreeEntries()
        {
            // This increases the array size and pushes free entries onto the
            // list.

            int offset = _arraySize;

            if (_arraySize == 0)
                _arraySize = InitialArraySize;
            else
                _arraySize *= 2;

            for (int i = _arraySize - 1; i >= offset; i--)
            {
                _freeList = new FreeEntry(i, _freeList);
            }
        }

        public JsSchema Add(int index, PropertyAttributes attributes, ref object[] values, object value)
        {
            // The entry shouldn't be in our schema yet.
            Debug.Assert(_schema.GetValue(index) < 0);

            // Get or create the new schema.
            JsSchema schema = null;

            // Check whether we already have a transformation.
            if (_transformations != null)
                schema = _transformations.GetValue(MakeAddIndex(index, attributes));

            int newOffset;

            // Build the new schema if we don't have it yet and add it to the
            // list of transformations.
            if (schema == null)
            {
                schema = new JsSchema(this);

                // Apply the mutation to the new schema. We get a free entry or,
                // if there isn't any, increase the array size.

                var freeList = schema._freeList;
                if (freeList == null)
                {
                    schema.GrowFreeEntries();
                    freeList = schema._freeList;
                }

                schema._freeList = freeList.Next;
                newOffset = freeList.Index;

                schema._schema.Add(index, attributes, newOffset);

                if (_transformations == null)
                    _transformations = new SchemaTransformationHashSet();

                _transformations.Add(MakeAddIndex(index, attributes), schema);
            }
            else
            {
                newOffset = schema._schema.GetValue(index);

                // The attributes of this property of the new schema should
                // be the same as what we're adding.
                Debug.Assert(schema._schema.GetAttributes(index) == attributes);
            }

            // Apply the transformation to the values array.
            if (_arraySize != schema._arraySize)
                Array.Resize(ref values, schema._arraySize);

            values[newOffset] = value;

            return schema;
        }

        private static int MakeAddIndex(int index, PropertyAttributes attributes)
        {
            return index * 8 + (int)attributes;
        }

        public JsSchema Remove(int index, ref object[] values)
        {
            int oldOffset = _schema.GetValue(index);

            // The entry should be in our schema.
            Debug.Assert(oldOffset >= 0);

            // Get or create the new schema.
            JsSchema schema = null;
            
            // Check whether we already have a transformation.
            if (_transformations != null)
                schema = _transformations.GetValue(index);

            // Build the new schema if we don't have it yet and add it to the
            // list of transformations.
            if (schema == null)
            {
                schema = new JsSchema(this);

                // Apply the transformation to the schema and add the index to
                // the free list.

                schema._schema.Remove(index);

                schema._freeList = new FreeEntry(oldOffset, schema._freeList);

                // Add the transformation.

                if (_transformations == null)
                    _transformations = new SchemaTransformationHashSet();

                _transformations.Add(index, schema);
            }

            // Apply the transformation to the values array.
            values[oldOffset] = null;

            return schema;
        }

        public int GetOffset(int index)
        {
            return _schema.GetValue(index);
        }

        public PropertyAttributes GetAttributes(int index)
        {
            return _schema.GetAttributes(index);
        }

        public bool TryGetAttributes(int index, out PropertyAttributes attributes)
        {
            return _schema.TryGetAttributes(index, out attributes);
        }

        public IEnumerable<int> GetKeys(bool enumerableOnly)
        {
            return _schema.GetKeys(enumerableOnly);
        }

        private class FreeEntry
        {
            public int Index { get; private set; }
            public FreeEntry Next { get; private set; }

            public FreeEntry(int index, FreeEntry next)
            {
                Index = index;
                Next = next;
            }
        }
    }
}

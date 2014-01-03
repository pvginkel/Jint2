using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    public sealed partial class JsSchema
    {
#if SCHEMA_STATISTICS
        private static int _createdSchemas;
        private static int _createdNodes;
        private static int _flattenedSchemas;
        private static int _numberLookups;
        private static int _numberLookupIterations;

        public static void PrintStatistics()
        {
            Console.WriteLine(
                "Created schemas={0}, created nodes={1}, flattened schemas={2}, number lookups={3}, number lookup iterations={4}",
                _createdSchemas,
                _createdNodes,
                _flattenedSchemas,
                _numberLookups,
                _numberLookupIterations
            );

            _createdSchemas = 0;
            _createdNodes = 0;
            _flattenedSchemas = 0;
            _numberLookups = 0;
            _numberLookupIterations = 0;
        }
#endif

        private const int InitialTransformationsSize = 3;
        private const int MaxLookupCount = 100;
        internal const int InitialArraySize = 20;

        private SchemaTransformationHashSet _transformations;
        private FreeEntry _freeList;
        private int _arraySize;
        private Node _node;
        private int _lookupCount;
        private SchemaHashSet _cachedSchema;

        public JsSchema()
        {
            GrowFreeEntries();
        }

        private JsSchema(JsSchema schema)
        {
#if SCHEMA_STATISTICS
            _createdSchemas++;
#endif

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
            Debug.Assert(GetOffset(index) < 0);

            // Get or create the new schema.
            JsSchema schema = null;

            // Check whether we already have a transformation.
            if (_transformations != null)
                schema = _transformations.GetValue(MakeIndex(index, attributes));

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

                schema._node = new Node(true, index, attributes, newOffset, _node);

                if (_transformations == null)
                    _transformations = new SchemaTransformationHashSet(InitialTransformationsSize);

                _transformations.Add(MakeIndex(index, attributes), schema);
            }
            else
            {
                newOffset = schema.GetOffset(index);

                // The attributes of this property of the new schema should
                // be the same as what we're adding.
                Debug.Assert(schema.GetAttributes(index) == attributes);
            }

            // Apply the transformation to the values array.
            if (_arraySize != schema._arraySize)
                Array.Resize(ref values, schema._arraySize);

            values[newOffset] = value;

            return schema;
        }

        private static int MakeIndex(int index, PropertyAttributes attributes)
        {
            return index * 8 + (int)attributes;
        }

        public JsSchema Remove(int index, ref object[] values)
        {
            int oldOffset = GetOffset(index);

            // The entry should be in our schema.
            Debug.Assert(oldOffset >= 0);

            // Get or create the new schema.
            JsSchema schema = null;
            
            // Check whether we already have a transformation.
            if (_transformations != null)
                schema = _transformations.GetValue(MakeIndex(index, 0));

            // Build the new schema if we don't have it yet and add it to the
            // list of transformations.
            if (schema == null)
            {
                schema = new JsSchema(this);

                // Apply the transformation to the schema and add the index to
                // the free list.

                schema._node = new Node(false, index, 0, -1, _node);

                schema._freeList = new FreeEntry(oldOffset, schema._freeList);

                // Add the transformation.

                if (_transformations == null)
                    _transformations = new SchemaTransformationHashSet();

                _transformations.Add(MakeIndex(index, 0), schema);
            }

            // Apply the transformation to the values array.
            values[oldOffset] = null;

            return schema;
        }

        private Node FindNode(int index)
        {
#if SCHEMA_STATISTICS
            _numberLookups++;
#endif

            var node = _node;
            while (node != null)
            {
                CheckFlattenSchema();

#if SCHEMA_STATISTICS
                _numberLookupIterations++;
#endif

                if (node.Index == index)
                {
                    if (node.Added)
                        return node;
                    return null;
                }

                node = node.Parent;
            }

            return null;
        }

        private void CheckFlattenSchema()
        {
            if (++_lookupCount == MaxLookupCount)
                QueueFlattenSchema(this);
        }

        private void FlattenSchema()
        {
#if SCHEMA_STATISTICS
            _flattenedSchemas++;
#endif

            Debug.Assert(_cachedSchema == null);

            var cachedSchema = new SchemaHashSet();

            foreach (var node in GetNodes())
            {
                cachedSchema.Add(node.Index, node.Attributes, node.Offset);
            }

            _cachedSchema = cachedSchema;
        }

        public int GetOffset(int index)
        {
            if (_cachedSchema != null)
                return _cachedSchema.GetValue(index);

            var node = FindNode(index);
            return node != null ? node.Offset : -1;
        }

        public PropertyAttributes GetAttributes(int index)
        {
            if (_cachedSchema != null)
                return _cachedSchema.GetAttributes(index);

            var node = FindNode(index);
            return node != null ? node.Attributes : 0;
        }

        public bool TryGetAttributes(int index, out PropertyAttributes attributes)
        {
            if (_cachedSchema != null)
                return _cachedSchema.TryGetAttributes(index, out attributes);

            var node = FindNode(index);
            if (node != null)
            {
                attributes = node.Attributes;
                return true;
            }

            attributes = 0;
            return false;
        }

        public IEnumerable<int> GetKeys(bool enumerableOnly)
        {
            if (_cachedSchema != null)
                return _cachedSchema.GetKeys(enumerableOnly);

            return GetKeysFromNode(enumerableOnly);
        }

        private IEnumerable<int> GetKeysFromNode(bool enumerableOnly)
        {
            foreach (var node in GetNodes())
            {
                if (
                    !enumerableOnly ||
                    (node.Attributes & PropertyAttributes.DontEnum) == 0
                )
                    yield return node.Index;
            }
        }

        private IEnumerable<Node> GetNodes()
        {
#if SCHEMA_STATISTICS
            _numberLookups++;
#endif

            HashSet<int> removed = null;
            var seen = new HashSet<int>();

            var node = _node;
            while (node != null)
            {
                CheckFlattenSchema();

#if SCHEMA_STATISTICS
                _numberLookupIterations++;
#endif

                if (!node.Added)
                {
                    if (removed == null)
                        removed = new HashSet<int>();

                    removed.Add(node.Index);
                }
                else if (
                    (removed == null || !removed.Contains(node.Index)) &&
                    !seen.Contains(node.Index)
                )
                {
                    seen.Add(node.Index);
                    yield return node;
                }

                node = node.Parent;
            }
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

        private class Node
        {
            public bool Added { get; private set; }
            public int Index { get; private set; }
            public PropertyAttributes Attributes { get; private set; }
            public int Offset { get; private set; }
            public Node Parent { get; private set; }

            public Node(bool added, int index, PropertyAttributes attributes, int offset, Node parent)
            {
#if SCHEMA_STATISTICS
                _createdNodes++;
#endif

                Added = added;
                Index = index;
                Attributes = attributes;
                Offset = offset;
                Parent = parent;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    public sealed class JsSchema
    {
        private const int InitialTransformationsSize = 3;
        internal const int InitialArraySize = 20;

        private SchemaTransformationHashSet _transformations;
        private FreeEntry _freeList;
        private int _arraySize;
        private INode _node;

        public JsSchema()
        {
            GrowFreeEntries();
            _node = TailNode.Instance;
        }

        private JsSchema(JsSchema schema)
        {
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

                schema._node = _node.Create(true, index, attributes, newOffset);

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

                schema._node = _node.Create(false, index, 0, -1);

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

        public int GetOffset(int index)
        {
            return _node.GetOffset(index);
        }

        public PropertyAttributes GetAttributes(int index)
        {
            PropertyAttributes attributes;
            _node.TryGetAttributes(index, out attributes);
            return attributes;
        }

        public bool TryGetAttributes(int index, out PropertyAttributes attributes)
        {
            return _node.TryGetAttributes(index, out attributes);
        }

        public IEnumerable<int> GetKeys(bool enumerableOnly)
        {
            HashSet<int> removed = null;
            HashSet<int> keys = new HashSet<int>();

            _node.GetKeys(enumerableOnly, ref removed, keys);

            return keys;
            //HashSet<int> removed = null;

            //var node = _node;
            //while (node != null)
            //{
            //    if (!node.Added)
            //    {
            //        if (removed == null)
            //            removed = new HashSet<int>();

            //        removed.Add(node.Index);
            //    }
            //    else if (
            //        (removed == null || !removed.Contains(node.Index)) &&
            //        !(enumerableOnly && (node.Attributes & PropertyAttributes.DontEnum) != 0)
            //    )
            //        yield return node.Index;

            //    node = node.Parent;
            //}
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

        private interface INode
        {
            int Depth { get; }

            INode Create(bool added, int index, PropertyAttributes attributes, int offset);
            int GetOffset(int index);
            bool TryGetAttributes(int index, out PropertyAttributes attributes);
            void GetKeys(bool enumerableOnly, ref HashSet<int> removed, HashSet<int> keys);
        }

        private sealed class TailNode : INode
        {
            public static readonly TailNode Instance = new TailNode();

            public int Depth
            {
                get { return 0; }
            }

            private TailNode()
            {
            }

            public INode Create(bool added, int index, PropertyAttributes attributes, int offset)
            {
                return new Node(added, index, attributes, offset, this);
            }

            public bool TryGetAttributes(int index, out PropertyAttributes attributes)
            {
                attributes = 0;
                return false;
            }

            public int GetOffset(int index)
            {
                return -1;
            }

            public void GetKeys(bool enumerableOnly, ref HashSet<int> removed, HashSet<int> keys)
            {
            }
        }

        private sealed class Node : INode
        {
            private const int MaxDepth = 20;

            public bool Added { get; private set; }
            public int Index { get; private set; }
            public PropertyAttributes Attributes { get; private set; }
            public int Offset { get; private set; }
            public INode Parent { get; private set; }
            private SnapshotNode _snapshot;

            public int Depth { get; private set; }

            public Node(bool added, int index, PropertyAttributes attributes, int offset, INode parent)
            {
                Added = added;
                Index = index;
                Attributes = attributes;
                Offset = offset;
                Parent = parent;
                Depth = parent == null ? 1 : parent.Depth + 1;
            }

            public INode Create(bool added, int index, PropertyAttributes attributes, int offset)
            {
                if (_snapshot == null && Depth == MaxDepth)
                    _snapshot = new SnapshotNode(this);

                return new Node(added, index, attributes, offset, (INode)_snapshot ?? this);
            }

            public int GetOffset(int index)
            {
                if (_snapshot != null)
                    return _snapshot.GetOffset(index);

                if (index == Index)
                {
                    if (!Added)
                        return -1;

                    return Offset;
                }

                return Parent.GetOffset(index);
            }

            public bool TryGetAttributes(int index, out PropertyAttributes attributes)
            {
                if (_snapshot != null)
                    return _snapshot.TryGetAttributes(index, out attributes);

                if (index == Index)
                {
                    if (!Added)
                    {
                        attributes = 0;
                        return false;
                    }

                    attributes = Attributes;
                    return true;
                }

                return Parent.TryGetAttributes(index, out attributes);
            }

            public void GetKeys(bool enumerableOnly, ref HashSet<int> removed, HashSet<int> keys)
            {
                if (_snapshot != null)
                {
                    _snapshot.GetKeys(enumerableOnly, ref removed, keys);
                    return;
                }

                if (!Added)
                {
                    if (removed == null)
                        removed = new HashSet<int>();

                    removed.Add(Index);
                }
                else if (
                    (removed == null || !removed.Contains(Index)) &&
                    !(enumerableOnly && (Attributes & PropertyAttributes.DontEnum) != 0)
                )
                {
                    keys.Add(Index);
                }

                Parent.GetKeys(enumerableOnly, ref removed, keys);
            }
        }

        private sealed class SnapshotNode : INode
        {
            private SchemaHashSet _schema;

            public SnapshotNode(INode node)
            {
                BuildSnapshot(node, node.Depth);
            }

            private void BuildSnapshot(INode node, int depth)
            {
                var normalNode = node as Node;
                if (normalNode != null)
                {
                    BuildSnapshot(normalNode.Parent, depth);

                    if (_schema == null)
                        _schema = new SchemaHashSet(depth * 10 / 7);

                    if (normalNode.Added)
                        _schema.Add(normalNode.Index, normalNode.Attributes, normalNode.Offset);
                    else
                        _schema.Remove(normalNode.Index);
                }
                else
                {
                    var snapshot = node as SnapshotNode;
                    if (snapshot != null)
                        _schema = new SchemaHashSet(snapshot._schema);
                }
            }

            public int Depth
            {
                get { return 0; }
            }

            public INode Create(bool added, int index, PropertyAttributes attributes, int offset)
            {
                return new Node(added, index, attributes, offset, this);
            }

            public int GetOffset(int index)
            {
                return _schema.GetValue(index);
            }

            public bool TryGetAttributes(int index, out PropertyAttributes attributes)
            {
                return _schema.TryGetAttributes(index, out attributes);
            }

            public void GetKeys(bool enumerableOnly, ref HashSet<int> removed, HashSet<int> keys)
            {
                foreach (int key in _schema.GetKeys(enumerableOnly))
                {
                    if (!removed.Contains(key))
                        keys.Add(key);
                }
            }
        }
    }
}

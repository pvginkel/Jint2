﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Support
{
    [DebuggerTypeProxy(typeof(SchemaHashSetDebugView))]
    internal class SchemaHashSet
    {
        private Entry[] _entries;

        public int Count { get; private set; }

        private int MaxLoadFactor
        {
            get { return _entries.Length * 7 / 10; }
        }

        public SchemaHashSet()
        {
            _entries = new Entry[PrimesHelper.GetPrime(20)];

            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].Invalidate();
            }
        }

        public SchemaHashSet(SchemaHashSet other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            
            _entries = new Entry[other._entries.Length];
            Array.Copy(other._entries, _entries, _entries.Length);
            Count = other.Count;
        }

        private int FindEntry(int index)
        {
            int offset = Hash(index);

            // If the first entry isn't valid, we don't have it in the list.

            if (!_entries[offset].IsValid)
                return -1;

            // We don't check IsValid in the loop, because the entries are
            // maintained such that the chain is always valid.

            while (true)
            {
                // If the index is equal, we've found the correct entry.

                if (_entries[offset].Index == index)
                    return offset;

                // See whether this entry is changed to another entry.

                offset = _entries[offset].Next;
                if (offset < 0)
                    return -1;

                // If the next entry is valid, move the offset to that entry.
            }
        }

        private int Hash(int index)
        {
            return (index & 0x7FFFFFFF) % _entries.Length;
        }

        public void Add(int index, PropertyAttributes attributes, int value)
        {
            int entryIndex = FindEntry(index);

            if (entryIndex >= 0)
                throw new ArgumentOutOfRangeException("index");

            // Grow the entries when we have to.

            if (Count >= MaxLoadFactor)
                GrowEntries();

            // If the entry at the ideal location doesn't have the correct has,
            // we're going to move that entry.

            int hash = Hash(index);

            if (_entries[hash].IsValid && Hash(_entries[hash].Index) != hash)
            {
                // Create a copy of the current entry and remove it.

                var copy = _entries[hash];

                Remove(copy.Index);

                // Put the new entry at the ideal location.

                _entries[hash] = new Entry(index, attributes, value, -1);

                // Increment the count.

                Count++;

                // And now add the previous entry.

                Add(copy.Index, copy.Attributes, copy.Value);
            }
            else
            {
                // Find the end of the chain currently at the entry.

                entryIndex = Hash(index);
                int free;

                if (_entries[entryIndex].IsValid)
                {
                    for (
                        int next = _entries[entryIndex].Next;
                        next != -1;
                        entryIndex = next, next = _entries[entryIndex].Next
                    )
                    {
                        // Find the end of the chain.
                    }

                    // Find a free entry.

                    free = entryIndex + 1;
                    int length = _entries.Length;

                    while (true)
                    {
                        if (free == length)
                            free = 0;

                        if (!_entries[free].IsValid)
                            break;

                        free++;
                    }
                }
                else
                {
                    free = entryIndex;
                    entryIndex = -1;
                }

                // Put the new entry into the free location.

                _entries[free] = new Entry(index, attributes, value, -1);

                // Fixup the chain if we have one.

                if (entryIndex >= 0)
                    _entries[entryIndex].Next = free;

                // Increment the count.

                Count++;
            }
        }

        private void GrowEntries()
        {
            var entries = _entries;

            _entries = new Entry[PrimesHelper.GetPrime(_entries.Length * 2)];

            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].Invalidate();
            }
            
            Count = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.IsValid)
                    Add(entry.Index, entry.Attributes, entry.Value);
            }
        }
        
        public int GetValue(int index)
        {
            int entryIndex = FindEntry(index);
            if (entryIndex >= 0)
                return _entries[entryIndex].Value;

            return -1;
        }

        public bool TryGetAttributes(int index, out PropertyAttributes attributes)
        {
            int entryIndex = FindEntry(index);
            if (entryIndex >= 0)
            {
                attributes = _entries[entryIndex].Attributes;
                return true;
            }

            attributes = 0;
            return false;
        }

        public PropertyAttributes GetAttributes(int index)
        {
            int entryIndex = FindEntry(index);
            if (entryIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            return _entries[entryIndex].Attributes;
        }

/*
        public void SetValue(int index, object value)
        {
            Debug.Assert(value == null || value.GetType() != typeof(object));

            int entryIndex = FindEntry(index);
            if (entryIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            _entries[entryIndex].Value = value;
        }
*/

        public bool Remove(int index)
        {
            int entryIndex = FindEntry(index);

            // Check whether we actually have this index.

            if (entryIndex < 0)
                return false;

            // When we remove an item, we need to re-index the complete chain.
            // The reason for this is that there may be elements in the
            // chain of which the hash of the index does not correspond
            // to the chain, because there wasn't any room to place the item.

            // First count the number of items in the chain.

            var last = -1;

            for (
                entryIndex = Hash(index);
                entryIndex >= 0;
                last = entryIndex, entryIndex = _entries[entryIndex].Next
            )
            {
                if (_entries[entryIndex].Index != index)
                    continue;

                // If this is not the tail of the chain, we need to fixup.

                var next = _entries[entryIndex].Next;

                if (last >= 0)
                {
                    // If this is not the head of the chain, the previous
                    // entry must point to the next entry and this entry
                    // becomes invalidated.

                    _entries[last].Next = next;

                    _entries[entryIndex].Invalidate();
                }
                else if (next >= 0)
                {
                    // Otherwise, we replace the head of the chain with the
                    // next entry and invalidate the next entry.

                    _entries[entryIndex] = _entries[next];

                    _entries[next].Invalidate();
                }
                else
                {
                    // If we're the head and there is no next entry, just
                    // invalidate this one.

                    _entries[entryIndex].Invalidate();
                }

                break;
            }

            // Decrement the count.

            Count--;

            return true;
        }

        public IEnumerable<int> GetKeys(bool enumerableOnly)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (
                    _entries[i].IsValid &&
                    (
                        !enumerableOnly ||
                        (_entries[i].Attributes & PropertyAttributes.DontEnum) == 0
                    )
                )
                    yield return _entries[i].Index;
            }
        }

        private struct Entry
        {
            private readonly int _index;
            private int _next;
            private int _value;

            public int Index
            {
                get { return _index; }
            }

            public PropertyAttributes Attributes
            {
                get { return (PropertyAttributes)(_value & 7); }
            }

            public int Value
            {
                get { return _value >> 3; }
            }

            public bool IsValid
            {
                get { return _value >= 0; }
            }

            public int Next
            {
                get { return _next; }
                set { _next = value; }
            }

            public Entry(int index, PropertyAttributes attributes, int value, int next)
            {
                Debug.Assert(value != null);

                _index = index;
                _value = value * 8 | (int)attributes;
                _next = next;
            }

            public void Invalidate()
            {
                _value = -1;
            }

            public override string ToString()
            {
                if (!IsValid)
                    return "IsValid=False";

                return String.Format(
                    "Index={0}, Value={1}, Attributes={2}, IsValid={3}, Next={4}",
                    Index,
                    Value,
                    Attributes,
                    IsValid,
                    Next
                );
            }
        }

        internal class SchemaHashSetDebugView
        {
            private readonly SchemaHashSet _container;

            public SchemaHashSetDebugView(SchemaHashSet container)
            {
                _container = container;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DisplayEntry[] Items
            {
                get
                {
                    var items = new List<DisplayEntry>();

                    foreach (int key in _container.GetKeys(false))
                    {
                        items.Add(new DisplayEntry(_container._entries[_container.FindEntry(key)]));
                    }

                    items.Sort((a, b) => Math.Abs(a.Index).CompareTo(Math.Abs(b.Index)));

                    return items.ToArray();
                }
            }

            private class DisplayEntry
            {
                public int Index { get; private set; }
                public int Value { get; private set; }
                public PropertyAttributes Attributes { get; private set; }

                public DisplayEntry(Entry entry)
                {
                    Index = entry.Index;
                    Value = entry.Value;
                    Attributes = entry.Attributes;
                }

                public override string ToString()
                {
                    return String.Format("Index={0}, Value={1}, Attributes={2}", Index, Value, Attributes);
                }
            }
        }
    }
}
namespace Jint.Support
{
    [DebuggerTypeProxy(typeof(SchemaTransformationHashSetDebugView))]
    internal class SchemaTransformationHashSet
    {
        private Entry[] _entries;

        public int Count { get; private set; }

        private int MaxLoadFactor
        {
            get { return _entries.Length * 7 / 10; }
        }

        public SchemaTransformationHashSet()
        {
            _entries = new Entry[PrimesHelper.GetPrime(20)];
        }

        public SchemaTransformationHashSet(SchemaTransformationHashSet other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            
            _entries = new Entry[other._entries.Length];
            Array.Copy(other._entries, _entries, _entries.Length);
            Count = other.Count;
        }

        private int FindEntry(int index)
        {
            int offset = Hash(index);

            // If the first entry isn't valid, we don't have it in the list.

            if (!_entries[offset].IsValid)
                return -1;

            // We don't check IsValid in the loop, because the entries are
            // maintained such that the chain is always valid.

            while (true)
            {
                // If the index is equal, we've found the correct entry.

                if (_entries[offset].Index == index)
                    return offset;

                // See whether this entry is changed to another entry.

                offset = _entries[offset].Next;
                if (offset < 0)
                    return -1;

                // If the next entry is valid, move the offset to that entry.
            }
        }

        private int Hash(int index)
        {
            return (index & 0x7FFFFFFF) % _entries.Length;
        }

        public void Add(int index, JsSchema value)
        {
            int entryIndex = FindEntry(index);

            if (entryIndex >= 0)
                throw new ArgumentOutOfRangeException("index");

            // Grow the entries when we have to.

            if (Count >= MaxLoadFactor)
                GrowEntries();

            // If the entry at the ideal location doesn't have the correct has,
            // we're going to move that entry.

            int hash = Hash(index);

            if (_entries[hash].IsValid && Hash(_entries[hash].Index) != hash)
            {
                // Create a copy of the current entry and remove it.

                var copy = _entries[hash];

                Remove(copy.Index);

                // Put the new entry at the ideal location.

                _entries[hash] = new Entry(index, value, -1);

                // Increment the count.

                Count++;

                // And now add the previous entry.

                Add(copy.Index, copy.Value);
            }
            else
            {
                // Find the end of the chain currently at the entry.

                entryIndex = Hash(index);
                int free;

                if (_entries[entryIndex].IsValid)
                {
                    for (
                        int next = _entries[entryIndex].Next;
                        next != -1;
                        entryIndex = next, next = _entries[entryIndex].Next
                    )
                    {
                        // Find the end of the chain.
                    }

                    // Find a free entry.

                    free = entryIndex + 1;
                    int length = _entries.Length;

                    while (true)
                    {
                        if (free == length)
                            free = 0;

                        if (!_entries[free].IsValid)
                            break;

                        free++;
                    }
                }
                else
                {
                    free = entryIndex;
                    entryIndex = -1;
                }

                // Put the new entry into the free location.

                _entries[free] = new Entry(index, value, -1);

                // Fixup the chain if we have one.

                if (entryIndex >= 0)
                    _entries[entryIndex].Next = free;

                // Increment the count.

                Count++;
            }
        }

        private void GrowEntries()
        {
            var entries = _entries;

            _entries = new Entry[PrimesHelper.GetPrime(_entries.Length * 2)];
            
            Count = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.IsValid)
                    Add(entry.Index, entry.Value);
            }
        }
        
        public JsSchema GetValue(int index)
        {
            int entryIndex = FindEntry(index);
            if (entryIndex >= 0)
                return _entries[entryIndex].Value;

            return null;
        }

/*
        public void SetValue(int index, object value)
        {
            Debug.Assert(value == null || value.GetType() != typeof(object));

            int entryIndex = FindEntry(index);
            if (entryIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            _entries[entryIndex].Value = value;
        }
*/

        public bool Remove(int index)
        {
            int entryIndex = FindEntry(index);

            // Check whether we actually have this index.

            if (entryIndex < 0)
                return false;

            // When we remove an item, we need to re-index the complete chain.
            // The reason for this is that there may be elements in the
            // chain of which the hash of the index does not correspond
            // to the chain, because there wasn't any room to place the item.

            // First count the number of items in the chain.

            var last = -1;

            for (
                entryIndex = Hash(index);
                entryIndex >= 0;
                last = entryIndex, entryIndex = _entries[entryIndex].Next
            )
            {
                if (_entries[entryIndex].Index != index)
                    continue;

                // If this is not the tail of the chain, we need to fixup.

                var next = _entries[entryIndex].Next;

                if (last >= 0)
                {
                    // If this is not the head of the chain, the previous
                    // entry must point to the next entry and this entry
                    // becomes invalidated.

                    _entries[last].Next = next;

                    _entries[entryIndex].Invalidate();
                }
                else if (next >= 0)
                {
                    // Otherwise, we replace the head of the chain with the
                    // next entry and invalidate the next entry.

                    _entries[entryIndex] = _entries[next];

                    _entries[next].Invalidate();
                }
                else
                {
                    // If we're the head and there is no next entry, just
                    // invalidate this one.

                    _entries[entryIndex].Invalidate();
                }

                break;
            }

            // Decrement the count.

            Count--;

            return true;
        }

        public IEnumerable<int> GetKeys(bool enumerableOnly)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].IsValid)
                    yield return _entries[i].Index;
            }
        }

        private struct Entry
        {
            private readonly int _index;
            private int _next;
            private JsSchema _value;

            public int Index
            {
                get { return _index; }
            }

            public JsSchema Value
            {
                get { return _value; }
            }

            public bool IsValid
            {
                get { return _value != null; }
            }

            public int Next
            {
                get { return _next; }
                set { _next = value; }
            }

            public Entry(int index, JsSchema value, int next)
            {
                Debug.Assert(value != null);

                _index = index;
                _value = value;
                _next = next;
            }

            public void Invalidate()
            {
                _value = null;
            }

            public override string ToString()
            {
                if (!IsValid)
                    return "IsValid=False";

                return String.Format(
                    "Index={0}, Value={1}, IsValid={2}, Next={3}",
                    Index,
                    Value,
                    IsValid,
                    Next
                );
            }
        }

        internal class SchemaTransformationHashSetDebugView
        {
            private readonly SchemaTransformationHashSet _container;

            public SchemaTransformationHashSetDebugView(SchemaTransformationHashSet container)
            {
                _container = container;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DisplayEntry[] Items
            {
                get
                {
                    var items = new List<DisplayEntry>();

                    foreach (int key in _container.GetKeys(false))
                    {
                        items.Add(new DisplayEntry(_container._entries[_container.FindEntry(key)]));
                    }

                    items.Sort((a, b) => Math.Abs(a.Index).CompareTo(Math.Abs(b.Index)));

                    return items.ToArray();
                }
            }

            private class DisplayEntry
            {
                public int Index { get; private set; }
                public JsSchema Value { get; private set; }

                public DisplayEntry(Entry entry)
                {
                    Index = entry.Index;
                    Value = entry.Value;
                }

                public override string ToString()
                {
                    return String.Format("Index={0}, Value={1}", Index, Value);
                }
            }
        }
    }
}

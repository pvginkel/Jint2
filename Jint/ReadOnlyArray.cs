using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint
{
    internal struct ReadOnlyArray<T> : IEquatable<ReadOnlyArray<T>>, IEnumerable<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly T[] _items;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>(new T[0]);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly ReadOnlyArray<T> Null = new ReadOnlyArray<T>();

        public T this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Length; }
        }

        public bool IsNull
        {
            get { return _items == null; }
        }

        public bool IsNullOrEmpty
        {
            get { return _items == null || _items.Length == 0; }
        }

        private ReadOnlyArray(T[] items)
        {
            _items = items;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ReadOnlyArray<T>))
                return false;

            return Equals((ReadOnlyArray<T>)obj);
        }

        public override int GetHashCode()
        {
            return _items != null ? _items.GetHashCode() : 0;
        }

        public bool Equals(ReadOnlyArray<T> other)
        {
            return _items == other._items;
        }

        public static bool operator ==(ReadOnlyArray<T> a, ReadOnlyArray<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ReadOnlyArray<T> a, ReadOnlyArray<T> b)
        {
            return !(a == b);
        }

        public static bool operator ==(ReadOnlyArray<T>? a, ReadOnlyArray<T>? b)
        {
            return a.GetValueOrDefault().Equals(b.GetValueOrDefault());
        }

        public static bool operator !=(ReadOnlyArray<T>? a, ReadOnlyArray<T>? b)
        {
            return !(a == b);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)_items).GetEnumerator();
        }

        public override string ToString()
        {
            if (_items == null)
                return null;

            return String.Format("Count = {0}", _items.Length);
        }

        public bool SequenceEquals(ReadOnlyArray<T> other)
        {
            return SequenceEquals(other, null);
        }

        public bool SequenceEquals(ReadOnlyArray<T> other, IEqualityComparer<T> comparer)
        {
            return ReadOnlyArray.SequenceEqual(this, other, comparer);
        }

        public static ReadOnlyArray<T> CreateFrom(T item)
        {
            return new ReadOnlyArray<T>(new[] { item });
        }

        public static ReadOnlyArray<T> CreateFrom(ICollection<T> items)
        {
            if (items == null)
                return Null;
            if (items.Count == 0)
                return Empty;

            var array = new T[items.Count];
            items.CopyTo(array, 0);
            return new ReadOnlyArray<T>(array);
        }

        public static ReadOnlyArray<T> CreateFrom(IEnumerable<T> items)
        {
            return new Builder(items).ToReadOnly();
        }

        public class Builder : List<T>
        {
            public Builder()
            {
            }

            public Builder(int capacity)
                : base(capacity)
            {
            }

            public Builder(IEnumerable<T> collection)
                : base(collection)
            {
            }

            public ReadOnlyArray<T> ToReadOnly()
            {
                if (Count == 0)
                    return Empty;

                return new ReadOnlyArray<T>(ToArray());
            }

            public ReadOnlyArray<T> ToReadOnlyOrNull()
            {
                if (Count == 0)
                    return Null;

                return ToReadOnly();
            }
        }
    }

    internal static class ReadOnlyArray
    {
        public static bool SequenceEqual<T>(ReadOnlyArray<T> a, ReadOnlyArray<T> b)
        {
            return SequenceEqual(a, b, null);
        }

        public static bool SequenceEqual<T>(ReadOnlyArray<T> a, ReadOnlyArray<T> b, IEqualityComparer<T> comparer)
        {
            if (a.Count != b.Count)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < a.Count; i++)
            {
                if (!comparer.Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }

        public static ReadOnlyArray<T> ToReadOnlyArray<T>(this IEnumerable<T> items)
        {
            return ReadOnlyArray<T>.CreateFrom(items);
        }

        public static ReadOnlyArray<T> ToReadOnlyArray<T>(this ICollection<T> items)
        {
            return ReadOnlyArray<T>.CreateFrom(items);
        }
    }
}

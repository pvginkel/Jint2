using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Support
{
    /// <summary>
    /// Sparse array implementation for the ArrayPropertyStore.
    /// </summary>
    /// <remarks>
    /// This is <b>NOT</b> a generic sparse array. This sparse array implementation
    /// is very restricted in its functionality and contains (just) enough to
    /// implement the ArrayPropertyStore.
    /// 
    /// The main idea here is that we cheat like crazy. Most of the time, this will
    /// just be have like a normal array. When you use the JS array like a normal
    /// array and just push to the back of it, you will only use a single real array.
    /// Only when you start doing crazy things like putting values at high offsets
    /// or creating large gaps, does this switch to an actual sparse array.
    /// </remarks>
    internal class SparseArray<T>
        where T : class
    {
        private const int ChunkShift = 5;
        private const int ChunkSize = 1 << ChunkShift;
        private const int InitialValuesSize = 20;
        private const int InitialChunkCount = 10;

        private T[] _values;
        private Chunk[] _chunks;
        private uint _chunkCount;

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    return null;

                if (_values != null)
                {
                    if (index < _values.Length)
                        return _values[index];
                    return null;
                }

                int offset = GetOffsetFromIndex(index);
                var chunk = FindChunk(offset);
                if (chunk.Found)
                    return _chunks[chunk.Index].Values[index - offset];

                return null;
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");

                if (_values != null)
                {
                    if (index < _values.Length)
                    {
                        _values[index] = value;
                        return;
                    }
                    if (index < _values.Length * 2)
                    {
                        // We allow the array to double in size every time
                        // we grow it.

                        GrowValues();
                        _values[index] = value;
                        return;
                    }

                    // We have a real array, but not enough room. Transfer the
                    // values to chunks and continue.
                    TransferToChunks();
                }

                int offset = GetOffsetFromIndex(index);
                var chunk = FindOrCreateChunk(offset);
                _chunks[chunk.Index].Values[index - offset] = value;
            }
        }

        public SparseArray()
        {
            _values = new T[InitialValuesSize];
        }

        public SparseArray(SparseArray<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (other._values != null)
            {
                _values = new T[other._values.Length];
                Array.Copy(other._values, _values, _values.Length);
            }
            else
            {
                _chunkCount = other._chunkCount;
                _chunks = new Chunk[other._chunks.Length];
                Array.Copy(other._chunks, _chunks, _chunks.Length);

                for (int i = 0; i < _chunkCount; i++)
                {
                    Array.Copy(other._chunks[i].Values, _chunks[i].Values, ChunkSize);
                }
            }
        }

        private int GetOffsetFromIndex(int index)
        {
            return index & ~(ChunkSize - 1);
        }

        private void TransferToChunks()
        {
            int chunkCount = (_values.Length >> ChunkShift) + 1;
            _chunks = new Chunk[Math.Max((int)(chunkCount * 1.2), InitialChunkCount)];

            for (int i = 0; i < chunkCount; i++)
            {
                int offset = i * ChunkSize;
                _chunks[i] = new Chunk(offset);

                int toCopy;
                if (i < chunkCount - 1)
                    toCopy = ChunkSize;
                else
                    toCopy = Math.Min(_values.Length - offset, ChunkSize);

                Array.Copy(_values, offset, _chunks[i].Values, 0, toCopy);
            }

            _values = null;
            _chunkCount = (uint)chunkCount;
        }

        private void GrowValues()
        {
            var newValues = new T[_values.Length * 2];
            Array.Copy(_values, newValues, _values.Length);
            _values = newValues;
        }

        private ChunkIndex FindOrCreateChunk(int offset)
        {
            var index = FindChunk(offset);

            if (!index.Found)
            {
                var chunk = new Chunk(offset);
                InsertChunk(chunk, index.Index);
            }

            return index;
        }

        private void InsertChunk(Chunk entry, uint index)
        {
            // We never create the chunks here; they are created by TransferToChunks.

            Debug.Assert(_chunkCount > 0);

            if (_chunks.Length == _chunkCount)
            {
                int newSize = (int)(_chunkCount * 1.2);
                if (newSize == _chunkCount)
                    newSize++;

                var destination = new Chunk[newSize];
                Array.Copy(_chunks, 0, destination, 0, index);
                destination[index] = entry;
                Array.Copy(_chunks, index, destination, index + 1, _chunkCount - index);

                _chunks = destination;
            }
            else
            {
                Array.Copy(_chunks, index, _chunks, index + 1, _chunkCount - index);
                _chunks[index] = entry;
            }

            _chunkCount++;
        }

        private ChunkIndex FindChunk(int offset)
        {
            uint lo = 0;
            uint hi = _chunkCount;

            if (hi <= 0)
                return new ChunkIndex(0, false);

            while (hi - lo > 3)
            {
                uint pv = (hi + lo) / 2;
                int checkOffset = _chunks[pv].Offset;

                if (offset == checkOffset)
                    return new ChunkIndex(pv, true);
                if (offset <= checkOffset)
                    hi = pv;
                else
                    lo = pv + 1;
            }

            do
            {
                int checkOffset = _chunks[lo].Offset;

                if (checkOffset == offset)
                    return new ChunkIndex(lo, true);
                if (checkOffset > offset)
                    break;

                lo++;
            }
            while (lo < hi);

            return new ChunkIndex(lo, false);
        }

        public bool ContainsKey(int index)
        {
            return this[index] != null;
        }

        public bool TryGetValue(int index, out T value)
        {
            value = this[index];
            return value != null;
        }

        public void Remove(int index)
        {
            this[index] = null;
        }

        public IEnumerable<int> GetKeys()
        {
            if (_values != null)
            {
                for (int i = 0; i < _values.Length; i++)
                {
                    if (_values[i] != null)
                        yield return i;
                }
            }
            else
            {
                for (int i = 0; i < _chunks.Length; i++)
                {
                    if (_chunks[i].IsValid)
                    {
                        var values = _chunks[i].Values;
                        int offset = _chunks[i].Offset;

                        for (int j = 0; j < values.Length; j++)
                        {
                            if (values[j] != null)
                                yield return offset + j;
                        }
                    }
                }
            }
        }

        public IEnumerable<T> GetValues()
        {
            if (_values != null)
            {
                foreach (var value in _values)
                {
                    if (value != null)
                        yield return value;
                }
            }
            else
            {
                for (int i = 0; i < _chunkCount; i++)
                {
                    foreach (var value in _chunks[i].Values)
                    {
                        if (value != null)
                            yield return value;
                    }
                }
            }
        }

        private struct ChunkIndex
        {
            private readonly uint _store;

            public ChunkIndex(uint index, bool found)
            {
                _store = index & 0x7FFFFFFF;

                if (found)
                    _store |= 0x80000000;
            }

            public bool Found
            {
                get { return (_store & 0x80000000) != 0; }
            }

            public uint Index
            {
                get { return _store & 0x7FFFFFFF; }
            }
        }

        public override string ToString()
        {
            if (_values != null)
                return String.Format("Values={0}", _values.Length);
            else
                return String.Format("Chunks={0}, ChunkCapacity={1}", _chunkCount, _chunks.Length);
        }

        [DebuggerDisplay("Offset={Offset}, IsValid={IsValid}")]
        private struct Chunk
        {
            private readonly int _offset;
            private readonly T[] _values;

            public int Offset
            {
                get { return _offset; }
            }

            public T[] Values
            {
                get { return _values; }
            }

            public bool IsValid
            {
                get { return _values != null; }
            }

            public Chunk(int offset)
            {
                _offset = offset;
                _values = new T[ChunkSize];
            }
        }
    }
}

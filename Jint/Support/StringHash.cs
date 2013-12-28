using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Support
{
    internal struct StringHash : IEquatable<StringHash>
    {
        private readonly ushort[] _hash;

        public StringHash(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            // This doesn't use something like MD5 or SHA1 to hash because this
            // hash is much much cheaper to calculate and we don't need a
            // cryptographically safe hash.

            _hash = new ushort[20];

            int offset = 0;
            foreach (char c in value)
            {
                _hash[offset] ^= c;
                offset = (offset + 1) % _hash.Length;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StringHash)
                return Equals((StringHash)obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (_hash == null)
                return 0;

            int result = 1;

            for (int i = 0; i < _hash.Length; i++)
            {
                result = result * 31 + _hash[i];
            }

            return result;
        }

        public bool Equals(StringHash other)
        {
            if (_hash == null || other._hash == null || _hash.Length != other._hash.Length)
                return false;

            for (int i = 0; i < _hash.Length; i++)
            {
                if (_hash[i] != other._hash[i])
                    return false;
            }

            return true;
        }
    }
}

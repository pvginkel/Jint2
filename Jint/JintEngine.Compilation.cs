using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint
{
    partial class JintEngine
    {
        private struct ScriptHash : IEquatable<ScriptHash>
        {
            private readonly string _fileName;
            private readonly StringHash _sourceHash;

            public ScriptHash(string fileName, string source)
            {
                _fileName = fileName;
                _sourceHash = new StringHash(source);
            }

            public override bool Equals(object obj)
            {
                if (obj is ScriptHash)
                    return Equals((ScriptHash)obj);

                return false;
            }

            public override int GetHashCode()
            {
                int result = 1;

                if (_fileName != null)
                    result = _fileName.GetHashCode();

                return result * 31 + _sourceHash.GetHashCode();
            }

            public bool Equals(ScriptHash other)
            {
                return
                    _fileName == other._fileName &&
                    _sourceHash.Equals(other._sourceHash);
            }
        }

        private struct FunctionHash : IEquatable<FunctionHash>
        {
            private readonly ReadOnlyArray<string> _parameters;
            private readonly StringHash _sourceHash;

            public FunctionHash(ReadOnlyArray<string> parameters, string source)
            {
                _parameters = parameters;
                _sourceHash = new StringHash(source);
            }

            public override int GetHashCode()
            {
                int result = _sourceHash.GetHashCode();

                for (int i = 0; i < _parameters.Count; i++)
                {
                    result = result * 31 + _parameters[i].GetHashCode();
                }

                return result;
            }

            public override bool Equals(object obj)
            {
                if (obj is FunctionHash)
                    return Equals((FunctionHash)obj);

                return false;
            }

            public bool Equals(FunctionHash other)
            {
                if (
                    _parameters == null || other._parameters == null ||
                    _parameters.Count != other._parameters.Count ||
                    !_sourceHash.Equals(other._sourceHash)
                )
                    return false;

                for (int i = 0; i < _parameters.Count; i++)
                {
                    if (_parameters[i] != other._parameters[i])
                        return false;
                }

                return true;
            }
        }
    }
}

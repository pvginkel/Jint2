using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class TypeMarker : IDisposable
        {
            private readonly BoundTypeManager _typeManager;
            private bool _disposed;

            public TypeMarker(BoundTypeManager typeManager)
            {
                _typeManager = typeManager;
            }

            public void MarkWrite(IBoundType type, BoundValueType valueType)
            {
                var internalType = (BoundType)type;

                if (type.Type == BoundValueType.Unset)
                    internalType.Type = valueType;
                else if (type.Type != valueType)
                    internalType.Type = BoundValueType.Unknown;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    foreach (BoundType type in _typeManager.Types)
                    {
                        if (!type.DefinitelyAssigned)
                            type.Type = BoundValueType.Unknown;
                    }

                    _disposed = true;
                }
            }
        }
    }
}

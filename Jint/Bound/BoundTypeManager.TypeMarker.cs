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

                // Initialize all globals to unknown; they come from the
                // GlobalScope object and are implicitly converted to Get/SetMember's.

                foreach (BoundType type in _typeManager.Types)
                {
                    if (type.Kind == BoundTypeKind.Global)
                        type.Type = BoundValueType.Unknown;
                }
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
                    // Force everything that is not definitely assigned to unknown
                    // so that we can assign JsUndefined.Instance to it.

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

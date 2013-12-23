﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class TypeMarker : IDisposable
        {
            private HashSet<BoundClosure> _closures;
            private BoundTypeManager _typeManager;
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

            public void MarkClosureUsage(BoundClosure closure)
            {
                if (_closures == null)
                    _closures = new HashSet<BoundClosure>();

                _closures.Add(closure);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_closures != null)
                        _typeManager.UsedClosures = _closures.ToReadOnlyArray();

                    _disposed = true;
                }
            }
        }
    }
}

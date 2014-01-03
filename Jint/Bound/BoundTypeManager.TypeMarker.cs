using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class TypeMarker : IDisposable
        {
            private readonly BoundTypeManager _typeManager;
            private Dictionary<IBoundType, Speculations> _speculations;
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

            public void SpeculateType(IBoundType boundType, SpeculatedType type, bool definite)
            {
                if (
                    boundType.Kind != BoundTypeKind.Local &&
                    boundType.Kind != BoundTypeKind.Temporary &&
                    boundType.Kind != BoundTypeKind.Magic
                )
                    return;

                var speculations = GetSpeculations(boundType);

                if (definite)
                {
                    Debug.Assert(speculations.Type == type || speculations.Type == SpeculatedType.Unknown);

                    speculations.Type = type;
                    speculations.Definite = true;
                }
                else
                {
                    switch (type)
                    {
                        case SpeculatedType.Array: speculations.ArrayCount++; break;
                        case SpeculatedType.Object: speculations.ObjectCount++; break;
                        default: throw new InvalidOperationException();
                    }
                }
            }

            private Speculations GetSpeculations(IBoundType boundType)
            {
                if (_speculations == null)
                    _speculations = new Dictionary<IBoundType, Speculations>();

                Speculations speculations;
                if (!_speculations.TryGetValue(boundType, out speculations))
                {
                    speculations = new Speculations();
                    _speculations.Add(boundType, speculations);
                }

                return speculations;
            }

            public void SpeculateType(IBoundType target, IBoundType source)
            {
                if (
                    (target.Kind != BoundTypeKind.Local && target.Kind != BoundTypeKind.Temporary) ||
                    (source.Kind != BoundTypeKind.Local && source.Kind != BoundTypeKind.Temporary)
                )
                    return;

                var sourceSpeculations = GetSpeculations(source);
                var targetSpeculations = GetSpeculations(target);

                if (sourceSpeculations.Definite)
                {
                    targetSpeculations.Definite = true;
                    targetSpeculations.Type = sourceSpeculations.Type;
                }
                else
                {
                    targetSpeculations.ArrayCount += sourceSpeculations.ArrayCount;
                    targetSpeculations.ObjectCount += sourceSpeculations.ObjectCount;
                }
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

                    if (_speculations != null)
                    {
                        foreach (var speculations in _speculations)
                        {
                            var type = (BoundType)speculations.Key;

                            if (
                                type.Type == BoundValueType.Object ||
                                type.Type == BoundValueType.Unknown
                            )
                            {
                                if (speculations.Value.Type != SpeculatedType.Unknown)
                                    type.SpeculatedType = speculations.Value.Type;
                                else if (speculations.Value.ObjectCount > speculations.Value.ArrayCount)
                                    type.SpeculatedType = SpeculatedType.Object;
                                else if (speculations.Value.ArrayCount > 0)
                                    type.SpeculatedType = SpeculatedType.Array;

#if TRACE_SPECULATION
                                Trace.WriteLine("Speculated: " + type.Name + " = " + type.SpeculatedType + " definite: " + speculations.Value.Definite);
#endif
                            }
                        }
                    }

                    _disposed = true;
                }
            }

            private class Speculations
            {
                public SpeculatedType Type { get; set; }
                public bool Definite { get; set; }
                public int ObjectCount { get; set; }
                public int ArrayCount { get; set; }
            }
        }
    }
}

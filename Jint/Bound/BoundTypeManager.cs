using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal partial class BoundTypeManager
    {
        private readonly HashSet<IBoundType> _types = new HashSet<IBoundType>();

        public IEnumerable<IBoundType> Types
        {
            get { return _types; }
        }

        public ReadOnlyArray<BoundClosure> UsedClosures { get; private set; }

        public IBoundType CreateType(BoundTypeKind type)
        {
            var result = new BoundType(this, type);

            _types.Add(result);

            return result;
        }

        public TypeMarker CreateTypeMarker()
        {
            return new TypeMarker(this);
        }

        public DefiniteAssignmentMarker CreateDefiniteAssignmentMarker(DefiniteAssignmentMarker.Branch parentBranch)
        {
            return new DefiniteAssignmentMarker(this, parentBranch);
        }

        [DebuggerDisplay("Kind={Kind}, Type={Type}, DefinitelyAssigned={DefinitelyAssigned}")]
        private class BoundType : IBoundType
        {
            private readonly BoundTypeManager _typeManager;

            public BoundTypeKind Kind { get; private set; }
            public BoundValueType Type { get; set; }
            public bool DefinitelyAssigned { get; set; }

            public BoundType(BoundTypeManager typeManager, BoundTypeKind kind)
            {
                _typeManager = typeManager;
                Kind = kind;
            }

            public void MarkUnused()
            {
                _typeManager._types.Remove(this);
            }
        }
    }
}

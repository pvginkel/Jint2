using System;
using System.Collections.Generic;
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

        public IBoundType CreateType(BoundTypeType type)
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

        private class BoundType : IBoundType
        {
            private readonly BoundTypeManager _typeManager;

            public BoundTypeType Type { get; private set; }
            public BoundValueType ValueType { get; set; }
            public bool DefinitelyAssigned { get; set; }

            public BoundType(BoundTypeManager typeManager, BoundTypeType type)
            {
                _typeManager = typeManager;
                Type = type;
            }

            public void MarkUnused()
            {
                _typeManager._types.Remove(this);
            }
        }
    }
}

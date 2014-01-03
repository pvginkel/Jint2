using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public IList<BoundMagicType> MagicTypes { get; private set; }

        public BoundTypeManager()
        {
            var magicTypes = new List<BoundMagicType>();

            MagicTypes = new ReadOnlyCollection<BoundMagicType>(magicTypes);

            foreach (var magicType in new[] { BoundMagicVariableType.Arguments, BoundMagicVariableType.Global, BoundMagicVariableType.This })
            {
                magicTypes.Add(new BoundMagicType(
                    magicType,
                    CreateType("<>" + magicType, BoundTypeKind.Magic)
                ));
            }
        }

        public IBoundType CreateType(string name, BoundTypeKind type)
        {
            var result = new BoundType(this, name, type);

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

        [DebuggerDisplay("Name={Name}, Kind={Kind}, Type={Type}, DefinitelyAssigned={DefinitelyAssigned}")]
        private class BoundType : IBoundType
        {
            private readonly BoundTypeManager _typeManager;

            public string Name { get; private set; }
            public BoundTypeKind Kind { get; private set; }
            public BoundValueType Type { get; set; }
            public SpeculatedType SpeculatedType { get; set; }
            public bool DefinitelyAssigned { get; set; }

            public BoundType(BoundTypeManager typeManager, string name, BoundTypeKind kind)
            {
                _typeManager = typeManager;
                Name = name;
                Kind = kind;
            }

            public void MarkUnused()
            {
                _typeManager._types.Remove(this);
            }
        }
    }
}

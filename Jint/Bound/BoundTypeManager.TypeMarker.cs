using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class TypeMarker
        {
            private readonly BoundTypeManager _typeManager;

            public MarkerBlock RootBlock { get; private set; }

            public TypeMarker(BoundTypeManager typeManager)
            {
                if (typeManager == null)
                    throw new ArgumentNullException("typeManager");

                _typeManager = typeManager;
                RootBlock = new MarkerBlock(this);
            }
        }

        public class MarkerBlock
        {
            private readonly TypeMarker _typeMarker;

            public Marker Marker { get; private set; }

            public MarkerBlock(TypeMarker typeMarker)
            {
                if (typeMarker == null)
                    throw new ArgumentNullException("typeMarker");

                _typeMarker = typeMarker;
                Marker = new Marker(this);
            }
        }

        public class Marker
        {
            private readonly MarkerBlock _block;

            public Marker(MarkerBlock block)
            {
                if (block == null)
                    throw new ArgumentNullException("block");

                _block = block;
            }

            internal void MarkWrite(IBoundType boundNodeType, BoundValueType boundValueType)
            {
                throw new NotImplementedException();
            }

            internal void MarkRead(IBoundType boundNodeType)
            {
                throw new NotImplementedException();
            }
        }
    }
}

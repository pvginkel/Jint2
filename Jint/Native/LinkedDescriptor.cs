using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native {
    /// <summary>
    /// Linked descriptor - a link to the particular property (represented by a descriptor) of the particular object.
    /// </summary>
    /// <remarks>
    /// This descriptors are used in scopes
    /// </remarks>
    internal class LinkedDescriptor : Descriptor {
        private readonly Descriptor _descriptor;
        private readonly JsDictionaryObject _that;

        /// <summary>
        /// Constructs new descriptor
        /// </summary>
        /// <param name="owner">An owner of the new descriptor</param>
        /// <param name="name">A name of the new descriptor</param>
        /// <param name="source">A property descriptor of the target object to which we should link to</param>
        /// <param name="that">A target object to whose property we are linking. This parameter will be
        /// used in the calls to a 'Get' and 'Set' properties of the source descriptor.</param>
        public LinkedDescriptor(JsDictionaryObject owner, string name, Descriptor source, JsDictionaryObject that)
            : base(owner, name) {
            if (source.IsReference) {
                LinkedDescriptor sourceLink = source as LinkedDescriptor;
                _descriptor = sourceLink._descriptor;
                _that = sourceLink._that;
            } else
                _descriptor = source;
            Enumerable = true;
            Writable = true;
            Configurable = true;
            _that = that;
        }

        public JsDictionaryObject TargetObject {
            get { return _that; }
        }

        public override bool IsReference {
            get { return true ; }
        }

        public override bool IsDeleted {
            get {
                return _descriptor.IsDeleted;
            }
            protected set {
                /* do nothing */;
            }
        }

        public override Descriptor Clone() {
            return new LinkedDescriptor(Owner, Name, this, TargetObject) {
                Writable = Writable,
                Configurable = Configurable,
                Enumerable = Enumerable
            };
        }

        public override JsInstance Get(JsDictionaryObject that) {
            return _descriptor.Get(that);
        }

        public override void Set(JsDictionaryObject that, JsInstance value) {
            _descriptor.Set(that, value);
        }

        internal override DescriptorType DescriptorType {
            get { return DescriptorType.Value; }
        }
    }
}

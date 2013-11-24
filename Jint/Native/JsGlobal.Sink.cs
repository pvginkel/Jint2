using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        /// <remarks>
        /// This is a special type which is only used as the sink object for
        /// prototypes. It's used when we don't have a parent prototype, to
        /// make sure that there is something.
        /// </remarks>
        [Serializable]
        private class Sink : JsObject
        {
            public override JsType Type
            {
                get { return JsType.Null; }
            }

            public override string Class
            {
                get { return ClassObject; }
            }

            public override int Length
            {
                get
                {
                    return 0;
                }
                set { }
            }

            public Sink(JsGlobal global)
                : base(global, null, null)
            {
            }

            public override bool ToBoolean()
            {
                return false;
            }

            public override double ToNumber()
            {
                return 0d;
            }

            public override string ToString()
            {
                return "null";
            }

            public override JsInstance ToPrimitive(PrimitiveHint hint)
            {
                return this;
            }

            public override Descriptor GetDescriptor(string index)
            {
                return null;
            }

            public override IEnumerable<string> GetKeys()
            {
                return new string[0];
            }

            public override object Value
            {
                get { return null; }
                set { }
            }

            public override void DefineOwnProperty(Descriptor value)
            {
            }

            public override bool HasProperty(string key)
            {
                return false;
            }

            public override bool HasOwnProperty(string key)
            {
                return false;
            }

            public override JsInstance this[string index]
            {
                get { return JsUndefined.Instance; }
                set { }
            }
        }
    }
}

using Jint.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsArray : JsObject
    {
        internal JsArray(JsGlobal global, JsObject prototype)
            : this(global, null, prototype)
        {
        }

        internal JsArray(JsGlobal global, SparseArray<JsInstance> array, JsObject prototype)
            : base(global, null, prototype, false)
        {
            // We always create the property store because we expect it to be used
            // and it's easier for the array functions.

            if (array != null)
                PropertyStore = new ArrayPropertyStore(this, array);
            else
                PropertyStore = new ArrayPropertyStore(this);
        }

        // 15.4.2
        public override string Class
        {
            get { return JsNames.ClassArray; }
        }
    }
}

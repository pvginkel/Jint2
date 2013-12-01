using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsArray : JsObject
    {
        internal JsArray(JsGlobal global, JsObject prototype)
            : this(global, null, 0, prototype)
        {
        }

        internal JsArray(JsGlobal global, SortedList<int, JsInstance> data, int length, JsObject prototype)
            : base(global, null, prototype)
        {
            // We always create the property store because we expect it to be used
            // and it's easier for the array functions.
            PropertyStore = new ArrayPropertyStore(this, data);
            Length = length;
        }

        // 15.4.2
        public override string Class
        {
            get { return JsNames.ClassArray; }
        }
    }
}

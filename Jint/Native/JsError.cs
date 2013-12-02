using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsError : JsObject
    {
        internal JsError(JsGlobal global, JsObject prototype, string message)
            : base(global, null, prototype, false)
        {
            this["message"] = JsString.Create(message);
            Value = message;
        }

        public override string Class
        {
            get { return JsNames.ClassError; }
        }
    }
}

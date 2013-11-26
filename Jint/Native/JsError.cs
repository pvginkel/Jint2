using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsError : JsObject
    {
        private string Message
        {
            get { return this["message"].ToString(); }
            set { this["message"] = JsString.Create(value); }
        }

        public override object Value
        {
            get { return Message; }
        }

        internal JsError(JsGlobal global, JsObject prototype, string message)
            : base(global, null, prototype)
        {
            Message = message;
        }

        public override string Class
        {
            get { return ClassError; }
        }

        public override bool IsClr
        {
            get { return false; }
        }
    }
}

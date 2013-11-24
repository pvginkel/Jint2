using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;

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

        public JsError(JsGlobal global, JsObject prototype)
            : this(global, prototype, string.Empty)
        {
        }

        public JsError(JsGlobal global, JsObject prototype, string message)
            : base(global, prototype)
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

using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;

namespace Jint.Native {
    [Serializable]
    public class JsError : JsObject {
        private string Message
        {
            get { return this["message"].ToString(); }
            set { this["message"] = _global.StringClass.New(value); }
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override object Value {
            get {
                return Message;
            }
        }

        private readonly IGlobal _global;

        public JsError(IGlobal global)
            : this(global, string.Empty) {
        }

        public JsError(IGlobal global, string message)
            : base(global.ErrorClass.PrototypeProperty) {
            _global = global;
            Message = message;
        }

        public override string Class {
            get { return ClassError; }
        }

        public override string ToString() {
            return Value.ToString();
        }
    }
}

﻿using System;
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
            set { this["message"] = _global.StringClass.New(value); }
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override object Value
        {
            get
            {
                return Message;
            }
        }

        public JsObject PrototypeProperty
        {
            get { return this[JsFunction.PrototypeName] as JsObject; }
            set { this[JsFunction.PrototypeName] = value; }
        }

        private readonly IGlobal _global;

        public JsError(IGlobal global, JsObject prototype)
            : this(global, prototype, string.Empty)
        {
            PrototypeProperty = global.ObjectClass.New(PrototypeProperty);
        }

        public JsError(IGlobal global, JsObject prototype, string message)
            : base(prototype)
        {
            _global = global;
            Message = message;
        }

        public override string Class
        {
            get { return ClassError; }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}

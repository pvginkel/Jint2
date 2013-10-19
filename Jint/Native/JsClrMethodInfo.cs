using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Delegates;
using System.Reflection;

namespace Jint.Native {
    [Serializable]
    public class JsClrMethodInfo : JsObject {
        private readonly string _value;

        public JsClrMethodInfo() {
        }

        public JsClrMethodInfo(string method) {
            _value = method;
        }

        public override bool ToBoolean() {
            return false;
        }

        public override double ToNumber() {
            return 0;
        }

        public override string ToString() {
            return String.Empty;
        }

        public override string Class {
            get { return "clrMethodInfo"; }
        }

        public override object Value {
            get {
                return _value;
            }
        }
    }
}

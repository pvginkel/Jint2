using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Delegates;
using System.Reflection;

namespace Jint.Native
{
    [Serializable]
    public class JsClrMethodInfo : JsObject
    {
        private readonly string _value;

        public JsClrMethodInfo(JsGlobal global)
            : this(global, null)
        {
        }

        public JsClrMethodInfo(JsGlobal global, string method)
            : base(global)
        {
            _value = method;
        }

        public override string Class
        {
            get { return "clrMethodInfo"; }
        }

        public override object Value
        {
            get { return _value; }
        }
    }
}

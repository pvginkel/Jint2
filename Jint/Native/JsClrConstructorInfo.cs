using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Delegates;
using System.Reflection;

namespace Jint.Native
{
    [Serializable]
    public class JsClrConstructorInfo : JsObject
    {
        private readonly ConstructorInfo _value;

        public JsClrConstructorInfo(JsGlobal global)
            : this(global, null)
        {
        }

        public JsClrConstructorInfo(JsGlobal global, ConstructorInfo clr)
            : base(global)
        {
            _value = clr;
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return 0;
        }

        public override string ToString()
        {
            return _value.Name;
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

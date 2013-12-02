using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsArguments : JsObject
    {
        internal JsArguments(JsGlobal global, JsFunction callee, JsInstance[] arguments)
            : base(global, null, global.CreateObject(), false)
        {
            int length;

            // Add the named parameters
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    DefineOwnProperty(new ValueDescriptor(this, i.ToString(CultureInfo.InvariantCulture), arguments[i], PropertyAttributes.DontEnum));
                }

                length = arguments.Length;
            }
            else
            {
                length = 0;
            }

            DefineOwnProperty(new ValueDescriptor(this, "callee", callee, PropertyAttributes.DontEnum));
            DefineOwnProperty(new ValueDescriptor(this, "length", JsNumber.Create(length), PropertyAttributes.DontEnum));
        }

        public override string Class
        {
            get { return JsNames.ClassArguments; }
        }
    }
}

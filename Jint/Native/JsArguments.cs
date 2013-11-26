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
            : base(global, null, global.CreateObject())
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

            DefineOwnProperty(new ValueDescriptor(this, JsNames.Callee, callee, PropertyAttributes.DontEnum));
            DefineOwnProperty(new ValueDescriptor(this, JsNames.Length, JsNumber.Create(length), PropertyAttributes.DontEnum));
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public override string Class
        {
            get { return ClassArguments; }
        }
    }
}

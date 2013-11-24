using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public class JsArguments : JsObject
    {
        public const string CalleeName = "callee";
        private const string LengthPropertyName = "length";

        public JsArguments(JsGlobal global, JsFunction callee, JsInstance[] arguments)
            : base(global, global.ObjectClass.New())
        {
            int length;

            // Add the named parameters
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    DefineOwnProperty(new ValueDescriptor(this, i.ToString(CultureInfo.InvariantCulture), arguments[i]) { Enumerable = false });
                }

                length = arguments.Length;
            }
            else
            {
                length = 0;
            }

            DefineOwnProperty(new ValueDescriptor(this, CalleeName, callee) { Enumerable = false });
            DefineOwnProperty(new ValueDescriptor(this, LengthPropertyName, JsNumber.Create(length)) { Enumerable = false, Configurable = true });
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return this[LengthPropertyName].ToNumber();
        }

        public override string Class
        {
            get { return ClassArguments; }
        }
    }
}

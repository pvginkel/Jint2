using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    partial class JintRuntime
    {
        public JsInstance New_Boolean(bool value)
        {
            return _booleanClass.New(value);
        }

        public JsInstance New_Number(double value)
        {
            return _numberClass.New(value);
        }

        public JsInstance New_String(string value)
        {
            return _stringClass.New(value);
        }
    }
}

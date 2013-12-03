using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsComparer : IComparer<JsInstance>
    {
        public IJintBackend Backend { get; private set; }
        public JsObject Function { get; private set; }

        public JsComparer(IJintBackend backend, JsObject function)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");
            if (function == null)
                throw new ArgumentNullException("function");

            Backend = backend;
            Function = function;
        }

        public int Compare(JsInstance x, JsInstance y)
        {
            return Backend.Compare(Function, x, y);
        }
    }
}

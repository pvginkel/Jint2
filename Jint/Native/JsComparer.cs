using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native {
    [Serializable]
    public class JsComparer : IComparer<JsInstance> {
        public IJintBackend Backend { get; private set; }
        public JsFunction Function { get; private set; }

        public JsComparer(IJintBackend backend, JsFunction function) {
            if (backend == null)
                throw new ArgumentNullException("backend");
            if (function == null)
                throw new ArgumentNullException("function");

            Backend = backend;
            Function = function;
        }

        public int Compare(JsInstance x, JsInstance y) {
            return Backend.Compare(Function, x, y);
        }
    }
}

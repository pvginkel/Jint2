using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsComparer : IComparer<JsBox>
    {
        public JintEngine Engine { get; private set; }
        public JsObject Function { get; private set; }

        public JsComparer(JintEngine engine, JsObject function)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (function == null)
                throw new ArgumentNullException("function");

            Engine = engine;
            Function = function;
        }

        public int Compare(JsBox x, JsBox y)
        {
            return Engine.Compare(Function, x, y);
        }
    }
}

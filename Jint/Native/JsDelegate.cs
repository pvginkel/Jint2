using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public class JsDelegate
    {
        public string Name { get; private set; }
        public JsFunction Delegate { get; private set; }
        public int ArgumentCount { get; private set; }
        public object Closure { get; private set; }

        public JsDelegate(string name, JsFunction @delegate, int argumentCount, object closure)
        {
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            Name = name;
            Delegate = @delegate;
            ArgumentCount = argumentCount;
            Closure = closure;
        }
    }
}

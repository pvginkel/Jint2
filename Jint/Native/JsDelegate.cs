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

        public JsDelegate(string name, JsFunction @delegate, int argumentCount)
        {
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            Name = name;
            Delegate = @delegate;
            ArgumentCount = argumentCount;
        }

        public override string ToString()
        {
            var method = Delegate.Method;
            var attributes = method.GetCustomAttributes(typeof(JsFunctionSourceAttribute), false);
            if (attributes.Length > 0)
                return ((JsFunctionSourceAttribute)attributes[0]).Source;
            if (Name != null)
                return String.Format("function {0} ( ) {{ [native code] }}", Name);
            return "(anonymous function)";
        }
    }
}

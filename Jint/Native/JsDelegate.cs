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
        public string SourceCode { get; private set; }

        public JsDelegate(string name, JsFunction @delegate, int argumentCount, string sourceCode)
        {
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            Name = name;
            Delegate = @delegate;
            ArgumentCount = argumentCount;
            SourceCode = sourceCode;
        }

        public override string ToString()
        {
            if (SourceCode != null)
                return SourceCode;
            if (Name != null)
                return String.Format("function {0} ( ) {{ [native code] }}", Name);
            return "(anonymous function)";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public class JsFunctionWrapper : JsFunction
    {
        public Func<JsInstance[], JsInstance> Delegate { get; set; }

        public JsFunctionWrapper(Func<JsInstance[], JsInstance> d, JsObject prototype)
            : base(prototype)
        {
            Delegate = d;
        }

        public override JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            try
            {
                //visitor.CurrentScope["this"] = visitor.Global;
                var result = Delegate(parameters) ?? JsUndefined.Instance;

                return new JsFunctionResult(result, that);
            }
            catch (Exception e)
            {
                if (e.InnerException is JsException)
                {
                    throw e.InnerException;
                }

                throw;
            }
        }

        public override string ToString()
        {
            return String.Format("function {0}() {{ [native code] }}", Delegate.Method.Name);
        }
    }
}

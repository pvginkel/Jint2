using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsFunctionWrapper : JsFunction
    {
        private readonly Func<JsInstance[], JsInstance> _delegate;
        private readonly Func<JsGlobal, JsInstance[], JsInstance> _globalDelegate;

        public JsFunctionWrapper(JsGlobal global, Func<JsInstance[], JsInstance> @delegate, JsObject prototype)
            : base(global, prototype)
        {
            _delegate = @delegate;
        }

        public JsFunctionWrapper(JsGlobal global, Func<JsGlobal, JsInstance[], JsInstance> @delegate, JsObject prototype)
            : base(global, prototype)
        {
            _globalDelegate = @delegate;
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            try
            {
                JsInstance result;
                if (_delegate != null)
                    result = _delegate(parameters);
                else
                    result = _globalDelegate(Global, parameters);

                return new JsFunctionResult(result ?? JsUndefined.Instance, that);
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
    }
}

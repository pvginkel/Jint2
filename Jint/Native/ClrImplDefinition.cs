using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Delegates;
using System.Reflection;

namespace Jint.Native
{
    /// <summary>
    /// Wraps a delegate which can be called as a method on an object, with or without parameters.
    /// </summary>
    [Serializable]
    public class ClrImplDefinition<T> : JsFunction
        where T : JsInstance
    {
        private readonly int? _length;
        private readonly Func<T, JsInstance[], JsInstance> _parameterImpl;
        private readonly Func<JsGlobal, T, JsInstance[], JsInstance> _parameterGlobalImpl;
        private readonly Func<JsGlobal, T, JsInstance> _globalImpl;
        private readonly Func<T, JsInstance> _impl;

        private ClrImplDefinition(JsGlobal global, int? length, JsObject prototype)
            : base(global, prototype)
        {
            _length = length;
        }

        public ClrImplDefinition(JsGlobal global, Func<T, JsInstance[], JsInstance> impl, JsObject prototype)
            : this(global, (int?)null, prototype)
        {
            _parameterImpl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<T, JsInstance[], JsInstance> impl, int length, JsObject prototype)
            : this(global, length, prototype)
        {
            _parameterImpl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<T, JsInstance> impl, JsObject prototype)
            : this(global, (int?)null, prototype)
        {
            _impl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<T, JsInstance> impl, int length, JsObject prototype)
            : this(global, length, prototype)
        {
            _impl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<JsGlobal, T, JsInstance[], JsInstance> impl, JsObject prototype)
            : this(global, (int?)null, prototype)
        {
            _parameterGlobalImpl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<JsGlobal, T, JsInstance[], JsInstance> impl, int length, JsObject prototype)
            : this(global, length, prototype)
        {
            _parameterGlobalImpl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<JsGlobal, T, JsInstance> impl, JsObject prototype)
            : this(global, (int?)null, prototype)
        {
            _globalImpl = impl;
        }

        public ClrImplDefinition(JsGlobal global, Func<JsGlobal, T, JsInstance> impl, int length, JsObject prototype)
            : this(global, length, prototype)
        {
            _globalImpl = impl;
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var casted = that as T;
            if (casted == null)
            {
                var jsObject = that as JsObject;
                JsFunction constructor = null;
                if (jsObject != null)
                    constructor = jsObject["constructor"] as JsFunction;
                throw new JsException(Global.TypeErrorClass.New("incompatible type: " + (constructor == null ? "<unknown>" : constructor.Name)));
            }

            try
            {
                JsInstance result;
                if (_impl != null)
                    result = _impl(casted);
                else if (_parameterImpl != null)
                    result = _parameterImpl(casted, parameters);
                else if (_globalImpl != null)
                    result = _globalImpl(Global, casted);
                else
                    result = _parameterGlobalImpl(Global, casted, parameters);

                return new JsFunctionResult(result, result);
            }
            catch (Exception e)
            {
                if (e.InnerException is JsException)
                    throw e.InnerException;

                throw;
            }
        }

        public override int Length
        {
            get { return _length.GetValueOrDefault(base.Length); }
        }

        public override string ToString()
        {
            return String.Format("function {0}() {{ [native code] }}", _impl.Method.Name);
        }
    }
}

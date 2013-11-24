using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Jint.Marshal;
using Jint.Expressions;

namespace Jint.Native
{
    /// <summary>
    /// Wraps a single method which is implemented by the delegate
    /// </summary>
    public class NativeMethod : JsFunction
    {

        private readonly MethodInfo _nativeMethod;
        private readonly JsMethodImpl _impl;

        public NativeMethod(JsGlobal global, JsMethodImpl impl, MethodInfo nativeMethod, JsObject prototype) :
            base(global, prototype)
        {
            if (impl == null)
                throw new ArgumentNullException("impl");
            _nativeMethod = nativeMethod;
            _impl = impl;
            if (nativeMethod != null)
            {
                Name = nativeMethod.Name;
                foreach (var item in nativeMethod.GetParameters())
                    Arguments.Add(item.Name);
            }
        }

        public NativeMethod(JsGlobal global, JsMethodImpl impl, JsObject prototype)
            : this(global, impl, null, prototype)
        {
            foreach (var item in impl.Method.GetParameters())
                Arguments.Add(item.Name);
        }

        public NativeMethod(JsGlobal global, MethodInfo info, JsObject prototype)
            : base(global, prototype)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            if (global == null)
                throw new ArgumentNullException("global");

            _nativeMethod = info;
            _impl = global.Marshaller.WrapMethod(info, true);
            Name = info.Name;

            foreach (var item in info.GetParameters())
                Arguments.Add(item.Name);
        }

        public override bool IsClr
        {
            get { return true; }
        }

        public MethodInfo GetWrappedMethod()
        {
            return _nativeMethod;
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var original = new JsInstance[parameters.Length];
            Array.Copy(parameters, original, parameters.Length);

            var result = _impl(Global, that, parameters);

            var outParameters = new bool[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                outParameters[i] = !ReferenceEquals(parameters[i], original[i]);
            }

            return new JsFunctionResult(result, that, outParameters);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs)
        {
            throw new JintException("This method can't be used as a constructor");
        }

        public override string GetBody()
        {
            return "[native code]";
        }

        public override JsInstance ToPrimitive(PrimitiveHint hint)
        {
            return JsString.Create(ToString());
        }
    }
}

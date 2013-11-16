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

        public NativeMethod(JsMethodImpl impl, MethodInfo nativeMethod, JsObject prototype) :
            base(prototype)
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

        public NativeMethod(JsMethodImpl impl, JsObject prototype) :
            this(impl, null, prototype)
        {
            foreach (var item in impl.Method.GetParameters())
                Arguments.Add(item.Name);
        }

        public NativeMethod(MethodInfo info, JsObject prototype, IGlobal global) :
            base(prototype)
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
            get
            {
                return true;
            }
        }

        public MethodInfo GetWrappedMethod()
        {
            return _nativeMethod;
        }

        public override JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            var original = new JsInstance[parameters.Length];
            Array.Copy(parameters, original, parameters.Length);

            var result = _impl(global, that, parameters);

            var outParameters = new bool[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                outParameters[i] = !ReferenceEquals(parameters[i], original[i]);
            }

            return new JsFunctionResult(result, that, outParameters);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, IGlobal global)
        {
            throw new JintException("This method can't be used as a constructor");
        }

        public override string GetBody()
        {
            return "[native code]";
        }

        public override JsInstance ToPrimitive(IGlobal global)
        {
            return global.StringClass.New(ToString());
        }
    }

}

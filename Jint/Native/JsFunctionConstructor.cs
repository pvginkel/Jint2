using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsFunctionConstructor : JsConstructor
    {
        public JsFunctionConstructor(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            Name = "Function";
        }

        internal void InitPrototype()
        {
            // We can't build the prototype here because we're bootstrapping.

            var prototype = Prototype;

            prototype.DefineOwnProperty("constructor", Global.FunctionClass.New<JsInstance>(GetConstructor), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty(CallName, new JsCallFunction(prototype), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(ApplyName, new JsApplyFunction(prototype), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("toString", Global.FunctionClass.New<JsObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", Global.FunctionClass.New<JsObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(new PropertyDescriptor<JsObject>(Global, prototype, "length", GetLengthImpl, SetLengthImpl));
        }

        private static JsInstance GetConstructor(JsInstance target)
        {
            throw new NotImplementedException();
        }

        public static JsInstance GetLengthImpl(JsObject target)
        {
            return JsNumber.Create(target.Length);
        }

        public static JsInstance SetLengthImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            int length = (int)parameters[0].ToNumber();

            if (length < 0 || double.IsNaN(length) || double.IsInfinity(length))
                throw new JsException(global.RangeErrorClass.New("invalid length"));

            var obj = (JsObject)target;
            obj.Length = length;

            return parameters[0];
        }

        public static JsInstance GetLength(JsObject target)
        {
            return JsNumber.Create(target.Length);
        }

        public JsFunction New()
        {
            return new JsFunction(new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<T, JsInstance> impl) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<T, JsInstance> impl, int length) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, length, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl, int length) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, length, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<JsGlobal, T, JsInstance> impl) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<JsGlobal, T, JsInstance> impl, int length) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, length, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<JsGlobal, T, JsInstance[], JsInstance> impl) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, new JsObject(Prototype));
        }

        public JsFunction New<T>(Func<JsGlobal, T, JsInstance[], JsInstance> impl, int length) where T : JsInstance
        {
            return new ClrImplDefinition<T>(impl, length, new JsObject(Prototype));
        }

        public JsFunction New(Delegate @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            var impl = Global.Marshaller.WrapMethod(@delegate.GetType().GetMethod("Invoke"), false);

            var wrapper = new JsObject(@delegate, JsNull.Instance);

            return New<JsInstance>((that, args) => impl(Global, wrapper, args));
        }

        public override JsFunctionResult Execute(JsGlobal visitor, JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var result = Construct(parameters, null, null);
            return new JsFunctionResult(result, result);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, JsGlobal global)
        {
            return Global.Backend.CompileFunction(parameters, genericArgs);
        }

        public static JsInstance ToString2(JsObject target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToSource());
        }
    }
}

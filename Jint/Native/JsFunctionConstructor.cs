﻿using System;
using System.Collections.Generic;
using System.Text;
using Jint.Backend.Dlr;
using Jint.Expressions;
using Jint.Delegates;
using Jint.Marshal;

namespace Jint.Native
{
    [Serializable]
    public class JsFunctionConstructor : JsConstructor
    {
        public JsFunctionConstructor(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            Name = "Function";
            DefineOwnProperty(PrototypeName, prototype, PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
        }

        public override void InitPrototype(JsGlobal global)
        {
            var prototype = PrototypeProperty;

            prototype.DefineOwnProperty("constructor", this, PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty(CallName, new JsCallFunction(this), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(ApplyName, new JsApplyFunction(this), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("toString", New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(new PropertyDescriptor<JsObject>(global, prototype, "length", GetLengthImpl, SetLengthImpl));
        }

        public JsInstance GetLengthImpl(JsDictionaryObject target)
        {
            return Global.NumberClass.New(target.Length);
        }

        public JsInstance SetLengthImpl(JsInstance target, JsInstance[] parameters)
        {
            int length = (int)parameters[0].ToNumber();

            if (length < 0 || double.IsNaN(length) || double.IsInfinity(length))
            {
                throw new JsException(Global.RangeErrorClass.New("invalid length"));
            }

            JsDictionaryObject obj = (JsDictionaryObject)target;
            obj.Length = length;

            return parameters[0];
        }

        public JsInstance GetLength(JsDictionaryObject target)
        {
            return Global.NumberClass.New(target.Length);
        }

        public JsFunction New()
        {
            JsFunction function = new JsFunction(PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            return function;
        }

        public JsFunction New<T>(Func<T, JsInstance> impl) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }
        public JsFunction New<T>(Func<T, JsInstance> impl, int length) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, length, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }
        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl, int length) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, length, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public JsFunction New(Delegate d)
        {
            if (d == null)
                throw new ArgumentNullException();
            //JsFunction function = new ClrFunction(d, PrototypeProperty);

            JsMethodImpl impl = Global.Marshaller.WrapMethod(d.GetType().GetMethod("Invoke"), false);
            JsObject wrapper = new JsObject(d, JsNull.Instance);

            JsFunction function = New<JsInstance>((that, args) => impl(Global, wrapper, args));
            function.PrototypeProperty = Global.ObjectClass.New(function);

            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public override JsFunctionResult Execute(JsGlobal visitor, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            var result = Construct(parameters, null, null);
            return new JsFunctionResult(result, result);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, JsGlobal global)
        {
            return Global.Backend.CompileFunction(parameters, genericArgs);
        }

        public JsInstance ToString2(JsDictionaryObject target, JsInstance[] parameters)
        {
            return Global.StringClass.New(target.ToSource());
        }
    }
}

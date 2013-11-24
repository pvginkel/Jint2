using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    class NativeTypeConstructor : NativeConstructor
    {
        protected NativeTypeConstructor(JsGlobal global, JsObject typePrototype)
            : base(typeof(Type), global, typePrototype)
        {
            // redefine prototype
            Prototype = typePrototype;
        }

        /// <summary>
        /// A static fuction for creating a constructor for <c>System.Type</c>
        /// </summary>
        /// <remarks>It also creates and initializes [[prototype]] and 'prototype' property to
        /// the same function object.</remarks>
        /// <param name="global">Global object</param>
        /// <returns>A js constructor function</returns>
        public static NativeTypeConstructor CreateNativeTypeConstructor(JsGlobal global)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            JsObject proto = global.FunctionClass.New();
            var inst = new NativeTypeConstructor(global, proto);
            inst.InitPrototype(global);
            inst.SetupNativeProperties(inst);
            return inst;
        }

        public override JsInstance Wrap<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value is Type)
            {
                var res = new NativeConstructor(value as Type, Global, Prototype);
                res.InitPrototype(Global);
                SetupNativeProperties(res);
                return res;
            }
            else
                throw new JintException("Attempt to wrap '" + value.GetType().FullName + "' with '" + typeof(Type).FullName + "'");
        }

        public JsInstance WrapSpecialType(Type value, JsObject prototype)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var res = new NativeConstructor(value, Global, prototype);
            res.InitPrototype(Global);
            SetupNativeProperties(res);
            return res;
        }
    }
}

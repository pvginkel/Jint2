using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsObjectConstructor : JsConstructor
    {
        public JsObjectConstructor(JsGlobal global, JsObject rootPrototype)
            : base(global, rootPrototype)
        {
            Name = "Object";
        }

        internal void InitPrototype()
        {
            // We need to keep this because the prototype is passed to the constructor rather than created in it
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("constructor", Global.FunctionClass.New<JsInstance>(GetConstructor), PropertyAttributes.DontEnum);

            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("toString", Global.FunctionClass.New<JsInstance>(ToStringImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("toLocaleString", Global.FunctionClass.New<JsInstance>(ToStringImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("valueOf", Global.FunctionClass.New<JsInstance>(ValueOfImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("hasOwnProperty", Global.FunctionClass.New<JsDictionaryObject>(HasOwnPropertyImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("isPrototypeOf", Global.FunctionClass.New<JsInstance>(IsPrototypeOfImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("propertyIsEnumerable", Global.FunctionClass.New<JsDictionaryObject>(PropertyIsEnumerableImpl), PropertyAttributes.DontEnum);
            ((JsDictionaryObject)this).Prototype.DefineOwnProperty("getPrototypeOf", new JsFunctionWrapper(GetPrototypeOfImpl, Global.FunctionClass.Prototype), PropertyAttributes.DontEnum);

            if (Global.HasOption(Options.EcmaScript5))
            {
                ((JsDictionaryObject)this).Prototype.DefineOwnProperty("defineProperty", new JsFunctionWrapper(DefineProperty, Global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
                ((JsDictionaryObject)this).Prototype.DefineOwnProperty("__lookupGetter__", Global.FunctionClass.New<JsDictionaryObject>(GetGetFunction), PropertyAttributes.DontEnum);
                ((JsDictionaryObject)this).Prototype.DefineOwnProperty("__lookupSetter__", Global.FunctionClass.New<JsDictionaryObject>(GetSetFunction), PropertyAttributes.DontEnum);
            }
        }

        private static JsInstance GetConstructor(JsInstance arg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new JsObject, sets a [[Prototype]] to the Object.PrototypeProperty and a 'constructor' property to the specified function
        /// </summary>
        /// <param name="constructor">JsFunction which is used as a constructor</param>
        /// <returns>new object</returns>
        public JsObject New(JsFunction constructor)
        {
            return New(constructor, null);
        }

        /// <summary>
        /// Creates new JsObject, sets a [[Prototype]] to the Prototype parameter and a 'constructor' property to the specified function.
        /// </summary>
        /// <param name="constructor">JsFunction which is used as a constructor</param>
        /// <param name="prototype">JsObjetc which is used as a prototype</param>
        /// <returns>new object</returns>
        public JsObject New(JsFunction constructor, JsObject prototype)
        {
            JsObject obj = new JsObject(prototype ?? Prototype);
            obj.DefineOwnProperty(new ValueDescriptor(obj, ConstructorName, constructor) { Enumerable = false });
            return obj;
        }

        /// <summary>
        /// Creates a new object which holds a specified value
        /// </summary>
        /// <param name="value">Value to store in the new object</param>
        /// <returns>new object</returns>
        public JsObject New(object value)
        {
            return new JsObject(value, Prototype);
        }

        public JsObject New()
        {
            return New((object)null);
        }

        /// <summary>
        /// 15.2.2.1
        /// </summary>
        public override JsFunctionResult Execute(JsGlobal global, JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (parameters.Length > 0)
            {
                JsInstance thatResult;

                switch (parameters[0].Class)
                {
                    case ClassString: thatResult = JsString.Create(parameters[0].ToString()); break;
                    case ClassNumber: thatResult = JsNumber.Create(parameters[0].ToNumber()); break;
                    case ClassBoolean: thatResult = JsBoolean.Create(parameters[0].ToBoolean()); break;
                    default: thatResult = parameters[0]; break;
                }

                return new JsFunctionResult(null, thatResult);
            }

            return new JsFunctionResult(null, New(this));
        }

        // 15.2.4.3 and 15.2.4.4
        public static JsInstance ToStringImpl(JsInstance target, JsInstance[] parameters)
        {
            return JsString.Create(String.Concat("[object ", target.Class, "]"));
        }

        // 15.2.4.4
        public static JsInstance ValueOfImpl(JsInstance target, JsInstance[] parameters)
        {
            return target;
        }

        // 15.2.4.5
        public static JsInstance HasOwnPropertyImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            return JsBoolean.Create(target.HasOwnProperty(parameters[0]));
        }

        // 15.2.4.6
        public static JsInstance IsPrototypeOfImpl(JsInstance target, JsInstance[] parameters)
        {
            if (target.Class != ClassObject)
                return JsBoolean.False;
            if (parameters.Length == 0)
                return JsBoolean.False;

            return JsBoolean.Create(((JsObject)target).IsPrototypeOf(parameters[0] as JsDictionaryObject));
        }

        // 15.2.4.7
        public static JsInstance PropertyIsEnumerableImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            throw new NotImplementedException();
            //if (!HasOwnProperty(parameters[0]))
            //    return JsBoolean.False;

            var v = target[parameters[0]];

            return JsBoolean.Create((v.Attributes & PropertyAttributes.DontEnum) == PropertyAttributes.None);
        }

        /// <summary>
        /// 15.2.3.2
        /// </summary>
        /// <returns></returns>
        public static JsInstance GetPrototypeOfImpl(JsGlobal global, JsInstance[] parameters)
        {
            if (parameters[0].Class != ClassObject)
                throw new JsException(global.TypeErrorClass.New());

            var constructor = (parameters[0] as JsObject ?? JsUndefined.Instance)[ConstructorName] as JsObject;

            return (constructor ?? JsUndefined.Instance)[PrototypeName];
        }

        /// <summary>
        /// 15.2.3.6
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="p"></param>
        /// <param name="currentDescriptor"></param>
        public static JsInstance DefineProperty(JsGlobal global, JsInstance[] parameters)
        {
            JsInstance instance = parameters[0];

            if (!(instance is JsDictionaryObject))
                throw new JsException(global.TypeErrorClass.New());

            string name = parameters[1].ToString();
            Descriptor desc = Descriptor.ToPropertyDesciptor(global, (JsDictionaryObject)instance, name, parameters[2]);

            ((JsDictionaryObject)instance).DefineOwnProperty(desc);

            return instance;
        }
    }
}
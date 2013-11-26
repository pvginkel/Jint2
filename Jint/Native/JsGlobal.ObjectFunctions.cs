using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ObjectFunctions
        {
            public static JsInstance GetConstructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return callee;
            }

            // 15.2.2.1
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                // TODO: This looks wrong. It looks like this should be returning
                // a JsObject that has the value set to the parameter. Chrome returns
                // 'object' for typeof(new Object(7)) and typeof(Object(7)).

                if (arguments.Length > 0)
                {
                    JsInstance thatResult;

                    switch (arguments[0].Class)
                    {
                        case JsInstance.ClassString: thatResult = JsString.Create(arguments[0].ToString()); break;
                        case JsInstance.ClassNumber: thatResult = JsNumber.Create(arguments[0].ToNumber()); break;
                        case JsInstance.ClassBoolean: thatResult = JsBoolean.Create(arguments[0].ToBoolean()); break;
                        default: thatResult = arguments[0]; break;
                    }

                    return thatResult;
                }

                JsObject obj = runtime.Global.CreateObject(callee.Prototype);

                obj.DefineOwnProperty(new ValueDescriptor(obj, JsNames.Constructor, callee, PropertyAttributes.DontEnum));

                return obj;
            }

            // 15.2.4.3 and 15.2.4.4
            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create("[object " + @this.Class + "]");
            }

            // 15.2.4.4
            public static JsInstance ValueOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                // TODO: This looks wrong and it looks like the Value should be returned
                // here. E.g. typeof(new Object(7).valueOf()) returns 'number' in Chrome.

                return @this;
            }

            // 15.2.4.5
            public static JsInstance HasOwnProperty(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                return JsBoolean.Create(target.HasOwnProperty(arguments[0]));
            }

            // 15.2.4.6
            public static JsInstance IsPrototypeOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target.Class != JsInstance.ClassObject)
                    return JsBoolean.False;
                if (arguments.Length == 0)
                    return JsBoolean.False;

                return JsBoolean.Create(((JsObject)target).IsPrototypeOf(arguments[0] as JsObject));
            }

            // 15.2.4.7
            public static JsInstance PropertyIsEnumerable(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                throw new NotImplementedException();
                /*
                if (!HasOwnProperty(arguments[0]))
                    return JsBoolean.False;

                var v = target[arguments[0]];

                return JsBoolean.Create((v.Attributes & PropertyAttributes.DontEnum) == PropertyAttributes.None);
                 * */
            }

            // 15.2.3.2
            public static JsInstance GetPrototypeOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments[0].Class != JsInstance.ClassObject)
                    throw new JsException(JsErrorType.TypeError);

                var jsObject = arguments[0] as JsObject;
                if (jsObject != null)
                {
                    var constructor = jsObject[JsNames.Constructor] as JsObject;
                    if (constructor != null)
                        return constructor[JsNames.Prototype];
                }

                return JsNull.Instance;
            }

            // 15.2.3.6
            public static JsInstance DefineProperty(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsInstance instance = arguments[0];

                if (!(instance is JsObject))
                    throw new JsException(JsErrorType.TypeError);

                string name = arguments[1].ToString();
                var desc = Descriptor.ToPropertyDescriptor(runtime.Global, (JsObject)instance, name, arguments[2]);

                ((JsObject)instance).DefineOwnProperty(desc);

                return instance;
            }

            public static JsInstance LookupGetter(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length == 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(arguments[0].ToString()))
                    return ((JsFunction)target.Prototype["__lookupGetter__"]).Execute(runtime, target.Prototype, arguments, null);

                var descriptor = target.GetOwnDescriptor(arguments[0].ToSource()) as PropertyDescriptor;
                if (descriptor == null)
                    return JsUndefined.Instance;

                return (JsInstance)descriptor.GetFunction ?? JsUndefined.Instance;
            }

            public static JsInstance LookupSetter(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length <= 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(arguments[0].ToString()))
                    return ((JsFunction)target.Prototype["__lookupSetter__"]).Execute(runtime, target.Prototype, arguments, null);

                var descriptor = target.GetOwnDescriptor(arguments[0].ToSource()) as PropertyDescriptor;
                if (descriptor == null)
                    return JsUndefined.Instance;

                return (JsInstance)descriptor.SetFunction ?? JsUndefined.Instance;
            }
        }
    }
}

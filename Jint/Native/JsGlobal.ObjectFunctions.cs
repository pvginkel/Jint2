using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ObjectFunctions
        {
            // 15.2.2.1
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                // TODO: This looks wrong. It looks like this should be returning
                // a JsObject that has the value set to the parameter. Chrome returns
                // 'object' for typeof(new Object(7)) and typeof(Object(7)).

                if (arguments.Length > 0)
                {
                    var argument = arguments[0];

                    if (!argument.IsLiteral)
                        return argument;

                    var global = runtime.Global;
                    JsObject result;

                    if (argument.IsString)
                        result = global.CreateObject(argument.ToString(), global.StringClass);
                    else if (argument.IsNull)
                        result = global.CreateObject(argument.ToNumber(), global.NumberClass);
                    else if (argument.IsBoolean)
                        result = global.CreateObject(argument.ToBoolean(), global.BooleanClass);
                    else
                        throw new InvalidOperationException();

                    return JsBox.CreateObject(result);
                }

                var obj = runtime.Global.CreateObject(callee.Prototype);

                obj.DefineOwnProperty(new ValueDescriptor(
                    obj,
                    "constructor",
                    JsBox.CreateObject(callee),
                    PropertyAttributes.DontEnum
                ));

                return JsBox.CreateObject(obj);
            }

            // 15.2.4.3 and 15.2.4.4
            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box("[object " + @this.GetClass() + "]");
            }

            // 15.2.4.4
            public static JsBox ValueOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                // TODO: This looks wrong and it looks like the Value should be returned
                // here. E.g. typeof(new Object(7).valueOf()) returns 'number' in Chrome.

                return @this;
            }

            // 15.2.4.5
            public static JsBox HasOwnProperty(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                return JsBoolean.Box(target.HasOwnProperty(arguments[0]));
            }

            // 15.2.4.6
            public static JsBox IsPrototypeOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (
                    target.Class != JsNames.ClassObject ||
                    arguments.Length == 0 ||
                    !arguments[0].IsObject
                )
                    return JsBox.False;

                return JsBoolean.Box(target.IsPrototypeOf((JsObject)arguments[0]));
            }

            // 15.2.4.7
            public static JsBox PropertyIsEnumerable(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                throw new NotImplementedException();
                /*
                if (!HasOwnProperty(arguments[0]))
                    return JsBoolean.False;

                var v = target[arguments[0]];

                return JsBoolean.Box((v.Attributes & PropertyAttributes.DontEnum) == PropertyAttributes.None);
                 * */
            }

            // 15.2.3.2
            public static JsBox GetPrototypeOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments[0].GetClass() != JsNames.ClassObject)
                    throw new JsException(JsErrorType.TypeError);

                if (arguments[0].IsObject)
                {
                    var constructor = ((JsObject)arguments[0]).GetProperty(Id.constructor);
                    if (constructor.IsObject)
                        return ((JsObject)constructor).GetProperty(Id.prototype);
                }

                return JsBox.Null;
            }

            // 15.2.3.6
            public static JsBox DefineProperty(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var instance = arguments[0];
                if (!instance.IsObject)
                    throw new JsException(JsErrorType.TypeError);

                string name = arguments[1].ToString();
                var desc = Descriptor.ToPropertyDescriptor(
                    runtime.Global,
                    (JsObject)instance,
                    name,
                    arguments[2]
                );

                ((JsObject)instance).DefineOwnProperty(desc);

                return instance;
            }

            public static JsBox LookupGetter(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length == 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(runtime.Global.ResolveIdentifier(arguments[0].ToString())))
                {
                    return ((JsObject)target.Prototype.GetProperty(Id.__lookupGetter__)).Execute(
                        runtime,
                        JsBox.CreateObject(target.Prototype),
                        arguments,
                        null
                    );
                }

                var descriptor = target.GetOwnDescriptor(runtime.Global.ResolveIdentifier(arguments[0].ToString())) as PropertyDescriptor;
                if (descriptor == null)
                    return JsBox.Undefined;

                if (descriptor.GetFunction != null)
                    return JsBox.CreateObject(descriptor.GetFunction);

                return JsBox.Undefined;
            }

            public static JsBox LookupSetter(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length <= 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(runtime.Global.ResolveIdentifier(arguments[0].ToString())))
                {
                    return ((JsObject)target.Prototype.GetProperty(Id.__lookupSetter__)).Execute(
                        runtime,
                        JsBox.CreateObject(target.Prototype),
                        arguments,
                        null
                    );
                }

                var descriptor = target.GetOwnDescriptor(runtime.Global.ResolveIdentifier(arguments[0].ToString())) as PropertyDescriptor;
                if (descriptor == null)
                    return JsBox.Undefined;

                if (descriptor.SetFunction != null)
                    return JsBox.CreateObject(descriptor.SetFunction);

                return JsBox.Undefined;
            }
        }
    }
}

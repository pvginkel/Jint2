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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                // TODO: This looks wrong. It looks like this should be returning
                // a JsObject that has the value set to the parameter. Chrome returns
                // 'object' for typeof(new Object(7)) and typeof(Object(7)).

                if (arguments.Length > 0)
                {
                    var argument = arguments[0];

                    var global = runtime.Global;
                    JsObject result;

                    if (argument is string)
                        result = global.CreateObject(argument, global.StringClass);
                    else if (argument is double)
                        result = global.CreateObject((double)argument, global.NumberClass);
                    else if (argument is bool)
                        result = global.CreateObject(argument, global.BooleanClass);
                    else
                        return argument;

                    return result;
                }

                var obj = runtime.Global.CreateObject(callee.Prototype);

                obj.DefineProperty(
                    Id.constructor,
                    callee,
                    PropertyAttributes.DontEnum
                );

                return obj;
            }

            // 15.2.4.3 and 15.2.4.4
            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                return "[object " + JsValue.GetClass(@this) + "]";
            }

            // 15.2.4.4
            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                // TODO: This looks wrong and it looks like the Value should be returned
                // here. E.g. typeof(new Object(7).valueOf()) returns 'number' in Chrome.

                return @this;
            }

            // 15.2.4.5
            public static object HasOwnProperty(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var target = (JsObject)@this;
                return BooleanBoxes.Box(target.HasOwnProperty(arguments[0]));
            }

            // 15.2.4.6
            public static object IsPrototypeOf(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var target = (JsObject)@this;
                if (
                    target.Class != JsNames.ClassObject ||
                    arguments.Length == 0 ||
                    !(arguments[0] is JsObject)
                )
                    return BooleanBoxes.False;

                return BooleanBoxes.Box(target.IsPrototypeOf((JsObject)arguments[0]));
            }

            // 15.2.4.7
            public static object PropertyIsEnumerable(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
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
            public static object GetPrototypeOf(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var @object = arguments[0] as JsObject;
                if (@object == null)
                    throw new JsException(JsErrorType.TypeError);

                var constructor = @object.GetProperty(Id.constructor) as JsObject;
                if (constructor != null)
                    return constructor.GetProperty(Id.prototype);

                return JsNull.Instance;
            }

            // 15.2.3.6
            public static object DefineProperty(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var instance = arguments[0] as JsObject;
                if (instance == null)
                    throw new JsException(JsErrorType.TypeError);

                ToPropertyDescriptor(
                    runtime.Global,
                    (JsObject)instance,
                    JsValue.ToString(arguments[1]),
                    arguments[2]
                );

                return instance;
            }

            /// <summary>
            /// 8.10.5
            /// </summary>
            private static void ToPropertyDescriptor(JsGlobal global, JsObject owner, string name, object value)
            {
                var @object = value as JsObject;
                if (@object == null)
                    throw new JsException(JsErrorType.TypeError, "The target object has to be an instance of an object");

                if (
                    (@object.HasProperty(Id.value) || @object.HasProperty(Id.writable)) &&
                    (@object.HasProperty(Id.set) || @object.HasProperty(Id.get))
                )
                    throw new JsException(JsErrorType.TypeError, "The property cannot be both writable and have get/set accessors or cannot have both a value and an accessor defined");

                var attributes = PropertyAttributes.None;
                JsObject getFunction = null;
                JsObject setFunction = null;
                object result;

                if (
                    @object.TryGetProperty(Id.enumerable, out result) &&
                    !JsValue.ToBoolean(result)
                )
                    attributes |= PropertyAttributes.DontEnum;

                if (
                    @object.TryGetProperty(Id.configurable, out result) &&
                    !JsValue.ToBoolean(result)
                )
                    attributes |= PropertyAttributes.DontDelete;

                if (
                    @object.TryGetProperty(Id.writable, out result) &&
                    !JsValue.ToBoolean(result)
                )
                    attributes |= PropertyAttributes.ReadOnly;

                if (@object.TryGetProperty(Id.get, out result))
                {
                    if (!JsValue.IsFunction(result))
                        throw new JsException(JsErrorType.TypeError, "The getter has to be a function");

                    getFunction = (JsObject)result;
                }

                if (@object.TryGetProperty(Id.set, out result))
                {
                    if (!JsValue.IsFunction(result))
                        throw new JsException(JsErrorType.TypeError, "The setter has to be a function");

                    setFunction = (JsObject)result;
                }

                if (@object.HasProperty(Id.value))
                {
                    owner.DefineProperty(
                        global.ResolveIdentifier(name),
                        @object.GetProperty(Id.value),
                        PropertyAttributes.None
                    );
                }
                else
                {
                    owner.DefineAccessor(
                        global.ResolveIdentifier(name),
                        getFunction,
                        setFunction,
                        attributes
                    );
                }
            }

            public static object LookupGetter(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length == 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(runtime.Global.ResolveIdentifier(JsValue.ToString(arguments[0]))))
                {
                    return ((JsObject)target.Prototype.GetProperty(Id.__lookupGetter__)).Execute(
                        runtime,
                        target.Prototype,
                        arguments
                    );
                }

                var accessor = target.GetOwnPropertyRaw(runtime.Global.ResolveIdentifier(JsValue.ToString(arguments[0]))) as PropertyAccessor;
                if (accessor == null)
                    return JsUndefined.Instance;

                var getter = accessor.Getter;
                if (getter != null)
                    return getter;

                return JsUndefined.Instance;
            }

            public static object LookupSetter(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
            {
                var target = (JsObject)@this;
                if (arguments.Length <= 0)
                    throw new ArgumentException("propertyName");

                if (!target.HasOwnProperty(runtime.Global.ResolveIdentifier(JsValue.ToString(arguments[0]))))
                {
                    return ((JsObject)target.Prototype.GetProperty(Id.__lookupSetter__)).Execute(
                        runtime,
                        target.Prototype,
                        arguments
                    );
                }

                var accessor = target.GetOwnPropertyRaw(runtime.Global.ResolveIdentifier(JsValue.ToString(arguments[0]))) as PropertyAccessor;
                if (accessor == null)
                    return JsUndefined.Instance;

                var setter = accessor.Setter;
                if (setter != null)
                    return setter;

                return JsUndefined.Instance;
            }
        }
    }
}

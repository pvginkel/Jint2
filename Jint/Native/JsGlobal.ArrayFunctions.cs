using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ArrayFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == null || @this == runtime.Global.GlobalScope)
                {
                    var result = runtime.Global.CreateArray();

                    if (arguments != null)
                    {
                        var propertyStore = (ArrayPropertyStore)result.PropertyStore;

                        for (int i = 0; i < arguments.Length; i++)
                        {
                            propertyStore.SetByIndex(i, arguments[i]); // Fast version since it avoids a type conversion
                        }
                    }

                    return result;
                }

                // When called as part of a new expression, it is a constructor: it initialises the newly created object.

                var target = (JsObject)@this;
                for (int i = 0; i < arguments.Length; i++)
                {
                    target[i.ToString()] = arguments[i];
                }

                return @this;
            }

            // 15.4.4.2
            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsFunction)runtime.Global.ArrayClass["join"]).Execute(runtime, @this, JsInstance.EmptyArray, null);
            }

            // 15.4.4.3
            public static JsInstance ToLocaleString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var global = runtime.Global;
                var array = (JsArray)@this;
                var result = global.CreateArray();

                for (int i = 0; i < array.Length; i++)
                {
                    var obj = (JsObject)array[i.ToString()];
                    result[i.ToString()] = ((JsFunction)obj["toLocaleString"]).Execute(runtime, obj, arguments, null);
                }

                return JsString.Create(result.ToString());
            }

            // 15.4.4.4
            public static JsInstance Concat(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var array = @this as JsArray;
                if (array != null)
                    return ((ArrayPropertyStore)array.PropertyStore).Concat(arguments);

                var global = runtime.Global;
                array = global.CreateArray();
                var propertyStore = (ArrayPropertyStore)array.PropertyStore;
                var items = new List<JsInstance>
                {
                    @this
                };
                items.AddRange(arguments);
                int n = 0;
                while (items.Count > 0)
                {
                    JsInstance e = items[0];
                    items.RemoveAt(0);
                    if (global.ArrayClass.HasInstance(e as JsObject))
                    {
                        for (int k = 0; k < ((JsObject)e).Length; k++)
                        {
                            int p = k;
                            JsInstance result;
                            if (((JsObject)e).TryGetProperty(p, out result))
                                propertyStore.SetByIndex(n, result);
                            n++;
                        }
                    }
                    else
                    {
                        propertyStore.SetByIndex(n, e);
                        n++;
                    }
                }

                return array;
            }

            // 15.4.4.5
            public static JsInstance Join(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var array = @this as JsArray;
                if (array != null)
                    return ((ArrayPropertyStore)array.PropertyStore).Join(arguments.Length > 0 ? arguments[0] : JsUndefined.Instance);

                string separator =
                    arguments.Length == 0 || JsInstance.IsUndefined(arguments[0])
                    ? ","
                    : arguments[0].ToString();

                var jsObject = @this as JsObject;

                if (jsObject == null || jsObject.Length == 0)
                    return JsString.Create();

                JsInstance firstElement = jsObject[0.ToString()];

                StringBuilder result;
                if (JsInstance.IsNullOrUndefined(firstElement))
                    result = new StringBuilder(string.Empty);
                else
                    result = new StringBuilder(firstElement.ToString());

                var length = jsObject["length"].ToNumber();

                for (int i = 1; i < length; i++)
                {
                    result.Append(separator);
                    JsInstance element = jsObject[i.ToString()];
                    if (!JsInstance.IsNullOrUndefined(element))
                        result.Append(element);
                }
                return JsString.Create(result.ToString());
            }

            // 15.4.4.6
            public static JsInstance Pop(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsObject = @this as JsObject;
                if (jsObject == null || jsObject.Length <= 0)
                    return JsUndefined.Instance;
                var key = (jsObject.Length - 1).ToString();
                var result = jsObject[key];
                jsObject.Delete(key);
                jsObject.Length--;
                return result;
            }

            // 15.4.4.7
            public static JsInstance Push(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var obj = (JsObject)@this;

                int length = (int)obj["length"].ToNumber();
                foreach (var arg in arguments)
                {
                    obj[JsNumber.Create(length)] = arg;
                    length++;
                }

                return JsNumber.Create(length);
            }

            // 15.4.4.8
            public static JsInstance Reverse(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                int len = target.Length;
                int middle = len / 2;

                for (int lower = 0; lower != middle; lower++)
                {
                    int upper = len - lower - 1;

                    JsInstance lowerValue;
                    bool lowerExists = target.TryGetProperty(lower, out lowerValue);
                    JsInstance upperValue;
                    bool upperExists = target.TryGetProperty(upper, out upperValue);

                    if (lowerExists)
                        target.SetProperty(lower, lowerValue);
                    else
                        target.Delete(upper);

                    if (upperExists)
                        target.SetProperty(lower, upperValue);
                    else
                        target.Delete(lower);
                }

                return target;
            }

            // 15.4.4.9
            public static JsInstance Shift(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsObject = @this as JsObject;
                if (jsObject == null || jsObject.Length == 0)
                    return JsUndefined.Instance;

                JsInstance first = jsObject[0.ToString()];
                for (int k = 1; k < jsObject.Length; k++)
                {
                    int from = k;
                    int to = k - 1;

                    JsInstance result;
                    if (jsObject.TryGetProperty(from, out result))
                        jsObject.SetProperty(to, result);
                    else
                        jsObject.Delete(to);
                }
                jsObject.Delete((jsObject.Length - 1).ToString());
                jsObject.Length--;

                return first;
            }

            // 15.4.4.10
            public static JsInstance Slice(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var global = runtime.Global;
                var target = (JsObject)@this;
                var start = arguments.Length > 0 ? (int)arguments[0].ToNumber() : 0;
                var end = arguments.Length > 1 ? (int)arguments[1].ToNumber() : target.Length;

                if (start < 0)
                    start += target.Length;
                if (end < 0)
                    end += target.Length;
                if (start > target.Length)
                    start = target.Length;
                if (end > target.Length)
                    end = target.Length;
                var result = global.CreateArray();
                var push = (JsFunction)result["push"];
                for (int i = start; i < end; i++)
                {
                    push.Execute(
                        runtime,
                        result,
                        new[] { target[JsNumber.Create(i)] },
                        null
                    );
                }

                return result;
            }

            // 15.4.4.11
            public static JsInstance Sort(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsObject = @this as JsObject;
                if (jsObject == null || jsObject.Length <= 1)
                    return @this;

                JsFunction compare = null;

                // Compare function defined
                if (arguments.Length > 0)
                {
                    compare = arguments[0] as JsFunction;
                }

                var values = new List<JsInstance>();
                var length = (int)jsObject["length"].ToNumber();

                for (int i = 0; i < length; i++)
                {
                    values.Add(jsObject[i.ToString()]);
                }

                if (compare != null)
                {
                    try
                    {
                        values.Sort(new JsComparer(runtime.Global.Backend, compare));
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException is JsException)
                            throw e.InnerException;

                        throw;
                    }
                }
                else
                {
                    values.Sort();
                }

                for (int i = 0; i < length; i++)
                {
                    jsObject[i.ToString()] = values[i];
                }

                return jsObject;
            }

            // 15.4.4.12
            public static JsInstance Splice(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                var global = target.Global;
                var array = global.CreateArray();
                var propertyStore = (ArrayPropertyStore)array.PropertyStore;
                int relativeStart = Convert.ToInt32(arguments[0].ToNumber());
                int actualStart = relativeStart < 0 ? Math.Max(target.Length + relativeStart, 0) : Math.Min(relativeStart, target.Length);
                int actualDeleteCount = Math.Min(Math.Max(Convert.ToInt32(arguments[1].ToNumber()), 0), target.Length - actualStart);
                int len = target.Length;

                for (int k = 0; k < actualDeleteCount; k++)
                {
                    int from = relativeStart + k;
                    JsInstance result;
                    if (target.TryGetProperty(from, out result))
                        propertyStore.SetByIndex(k, result);
                }

                List<JsInstance> items = new List<JsInstance>();
                items.AddRange(arguments);
                items.RemoveAt(0);
                items.RemoveAt(0);

                // use non-distructional copy, determine direction
                if (items.Count < actualDeleteCount)
                {
                    for (int k = actualStart; k < len - actualDeleteCount; k++)
                    {
                        int from = k + actualDeleteCount;
                        int to = k + items.Count;
                        JsInstance result;
                        if (target.TryGetProperty(from, out result))
                            target.SetProperty(to, result);
                        else
                            target.Delete(to);
                    }

                    for (int k = target.Length; k > len - actualDeleteCount + items.Count; k--)
                    {
                        target.Delete((k - 1).ToString());
                    }

                    target.Length = len - actualDeleteCount + items.Count;
                }
                else
                {
                    for (int k = len - actualDeleteCount; k > actualStart; k--)
                    {
                        int from = k + actualDeleteCount - 1;
                        int to = k + items.Count - 1;
                        JsInstance result;
                        if (target.TryGetProperty(from, out result))
                            target.SetProperty(to, result);
                        else
                            target.Delete(to);
                    }
                }

                for (int k = 0; k < items.Count; k++)
                {
                    target.SetProperty(k, items[k]);
                }

                return array;
            }

            // 15.4.4.13
            public static JsInstance UnShift(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                for (int k = target.Length; k > 0; k--)
                {
                    int from = k - 1;
                    int to = k + arguments.Length - 1;
                    JsInstance result;
                    if (target.TryGetProperty(from, out result))
                        target.SetProperty(to, result);
                    else
                        target.Delete(to);
                }

                var items = new List<JsInstance>(arguments);
                for (int j = 0; items.Count > 0; j++)
                {
                    JsInstance e = items[0];
                    items.RemoveAt(0);
                    target.SetProperty(j, e);
                }

                return JsNumber.Create(target.Length);
            }

            // 15.4.4.15
            public static JsInstance IndexOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Create(-1);

                var target = (JsObject)@this;
                int len = (int)target["length"].ToNumber();
                if (len == 0)
                    return JsNumber.Create(-1);
                int n = 0;
                if (arguments.Length > 1)
                    n = Convert.ToInt32(arguments[1].ToNumber());
                int k;
                if (n >= len)
                    return JsNumber.Create(-1);

                JsInstance searchParameter = arguments[0];
                if (n >= 0)
                    k = n;
                else
                    k = len - Math.Abs(n);
                while (k < len)
                {
                    JsInstance result;
                    if (
                        target.TryGetProperty(k, out result) &&
                        JsInstance.StrictlyEquals(result, searchParameter)
                    )
                        return JsNumber.Create(k);
                    k++;
                }
                return JsNumber.Create(-1);
            }

            // 15.4.4.15
            public static JsInstance LastIndexOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Create(-1);

                var target = (JsObject)@this;
                int len = target.Length;
                if (len == 0)
                    return JsNumber.Create(-1);
                int n = len;
                if (arguments.Length > 1)
                    n = Convert.ToInt32(arguments[1].ToNumber());
                int k;
                JsInstance searchParameter = arguments[0];
                if (n >= 0)
                    k = Math.Min(n, len - 1);
                else
                    k = len - Math.Abs(n - 1);
                while (k >= 0)
                {
                    JsInstance result;
                    if (target.TryGetProperty(k, out result))
                    {
                        if (result == searchParameter)
                        {
                            return JsNumber.Create(k);
                        }
                    }
                    k--;
                }
                return JsNumber.Create(-1);
            }

            public static JsInstance GetLength(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(((JsObject)@this).Length);
            }

            public static JsInstance SetLength(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target is JsArray)
                {
                    target.Length = (int)arguments[0].ToNumber();
                }
                else
                {
                    int oldLen = target.Length;
                    target.Length = (int)arguments[0].ToNumber();

                    for (int i = target.Length; i < oldLen; i++)
                        target.Delete(JsNumber.Create(i));
                }

                return arguments[0];
            }
        }
    }
}

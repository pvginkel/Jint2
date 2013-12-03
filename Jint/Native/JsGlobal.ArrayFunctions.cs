using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ArrayFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsObject target;

                if (@this == null || @this == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);
                else
                    target = (JsObject)@this;

                target.SetClass(JsNames.ClassArray);
                target.SetIsClr(false);

                var propertyStore = new ArrayPropertyStore(target);
                target.PropertyStore = propertyStore;

                for (int i = 0; i < arguments.Length; i++)
                {
                    propertyStore[i] = arguments[i]; // Fast version since it avoids a type conversion
                }

                return target;
            }

            // 15.4.4.2
            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsObject)runtime.Global.ArrayClass.GetProperty(Id.join)).Execute(runtime, @this, JsInstance.EmptyArray, null);
            }

            // 15.4.4.3
            public static JsInstance ToLocaleString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var global = runtime.Global;
                var store = @this.FindArrayStore();
                var result = global.CreateArray();
                var resultStore = result.FindArrayStore();

                for (int i = 0; i < store.Length; i++)
                {
                    var obj = (JsObject)store[i];
                    resultStore[i] = ((JsObject)obj.GetProperty(Id.toLocaleString)).Execute(runtime, obj, arguments, null);
                }

                return JsString.Create(result.ToString());
            }

            // 15.4.4.4
            public static JsInstance Concat(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                {
                    var array = runtime.Global.CreateArray();
                    store = array.FindArrayStore();
                    store[0] = @this;
                }

                return store.Concat(arguments);
            }

            // 15.4.4.5
            public static JsInstance Join(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                return store.Join(arguments.Length > 0 ? arguments[0] : JsUndefined.Instance);
            }

            // 15.4.4.6
            public static JsInstance Pop(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                var key = store.Length - 1;
                var result = store[key];

                store.Delete(key);
                store.Length--;

                return result;
            }

            // 15.4.4.7
            public static JsInstance Push(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var store = @this.FindArrayStore();

                if (store != null)
                {
                    foreach (var arg in arguments)
                    {
                        store[store.Length] = arg;
                    }

                    return JsNumber.Create(store.Length);
                }
                else
                {
                    var obj = (JsObject)@this;

                    int length = (int)obj.GetProperty(Id.length).ToNumber();

                    foreach (var arg in arguments)
                    {
                        obj[JsNumber.Create(length)] = arg;
                        length++;
                    }

                    return JsNumber.Create(length);
                }
            }

            // 15.4.4.8
            public static JsInstance Reverse(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                int middle = length / 2;

                for (int lower = 0; lower != middle; lower++)
                {
                    int upper = length - lower - 1;

                    JsInstance lowerValue;
                    bool lowerExists = target.TryGetProperty(lower, out lowerValue);
                    JsInstance upperValue;
                    bool upperExists = target.TryGetProperty(upper, out upperValue);

                    if (lowerExists)
                        target.SetProperty(upper, lowerValue);
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
            public static JsInstance Shift(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                var first = store[0];
                for (int k = 1; k < store.Length; k++)
                {
                    int from = k;
                    int to = k - 1;

                    JsInstance result;
                    if (store.TryGetProperty(from, out result))
                        store[to] = result;
                    else
                        store.Delete(to);
                }

                store.Delete(store.Length - 1);
                store.Length--;

                return first;
            }

            // 15.4.4.10
            public static JsInstance Slice(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var global = runtime.Global;
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();

                var start = arguments.Length > 0 ? (int)arguments[0].ToNumber() : 0;
                var end = arguments.Length > 1 ? (int)arguments[1].ToNumber() : length;

                if (start < 0)
                    start += length;
                if (end < 0)
                    end += length;
                if (start > length)
                    start = length;
                if (end > length)
                    end = length;

                var result = global.CreateArray();
                var push = (JsObject)result.GetProperty(Id.push);

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
            public static JsInstance Sort(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = @this as JsObject;
                int length = 0;
                if (target != null)
                    length = (int)target.GetProperty(Id.length).ToNumber();
                if (length <= 1)
                    return @this;

                JsObject compare = null;

                // Compare function defined
                if (arguments.Length > 0)
                    compare = arguments[0] as JsObject;

                var values = new List<JsInstance>();

                for (int i = 0; i < length; i++)
                {
                    values.Add(target.GetProperty(i));
                }

                if (compare != null)
                {
                    try
                    {
                        values.Sort(new JsComparer(runtime.Global.Engine, compare));
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
                    target.SetProperty(i, values[i]);
                }

                return target;
            }

            // 15.4.4.12
            public static JsInstance Splice(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                var global = target.Global;

                var result = global.CreateArray();
                var resultStore = (ArrayPropertyStore)result.PropertyStore;

                int relativeStart = (int)arguments[0].ToNumber();

                int actualStart =
                    relativeStart < 0
                    ? Math.Max(length + relativeStart, 0)
                    : Math.Min(relativeStart, length);

                int actualDeleteCount =
                    Math.Min(
                        Math.Max(
                            (int)arguments[1].ToNumber(),
                            0
                        ),
                        length - actualStart
                    );

                for (int k = 0; k < actualDeleteCount; k++)
                {
                    int from = relativeStart + k;
                    JsInstance item;
                    if (target.TryGetProperty(from, out item))
                        resultStore[k] = item;
                }

                var items = new List<JsInstance>();

                items.AddRange(arguments);
                items.RemoveAt(0);
                items.RemoveAt(0);

                // use non-distructional copy, determine direction
                if (items.Count < actualDeleteCount)
                {
                    for (int k = actualStart; k < length - actualDeleteCount; k++)
                    {
                        int from = k + actualDeleteCount;
                        int to = k + items.Count;
                        JsInstance item;
                        if (target.TryGetProperty(from, out item))
                            target.SetProperty(to, item);
                        else
                            target.Delete(to);
                    }

                    for (int k = length; k > length - actualDeleteCount + items.Count; k--)
                    {
                        target.Delete((k - 1).ToString());
                    }

                    target.SetProperty(Id.length, JsNumber.Create(length - actualDeleteCount + items.Count));
                }
                else
                {
                    for (int k = length - actualDeleteCount; k > actualStart; k--)
                    {
                        int from = k + actualDeleteCount - 1;
                        int to = k + items.Count - 1;
                        JsInstance item;
                        if (target.TryGetProperty(from, out item))
                            target.SetProperty(to, item);
                        else
                            target.Delete(to);
                    }
                }

                for (int k = 0; k < items.Count; k++)
                {
                    target.SetProperty(k, items[k]);
                }

                return result;
            }

            // 15.4.4.13
            public static JsInstance UnShift(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();

                for (int k = length; k > 0; k--)
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

                return target.GetProperty(Id.length);
            }

            // 15.4.4.15
            public static JsInstance IndexOf(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Create(-1);

                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                if (length == 0)
                    return JsNumber.Create(-1);

                int n = 0;
                if (arguments.Length > 1)
                    n = Convert.ToInt32(arguments[1].ToNumber());

                if (n >= length)
                    return JsNumber.Create(-1);

                var searchParameter = arguments[0];

                int k;
                if (n >= 0)
                    k = n;
                else
                    k = length - Math.Abs(n);

                while (k < length)
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
            public static JsInstance LastIndexOf(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Create(-1);

                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                if (length == 0)
                    return JsNumber.Create(-1);

                int n = length;
                if (arguments.Length > 1)
                    n = (int)arguments[1].ToNumber();

                int k;
                var searchParameter = arguments[0];
                if (n >= 0)
                    k = Math.Min(n, length - 1);
                else
                    k = length - Math.Abs(n - 1);

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

            public static JsInstance GetLength(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(@this.FindArrayStore().Length);
            }

            public static JsInstance SetLength(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                var store = target.FindArrayStore();

                if (store != null)
                {
                    store.Length = (int)arguments[0].ToNumber();
                }
                else
                {
                    int oldLen = (int)target.GetProperty(Id.length).ToNumber();
                    int newLength = (int)arguments[0].ToNumber();

                    target.SetProperty(Id.length, JsNumber.Create(newLength));

                    for (int i = newLength; i < oldLen; i++)
                    {
                        target.Delete(JsNumber.Create(i));
                    }
                }

                return arguments[0];
            }
        }
    }
}

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
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                target.SetClass(JsNames.ClassArray);
                target.SetIsClr(false);

                var propertyStore = new ArrayPropertyStore(target);
                target.PropertyStore = propertyStore;

                for (int i = 0; i < arguments.Length; i++)
                {
                    propertyStore[i] = arguments[i]; // Fast version since it avoids a type conversion
                }

                return JsBox.CreateObject(target);
            }

            // 15.4.4.2
            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return ((JsObject)runtime.Global.ArrayClass.GetProperty(Id.join)).Execute(runtime, @this, JsBox.EmptyArray, null);
            }

            // 15.4.4.3
            public static JsBox ToLocaleString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var global = runtime.Global;
                var store = @this.FindArrayStore();
                var result = global.CreateArray();
                var resultStore = result.FindArrayStore();

                for (int i = 0; i < store.Length; i++)
                {
                    var obj = (JsObject)store[i];
                    resultStore[i] = ((JsObject)obj.GetProperty(Id.toLocaleString)).Execute(
                        runtime, JsBox.CreateObject(obj), arguments, null
                    );
                }

                return JsString.Box(result.ToString());
            }

            // 15.4.4.4
            public static JsBox Concat(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                {
                    var array = runtime.Global.CreateArray();
                    store = array.FindArrayStore();
                    store[0] = @this;
                }

                return JsBox.CreateObject(store.Concat(arguments));
            }

            // 15.4.4.5
            public static JsBox Join(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsBox.Undefined;

                return store.Join(arguments.Length > 0 ? arguments[0] : JsBox.Undefined);
            }

            // 15.4.4.6
            public static JsBox Pop(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsBox.Undefined;

                var key = store.Length - 1;
                var result = store[key];

                store.Delete(key);
                store.Length--;

                return result;
            }

            // 15.4.4.7
            public static JsBox Push(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var store = @this.FindArrayStore();

                if (store != null)
                {
                    foreach (var arg in arguments)
                    {
                        store[store.Length] = arg;
                    }

                    return JsNumber.Box(store.Length);
                }
                else
                {
                    var obj = (JsObject)@this;

                    int length = (int)obj.GetProperty(Id.length).ToNumber();

                    foreach (var arg in arguments)
                    {
                        obj[JsNumber.Box(length)] = arg;
                        length++;
                    }

                    return JsNumber.Box(length);
                }
            }

            // 15.4.4.8
            public static JsBox Reverse(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                int middle = length / 2;

                for (int lower = 0; lower != middle; lower++)
                {
                    int upper = length - lower - 1;

                    JsBox lowerValue;
                    bool lowerExists = target.TryGetProperty(lower, out lowerValue);
                    JsBox upperValue;
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

                return JsBox.CreateObject(target);
            }

            // 15.4.4.9
            public static JsBox Shift(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsBox.Undefined;

                var first = store[0];
                for (int k = 1; k < store.Length; k++)
                {
                    int from = k;
                    int to = k - 1;

                    JsBox result;
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
            public static JsBox Slice(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
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
                var boxedResult = JsBox.CreateObject(result);

                for (int i = start; i < end; i++)
                {
                    push.Execute(
                        runtime,
                        boxedResult,
                        new[] { target[JsNumber.Box(i)] },
                        null
                    );
                }

                return boxedResult;
            }

            // 15.4.4.11
            public static JsBox Sort(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                int length = 0;
                if (@this.IsObject)
                    length = (int)((JsObject)@this).GetProperty(Id.length).ToNumber();
                if (length <= 1)
                    return @this;

                JsObject compare = null;
                var target = (JsObject)@this;

                // Compare function defined
                if (arguments.Length > 0 && arguments[0].IsObject)
                    compare = (JsObject)arguments[0];

                var values = new List<JsBox>();

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
                    values.Sort(JsBoxComparer.Instance);
                }

                for (int i = 0; i < length; i++)
                {
                    target.SetProperty(i, values[i]);
                }

                return JsBox.CreateObject(target);
            }

            // 15.4.4.12
            public static JsBox Splice(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
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
                    JsBox item;
                    if (target.TryGetProperty(from, out item))
                        resultStore[k] = item;
                }

                var items = new List<JsBox>();

                items.AddRange(arguments);
                items.RemoveAt(0);
                items.RemoveAt(0);

                // Use non-destructing copy, determine direction
                if (items.Count < actualDeleteCount)
                {
                    for (int k = actualStart; k < length - actualDeleteCount; k++)
                    {
                        int from = k + actualDeleteCount;
                        int to = k + items.Count;
                        JsBox item;
                        if (target.TryGetProperty(from, out item))
                            target.SetProperty(to, item);
                        else
                            target.Delete(to);
                    }

                    for (int k = length; k > length - actualDeleteCount + items.Count; k--)
                    {
                        target.Delete(k - 1);
                    }

                    target.SetProperty(Id.length, JsNumber.Box(length - actualDeleteCount + items.Count));
                }
                else
                {
                    for (int k = length - actualDeleteCount; k > actualStart; k--)
                    {
                        int from = k + actualDeleteCount - 1;
                        int to = k + items.Count - 1;
                        JsBox item;
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

                return JsBox.CreateObject(result);
            }

            // 15.4.4.13
            public static JsBox UnShift(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();

                for (int k = length; k > 0; k--)
                {
                    int from = k - 1;
                    int to = k + arguments.Length - 1;
                    JsBox result;
                    if (target.TryGetProperty(from, out result))
                        target.SetProperty(to, result);
                    else
                        target.Delete(to);
                }

                var items = new List<JsBox>(arguments);
                for (int j = 0; items.Count > 0; j++)
                {
                    JsBox e = items[0];
                    items.RemoveAt(0);
                    target.SetProperty(j, e);
                }

                return target.GetProperty(Id.length);
            }

            // 15.4.4.15
            public static JsBox IndexOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Box(-1);

                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                if (length == 0)
                    return JsNumber.Box(-1);

                int n = 0;
                if (arguments.Length > 1)
                    n = Convert.ToInt32(arguments[1].ToNumber());

                if (n >= length)
                    return JsNumber.Box(-1);

                var searchParameter = arguments[0];

                int k;
                if (n >= 0)
                    k = n;
                else
                    k = length - Math.Abs(n);

                while (k < length)
                {
                    JsBox result;
                    if (
                        target.TryGetProperty(k, out result) &&
                        JsBox.StrictlyEquals(result, searchParameter)
                    )
                        return JsNumber.Box(k);
                    k++;
                }

                return JsNumber.Box(-1);
            }

            // 15.4.4.15
            public static JsBox LastIndexOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsNumber.Box(-1);

                var target = (JsObject)@this;
                int length = (int)target.GetProperty(Id.length).ToNumber();
                if (length == 0)
                    return JsNumber.Box(-1);

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
                    JsBox result;
                    if (
                        target.TryGetProperty(k, out result) &&
                        JsBox.StrictlyEquals(result, searchParameter)
                    )
                        return JsNumber.Box(k);

                    k--;
                }

                return JsNumber.Box(-1);
            }

            public static JsBox GetLength(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(@this.FindArrayStore().Length);
            }

            public static JsBox SetLength(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
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

                    target.SetProperty(Id.length, JsNumber.Box(newLength));

                    for (int i = newLength; i < oldLen; i++)
                    {
                        target.Delete(JsNumber.Box(i));
                    }
                }

                return arguments[0];
            }

            private class JsBoxComparer : IComparer<JsBox>
            {
                public static readonly JsBoxComparer Instance = new JsBoxComparer();

                private JsBoxComparer()
                {
                }

                public int Compare(JsBox x, JsBox y)
                {
                    return x.ToString().CompareTo(y.ToString());
                }
            }
        }
    }
}

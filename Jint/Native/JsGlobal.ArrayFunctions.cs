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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                target.SetClass(JsNames.ClassArray);
                target.IsClr = false;

                var propertyStore = new ArrayPropertyStore(target);
                target.PropertyStore = propertyStore;

                for (int i = 0; i < arguments.Length; i++)
                {
                    propertyStore.DefineOrSetPropertyValue(i, arguments[i]);
                }

                return target;
            }

            // 15.4.4.2
            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return ((JsObject)runtime.Global.ArrayClass.GetProperty(Id.join)).Execute(runtime, @this, JsValue.EmptyArray);
            }

            // 15.4.4.3
            public static object ToLocaleString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var global = runtime.Global;
                var store = @this.FindArrayStore();
                var result = global.CreateArray();
                var resultStore = result.FindArrayStore();

                for (int i = 0; i < store.Length; i++)
                {
                    var obj = (JsObject)store.GetOwnProperty(i);

                    resultStore.DefineOrSetPropertyValue(
                        i,
                        ((JsObject)obj.GetProperty(Id.toLocaleString)).Execute(
                            runtime, obj, arguments
                        )
                    );
                }

                return result.ToString();
            }

            // 15.4.4.4
            public static object Concat(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                {
                    var array = runtime.Global.CreateArray();
                    store = array.FindArrayStore();
                    store.DefineOrSetPropertyValue(0, @this);
                }

                return store.Concat(arguments);
            }

            // 15.4.4.5
            public static object Join(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                return store.Join(arguments.Length > 0 ? arguments[0] : JsUndefined.Instance);
            }

            // 15.4.4.6
            public static object Pop(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                var key = store.Length - 1;
                var result = store.GetOwnProperty(key);

                store.DeleteProperty(key);
                store.Length--;

                return result;
            }

            // 15.4.4.7
            public static object Push(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var store = @this.FindArrayStore();

                if (store != null)
                {
                    foreach (var arg in arguments)
                    {
                        store.DefineOrSetPropertyValue(store.Length, arg);
                    }

                    return (double)store.Length;
                }
                else
                {
                    var obj = (JsObject)@this;
                    int length = (int)JsValue.ToNumber(obj.GetProperty(Id.length));

                    foreach (var arg in arguments)
                    {
                        obj.SetProperty(length, arg);
                        length++;
                    }

                    return (double)length;
                }
            }

            // 15.4.4.8
            public static object Reverse(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));
                int middle = length / 2;

                for (int lower = 0; lower != middle; lower++)
                {
                    int upper = length - lower - 1;

                    object lowerValue;
                    bool lowerExists = target.TryGetProperty(lower, out lowerValue);
                    object upperValue;
                    bool upperExists = target.TryGetProperty(upper, out upperValue);

                    if (lowerExists)
                        target.SetProperty(upper, lowerValue);
                    else
                        target.DeleteProperty(upper);

                    if (upperExists)
                        target.SetProperty(lower, upperValue);
                    else
                        target.DeleteProperty(lower);
                }

                return target;
            }

            // 15.4.4.9
            public static object Shift(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var store = @this.FindArrayStore();
                if (store == null)
                    return JsUndefined.Instance;

                var first = store.GetOwnProperty(0);
                for (int k = 1; k < store.Length; k++)
                {
                    int from = k;
                    int to = k - 1;

                    object result = store.GetOwnPropertyRaw(from);
                    if (result != null)
                    {
                        object value;

                        var accessor = result as PropertyAccessor;
                        if (accessor != null)
                            value = accessor.GetValue(@this);
                        else
                        {
                            value = result;
                        }

                        store.DefineOrSetPropertyValue(to, value);
                    }
                    else
                    {
                        store.DeleteProperty(to);
                    }
                }

                store.DeleteProperty(store.Length - 1);
                store.Length--;

                return first;
            }

            // 15.4.4.10
            public static object Slice(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var global = runtime.Global;
                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));

                var start = arguments.Length > 0 ? (int)JsValue.ToNumber(arguments[0]) : 0;
                var end = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : length;

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
                var boxedResult = (object)result;

                for (int i = start; i < end; i++)
                {
                    push.Execute(
                        runtime,
                        boxedResult,
                        new[] { target.GetProperty(i) }
                    );
                }

                return boxedResult;
            }

            // 15.4.4.11
            public static object Sort(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                int length = 0;
                var target = @this as JsObject;
                if (target != null)
                    length = (int)JsValue.ToNumber(((JsObject)@this).GetProperty(Id.length));
                if (length <= 1)
                    return @this;

                JsObject compare = null;

                // Compare function defined
                if (arguments.Length > 0)
                    compare = arguments[0] as JsObject;

                var values = new List<object>();

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
                    values.Sort(ToStringComparer.Instance);
                }

                for (int i = 0; i < length; i++)
                {
                    target.SetProperty(i, values[i]);
                }

                return target;
            }

            // 15.4.4.12
            public static object Splice(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));
                var global = target.Global;

                var result = global.CreateArray();
                var resultStore = (ArrayPropertyStore)result.PropertyStore;

                int relativeStart = (int)JsValue.ToNumber(arguments[0]);

                int actualStart =
                    relativeStart < 0
                    ? Math.Max(length + relativeStart, 0)
                    : Math.Min(relativeStart, length);

                int actualDeleteCount =
                    Math.Min(
                        Math.Max(
                            (int)JsValue.ToNumber(arguments[1]),
                            0
                        ),
                        length - actualStart
                    );

                for (int k = 0; k < actualDeleteCount; k++)
                {
                    int from = relativeStart + k;
                    object item;
                    if (target.TryGetProperty(from, out item))
                        resultStore.DefineOrSetPropertyValue(k, item);
                }

                var items = new List<object>();

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
                        object item;
                        if (target.TryGetProperty(from, out item))
                            target.SetProperty(to, item);
                        else
                            target.DeleteProperty(to);
                    }

                    for (int k = length; k > length - actualDeleteCount + items.Count; k--)
                    {
                        target.DeleteProperty(k - 1);
                    }

                    target.SetProperty(Id.length, (double)(length - actualDeleteCount + items.Count));
                }
                else
                {
                    for (int k = length - actualDeleteCount; k > actualStart; k--)
                    {
                        int from = k + actualDeleteCount - 1;
                        int to = k + items.Count - 1;
                        object item;
                        if (target.TryGetProperty(from, out item))
                            target.SetProperty(to, item);
                        else
                            target.DeleteProperty(to);
                    }
                }

                for (int k = 0; k < items.Count; k++)
                {
                    target.SetProperty(k, items[k]);
                }

                return result;
            }

            // 15.4.4.13
            public static object UnShift(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));

                for (int k = length; k > 0; k--)
                {
                    int from = k - 1;
                    int to = k + arguments.Length - 1;
                    object result;
                    if (target.TryGetProperty(from, out result))
                        target.SetProperty(to, result);
                    else
                        target.DeleteProperty(to);
                }

                var items = new List<object>(arguments);
                for (int j = 0; items.Count > 0; j++)
                {
                    object e = items[0];
                    items.RemoveAt(0);
                    target.SetProperty(j, e);
                }

                return target.GetProperty(Id.length);
            }

            // 15.4.4.15
            public static object IndexOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return (double)(-1);

                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));
                if (length == 0)
                    return (double)(-1);

                int n = 0;
                if (arguments.Length > 1)
                    n = (int)JsValue.ToNumber(arguments[1]);

                if (n >= length)
                    return (double)(-1);

                var searchParameter = arguments[0];

                int k;
                if (n >= 0)
                    k = n;
                else
                    k = length - Math.Abs(n);

                while (k < length)
                {
                    object result;
                    if (
                        target.TryGetProperty(k, out result) &&
                        JintRuntime.CompareSame(result, searchParameter)
                    )
                        return (double)k;
                    k++;
                }

                return (double)(-1);
            }

            // 15.4.4.15
            public static object LastIndexOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return (double)(-1);

                var target = (JsObject)@this;
                int length = (int)JsValue.ToNumber(target.GetProperty(Id.length));
                if (length == 0)
                    return (double)(-1);

                int n = length;
                if (arguments.Length > 1)
                    n = (int)JsValue.ToNumber(arguments[1]);

                int k;
                var searchParameter = arguments[0];
                if (n >= 0)
                    k = Math.Min(n, length - 1);
                else
                    k = length - Math.Abs(n - 1);

                while (k >= 0)
                {
                    object result;
                    if (
                        target.TryGetProperty(k, out result) &&
                        JintRuntime.CompareSame(result, searchParameter)
                    )
                        return (double)k;

                    k--;
                }

                return (double)(-1);
            }

            public static object GetLength(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)@this.FindArrayStore().Length;
            }

            public static object SetLength(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                var store = target.FindArrayStore();

                if (store != null)
                {
                    store.Length = (int)JsValue.ToNumber(arguments[0]);
                }
                else
                {
                    int oldLen = (int)JsValue.ToNumber(target.GetProperty(Id.length));
                    int newLength = (int)JsValue.ToNumber(arguments[0]);

                    target.SetProperty(Id.length, (double)newLength);

                    for (int i = newLength; i < oldLen; i++)
                    {
                        target.DeleteProperty((double)i);
                    }
                }

                return arguments[0];
            }

            private class ToStringComparer : IComparer<object>
            {
                public static readonly ToStringComparer Instance = new ToStringComparer();

                private ToStringComparer()
                {
                }

                public int Compare(object x, object y)
                {
                    return JsValue.ToString(x).CompareTo(JsValue.ToString(y));
                }
            }
        }
    }
}

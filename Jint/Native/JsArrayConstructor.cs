using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsArrayConstructor : JsConstructor
    {
        public JsArrayConstructor(JsGlobal global)
            : base(global, BuildPrototype(global))
        {
            Name = "Array";
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty(new PropertyDescriptor<JsObject>(global, prototype, "length", GetLengthImpl, SetLengthImpl) { Enumerable = false });

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsArray>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsArray>(ToLocaleStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("concat", global.FunctionClass.New<JsInstance>(Concat), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("join", global.FunctionClass.New<JsInstance>(Join, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("pop", global.FunctionClass.New<JsInstance>(Pop), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("push", global.FunctionClass.New<JsObject>(Push, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("reverse", global.FunctionClass.New<JsObject>(Reverse), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("shift", global.FunctionClass.New<JsInstance>(Shift), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("slice", global.FunctionClass.New<JsObject>(Slice, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("sort", global.FunctionClass.New<JsInstance>(Sort), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("splice", global.FunctionClass.New<JsObject>(Splice, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("unshift", global.FunctionClass.New<JsObject>(UnShift, 1), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("indexOf", global.FunctionClass.New<JsObject>(IndexOfImpl, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("lastIndexOf", global.FunctionClass.New<JsObject>(LastIndexOfImpl, 1), PropertyAttributes.DontEnum);

            return prototype;
        }


        public JsArray New()
        {
            JsArray array = new JsArray(Global, Prototype);
            //array.DefineOwnProperty("constructor", new ValueDescriptor(this) { Enumerable = false });
            return array;
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs)
        {
            JsArray array = New();

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                    array.Put(i, parameters[i]); // fast version since it avoids a type conversion
            }

            return array;
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (that == null || that == Global.GlobalScope)
            {
                var result = Construct(parameters, null);
                return new JsFunctionResult(result, result);
            }
            else
            {
                // When called as part of a new expression, it is a constructor: it initialises the newly created object.
                for (int i = 0; i < parameters.Length; i++)
                {
                    ((JsObject)that)[i.ToString()] = parameters[i];
                }

                return new JsFunctionResult(that, that);
            }
        }

        /// <summary>
        /// 15.4.4.2
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToStringImpl(JsArray target, JsInstance[] parameters)
        {
            var global = target.Global;
            var result = global.ArrayClass.New();

            for (int i = 0; i < target.Length; i++)
            {
                var obj = target[i.ToString()];
                if (IsNullOrUndefined(obj) || (obj.IsClr && obj.Value == null))
                {
                    result[i.ToString()] = JsString.Create();
                }
                else
                {
                    var jsObject = obj as JsObject;
                    if (jsObject == null)
                        jsObject = global.GetPrototype(obj);

                    var function = jsObject["toString"] as JsFunction;
                    if (function != null)
                        result[i.ToString()] = global.Backend.ExecuteFunction(function, obj, parameters, null).Result;
                    else
                        result[i.ToString()] = JsString.Create();
                }
            }

            return JsString.Create(result.ToString());

        }

        /// <summary>
        /// 15.4.4.3
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleStringImpl(JsArray target, JsInstance[] parameters)
        {
            var global = target.Global;
            var result = global.ArrayClass.New();

            for (int i = 0; i < target.Length; i++)
            {
                var obj = (JsObject)target[i.ToString()];
                result[i.ToString()] = global.Backend.ExecuteFunction((JsFunction)obj["toLocaleString"], obj, parameters, null).Result;
            }

            return JsString.Create(result.ToString());
        }

        /// <summary>
        /// 15.4.4.4
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Concat(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            if (target is JsArray)
                return ((JsArray)target).Concat(parameters);

            var array = global.ArrayClass.New();
            var items = new List<JsInstance>
            {
                target
            };
            items.AddRange(parameters);
            int n = 0;
            while (items.Count > 0)
            {
                JsInstance e = items[0];
                items.RemoveAt(0);
                if (global.ArrayClass.HasInstance(e as JsObject))
                {
                    for (int k = 0; k < ((JsObject)e).Length; k++)
                    {
                        string p = k.ToString();
                        JsInstance result = null;
                        if (((JsObject)e).TryGetProperty(p, out result))
                            array.Put(n, result);
                        n++;
                    }
                }
                else
                {
                    array.Put(n, e);
                    n++;
                }
            }
            return array;
        }

        /// <summary>
        /// 15.4.4.5
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Join(JsInstance target, JsInstance[] parameters)
        {
            if (target is JsArray)
                return ((JsArray)target).Join(parameters.Length > 0 ? parameters[0] : JsUndefined.Instance);
            string separator = (parameters.Length == 0 || IsUndefined(parameters[0]))
                ? ","
                : parameters[0].ToString();

            var jsObject = target as JsObject;

            if (jsObject == null || jsObject.Length == 0)
                return JsString.Create();

            JsInstance element0 = jsObject[0.ToString()];

            StringBuilder r;
            if (IsNullOrUndefined(element0))
                r = new StringBuilder(string.Empty);
            else
                r = new StringBuilder(element0.ToString());

            var length = jsObject["length"].ToNumber();

            for (int k = 1; k < length; k++)
            {
                r.Append(separator);
                JsInstance element = jsObject[k.ToString()];
                if (!IsNullOrUndefined(element))
                    r.Append(element);
            }
            return JsString.Create(r.ToString());
        }

        /// <summary>
        /// 15.4.4.6
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Pop(JsInstance target, JsInstance[] parameters)
        {
            var jsObject = target as JsObject;
            if (jsObject == null || jsObject.Length <= 0)
                return JsUndefined.Instance;
            var key = (jsObject.Length - 1).ToString();
            var result = jsObject[key];
            jsObject.Delete(key);
            jsObject.Length--;
            return result;
        }

        /// <summary>
        /// 15.4.4.7
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Push(JsObject target, JsInstance[] parameters)
        {
            int length = (int)target["length"].ToNumber();
            foreach (var arg in parameters)
            {
                target[JsNumber.Create(length)] = arg;
                length++;
            }

            return JsNumber.Create(length);
        }

        /// <summary>
        /// 15.4.4.8
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Reverse(JsObject target, JsInstance[] parameters)
        {
            int len = target.Length;
            int middle = len / 2;

            for (int lower = 0; lower != middle; lower++)
            {
                int upper = len - lower - 1;
                string upperP = upper.ToString();
                string lowerP = lower.ToString();

                JsInstance lowerValue = null;
                JsInstance upperValue = null;
                bool lowerExists = target.TryGetProperty(lowerP, out lowerValue);
                bool upperExists = target.TryGetProperty(upperP, out upperValue);

                if (lowerExists)
                    target[upperP] = lowerValue;
                else
                    target.Delete(upperP);

                if (upperExists)
                    target[lowerP] = upperValue;
                else
                    target.Delete(lowerP);
            }
            return target;
        }

        /// <summary>
        /// 15.4.4.9
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Shift(JsInstance target, JsInstance[] parameters)
        {
            var jsObject = target as JsObject;
            if (jsObject == null || jsObject.Length == 0)
                return JsUndefined.Instance;

            JsInstance first = jsObject[0.ToString()];
            for (int k = 1; k < jsObject.Length; k++)
            {
                JsInstance result = null;

                string from = k.ToString();
                string to = (k - 1).ToString();
                if (jsObject.TryGetProperty(from, out result))
                {
                    jsObject[to] = result;
                }
                else
                {
                    jsObject.Delete(to);
                }
            }
            jsObject.Delete((jsObject.Length - 1).ToString());
            jsObject.Length--;

            return first;
        }

        /// <summary>
        /// 15.4.4.10
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Slice(JsObject target, JsInstance[] parameters)
        {
            var global = target.Global;
            var start = parameters.Length > 0 ? (int)parameters[0].ToNumber() : 0;
            var end = parameters.Length > 1 ? (int)parameters[1].ToNumber() : target.Length;

            if (start < 0)
                start += target.Length;
            if (end < 0)
                end += target.Length;
            if (start > target.Length)
                start = target.Length;
            if (end > target.Length)
                end = target.Length;
            JsArray result = global.ArrayClass.New();
            for (int i = start; i < end; i++)
                Push(result, new JsInstance[] { target[JsNumber.Create(i)] });

            return result;
        }

        /// <summary>
        /// 15.4.4.11
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Sort(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            var jsObject = target as JsObject;
            if (jsObject == null || jsObject.Length <= 1)
                return target;

            JsFunction compare = null;

            // Compare function defined
            if (parameters.Length > 0)
            {
                compare = parameters[0] as JsFunction;
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
                    values.Sort(new JsComparer(global.Backend, compare));
                }
                catch (Exception e)
                {
                    if (e.InnerException is JsException)
                    {
                        throw e.InnerException;
                    }

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

        /// <summary>
        /// 15.4.4.12
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance Splice(JsObject target, JsInstance[] parameters)
        {
            var global = target.Global;
            var array = global.ArrayClass.New();
            int relativeStart = Convert.ToInt32(parameters[0].ToNumber());
            int actualStart = relativeStart < 0 ? Math.Max(target.Length + relativeStart, 0) : Math.Min(relativeStart, target.Length);
            int actualDeleteCount = Math.Min(Math.Max(Convert.ToInt32(parameters[1].ToNumber()), 0), target.Length - actualStart);
            int len = target.Length;

            for (int k = 0; k < actualDeleteCount; k++)
            {
                string from = (relativeStart + k).ToString();
                JsInstance result = null;
                if (target.TryGetProperty(from, out result))
                {
                    array.Put(k, result);
                }
            }

            List<JsInstance> items = new List<JsInstance>();
            items.AddRange(parameters);
            items.RemoveAt(0);
            items.RemoveAt(0);

            // use non-distructional copy, determine direction
            if (items.Count < actualDeleteCount)
            {
                for (int k = actualStart; k < len - actualDeleteCount; k++)
                {
                    JsInstance result = null;
                    string from = (k + actualDeleteCount).ToString();
                    string to = (k + items.Count).ToString();
                    if (target.TryGetProperty(from, out result))
                    {
                        target[to] = result;
                    }
                    else
                    {
                        target.Delete(to);
                    }
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
                    JsInstance result = null;
                    string from = (k + actualDeleteCount - 1).ToString();
                    string to = (k + items.Count - 1).ToString();
                    if (target.TryGetProperty(from, out result))
                    {
                        target[to] = result;
                    }
                    else
                    {
                        target.Delete(to);
                    }
                }


            }
            for (int k = 0; k < items.Count; k++)
                target[k.ToString()] = items[k];

            return array;
        }

        /// <summary>
        /// 15.4.4.13
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance UnShift(JsObject target, JsInstance[] parameters)
        {
            for (int k = target.Length; k > 0; k--)
            {
                JsInstance result = null;
                string from = (k - 1).ToString();
                string to = (k + parameters.Length - 1).ToString();
                if (target.TryGetProperty(from, out result))
                {
                    target[to] = result;
                }
                else
                {
                    target.Delete(to);
                }
            }
            List<JsInstance> items = new List<JsInstance>(parameters);
            for (int j = 0; items.Count > 0; j++)
            {
                JsInstance e = items[0];
                items.RemoveAt(0);
                target[j.ToString()] = e;
            }
            return JsNumber.Create(target.Length);
        }

        /// <summary>
        /// 15.4.4.15
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance LastIndexOfImpl(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
            {
                return JsNumber.Create(-1);
            }

            int len = target.Length;
            if (len == 0)
                return JsNumber.Create(-1);
            int n = len;
            if (parameters.Length > 1)
                n = Convert.ToInt32(parameters[1].ToNumber());
            int k;
            JsInstance searchParameter = parameters[0];
            if (n >= 0)
                k = Math.Min(n, len - 1);
            else
                k = len - Math.Abs(n - 1);
            while (k >= 0)
            {
                JsInstance result = null;
                if (target.TryGetProperty(k.ToString(), out result))
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

        /// <summary>
        /// 15.4.4.15
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance IndexOfImpl(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
            {
                return JsNumber.Create(-1);
            }

            int len = (int)target["length"].ToNumber();
            if (len == 0)
                return JsNumber.Create(-1);
            int n = 0;
            if (parameters.Length > 1)
                n = Convert.ToInt32(parameters[1].ToNumber());
            int k;
            if (n >= len)
                return JsNumber.Create(-1);

            JsInstance searchParameter = parameters[0];
            if (n >= 0)
                k = n;
            else
                k = len - Math.Abs(n);
            while (k < len)
            {
                JsInstance result;
                if (
                    target.TryGetProperty(k.ToString(), out result) &&
                    StrictlyEquals(result, searchParameter)
                )
                    return JsNumber.Create(k);
                k++;
            }
            return JsNumber.Create(-1);
        }

        private static JsInstance GetLengthImpl(JsObject that)
        {
            return JsNumber.Create(that.Length);
        }

        private static JsInstance SetLengthImpl(JsObject that, JsInstance[] parameters)
        {
            if (that is JsArray)
            {
                that.Length = (int)parameters[0].ToNumber();
            }
            else
            {
                int oldLen = that.Length;
                that.Length = (int)parameters[0].ToNumber();

                for (int i = that.Length; i < oldLen; i++)
                    that.Delete(JsNumber.Create(i));
            }

            return parameters[0];
        }
    }
}

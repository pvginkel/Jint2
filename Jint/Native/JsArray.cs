﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsArray : JsObject
    {
        private int _length;
        private SortedList<int, JsInstance> _data = new SortedList<int, JsInstance>();

        internal JsArray(JsGlobal global, JsObject prototype)
            : base(global, null, prototype)
        {
        }

        private JsArray(JsGlobal global, SortedList<int, JsInstance> data, int len, JsObject prototype)
            : base(global, null, prototype)
        {
            _data = data;
            _length = len;
        }

        /// <summary>
        /// 15.4.2
        /// </summary>
        public override string Class
        {
            get
            {
                return ClassArray;
            }
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public override int Length
        {
            get
            {
                return _length;
            }
            set
            {
                SetLength(value);
            }
        }

        public override JsInstance this[string index]
        {
            get
            {
                int i;
                if (Int32.TryParse(index, out i))
                    return Get(i);
                else
                    return base[index];
            }
            set
            {
                int i;
                if (Int32.TryParse(index, out i))
                    Put(i, value);
                else
                    base[index] = value;
            }
        }

        /// <summary>
        /// Overriden indexer to optimize cases when we already have a number
        /// </summary>
        /// <param name="key">index</param>
        /// <returns>value</returns>
        public override JsInstance this[JsInstance key]
        {
            get
            {
                double keyNumber = key.ToNumber();
                int i = (int)keyNumber;
                if (i == keyNumber && i >= 0)
                {
                    // we have got an index
                    return Get(i);
                }
                else
                {
                    return base[key.ToString()];
                }
            }
            set
            {
                double keyNumber = key.ToNumber();
                int i = (int)keyNumber;
                if (i == keyNumber && i >= 0)
                {
                    // we have got an index
                    Put(i, value);
                }
                else
                {
                    base[key.ToString()] = value;
                }
            }
        }

        public override void DefineOwnProperty(Descriptor d)
        {
            int index;
            if (int.TryParse(d.Name, out index))
                Put(index, d.Get(this));
            else
                base.DefineOwnProperty(d);
        }

        public JsInstance Get(int i)
        {
            JsInstance value;
            return _data.TryGetValue(i, out value) && value != null ? value : JsUndefined.Instance;
        }

        public JsInstance Put(int i, JsInstance value)
        {
            if (i >= _length)
                _length = i + 1;
            return _data[i] = value;
        }

        private void SetLength(int newLength)
        {
            if (newLength < 0)
                throw new ArgumentOutOfRangeException("New length is out of range");

            if (newLength < _length)
            {
                int keyIndex = FindKeyOrNext(newLength);
                if (keyIndex >= 0)
                {
                    for (int i = _data.Count - 1; i >= keyIndex; i--)
                        _data.RemoveAt(i);
                }
            }
            _length = newLength;
        }

        public override bool TryGetProperty(string key, out JsInstance result)
        {
            result = JsUndefined.Instance;

            int index;
            if (int.TryParse(key, out index))
                return _data.TryGetValue(Convert.ToInt32(index), out result);
            else
                return base.TryGetProperty(key, out result);

        }

        private int FindKeyOrNext(int key)
        {
            int left = 0, right = _data.Count - 1;
            int index = 0;
            while (left <= right)
            {
                int current = _data.Keys[index];
                if (current == key)
                    return index;
                else
                {
                    if (current > key)
                        right = index - 1;
                    else
                        left = index + 1;
                    index = (left + right) / 2;
                }
            }

            // not found, left will contain next after index if it's in range
            return left < _data.Count ? left : -1;

        }

        private int FindKeyOrPrev(int key)
        {
            int left = 0, right = _data.Count - 1;
            int index = 0;
            while (left <= right)
            {
                int current = _data.Keys[index];
                if (current == key)
                    return index;
                else
                {
                    if (current > key)
                        right = index - 1;
                    else
                        left = index + 1;
                    index = (left + right) / 2;
                }
            }

            // not found, right will contain previous before index if it's in range
            return right;
        }

        public override bool Delete(JsInstance key)
        {
            double keyNumber = key.ToNumber();
            int index = (int)keyNumber;
            if (index == keyNumber)
            {
                _data.Remove(index);
                return true;
            }

            return base.Delete(key.ToString());
        }

        public override bool Delete(string key)
        {
            int index;
            if (int.TryParse(key, out index))
            {
                _data.Remove(index);
                return true;
            }

            return base.Delete(key);
        }

        public JsArray Concat(JsInstance[] args)
        {
            var newData = new SortedList<int, JsInstance>(_data);
            int offset = _length;
            foreach (var item in args)
            {
                if (item is JsArray)
                {
                    foreach (var pair in ((JsArray)item)._data)
                        newData.Add(pair.Key + offset, pair.Value);
                    offset += ((JsArray)item).Length;
                }
                else if (Global.ArrayClass.HasInstance(item as JsObject))
                {
                    // Array subclass
                    JsObject obj = (JsObject)item;

                    for (int i = 0; i < obj.Length; i++)
                    {
                        JsInstance value;
                        if (obj.TryGetProperty(i.ToString(), out value))
                            newData.Add(offset + i, value);
                    }
                }
                else
                {
                    newData.Add(offset, item);
                    offset++;
                }
            }

            return new JsArray(Global, newData, offset, Global.ArrayClass.Prototype);
        }

        public JsString Join(JsInstance separator)
        {
            if (_length == 0)
                return JsString.Create();

            string sep = IsUndefined(separator) ? "," : separator.ToString();
            string[] map = new string[_length];

            for (int i = 0; i < _length; i++)
            {
                JsInstance item;
                map[i] =
                    _data.TryGetValue(i, out item) && !IsNullOrUndefined(item)
                    ? item.ToString()
                    : "";
            }

            return JsString.Create(String.Join(sep, map));
        }

        private IEnumerable<string> BaseGetKeys()
        {
            return base.GetKeys();
        }

        public override IEnumerable<string> GetKeys()
        {
            var keys = _data.Keys;
            for (int i = 0; i < keys.Count; i++)
                yield return keys[i].ToString();

            foreach (var key in BaseGetKeys())
                yield return key;
        }

        private IEnumerable<JsInstance> BaseGetValues()
        {
            return base.GetValues();
        }

        public override IEnumerable<JsInstance> GetValues()
        {
            var vals = _data.Values;
            for (int i = 0; i < vals.Count; i++)
                yield return vals[i];
            foreach (var val in BaseGetValues())
                yield return val;
        }

        public override bool HasOwnProperty(string key)
        {
            int index;
            if (int.TryParse(key, out index))
                return index >= 0 && index < _length ? _data.ContainsKey(index) : false;
            else
                return base.HasOwnProperty(key);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class ArrayPropertyStore : IPropertyStore
    {
        private readonly JsObject _owner;
        private DictionaryPropertyStore _baseStore;
        private readonly SortedList<int, JsInstance> _data;

        public ArrayPropertyStore(JsObject owner, SortedList<int, JsInstance> data)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
            _data = data ?? new SortedList<int, JsInstance>();
        }

        public void SetLength(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "New length is out of range");

            if (length < _owner.Length)
            {
                int keyIndex = FindKeyOrNext(length);
                if (keyIndex >= 0)
                {
                    for (int i = _data.Count - 1; i >= keyIndex; i--)
                        _data.RemoveAt(i);
                }
            }
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

        public bool HasOwnProperty(string index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _owner.Length && _data.ContainsKey(i);

            if (_baseStore != null)
                return _baseStore.HasOwnProperty(index);

            return false;
        }

        public bool HasOwnProperty(JsInstance index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _owner.Length && _data.ContainsKey(i);

            if (_baseStore != null)
                return _baseStore.HasOwnProperty(index);

            return false;
        }

        public Descriptor GetOwnDescriptor(string index)
        {
            if (_baseStore != null)
                return _baseStore.GetOwnDescriptor(index);

            return null;
        }

        public Descriptor GetOwnDescriptor(JsInstance index)
        {
            if (_baseStore != null)
                return _baseStore.GetOwnDescriptor(index);

            return null;
        }

        private bool TryParseIndex(string index, out int result)
        {
            return int.TryParse(index, out result);
        }

        private bool TryParseIndex(JsInstance index, out int result)
        {
            double indexNumber = index.ToNumber();
            result = (int)indexNumber;
// ReSharper disable CompareOfFloatsByEqualityOperator
            return result == indexNumber && result >= 0;
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                result = GetByIndex(i);
                return true;
            }

            result = null;
            return false;
        }

        public bool TryGetProperty(string index, out JsInstance result)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                result = GetByIndex(i);
                return true;
            }

            result = null;
            return false;
        }

        public JsInstance GetByIndex(int index)
        {
            JsInstance value;
            if (_data.TryGetValue(index, out value))
                return value;

            return JsUndefined.Instance;
        }

        public bool TrySetProperty(string index, JsInstance value)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                SetByIndex(i, value);
                return true;
            }

            return false;
        }

        public bool TrySetProperty(JsInstance index, JsInstance value)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                SetByIndex(i, value);
                return true;
            }

            return false;
        }

        public void SetByIndex(int index, JsInstance value)
        {
            if (index >= _owner.Length)
                _owner.Length = index + 1;

            _data[index] = value;
        }

        public bool Delete(JsInstance index)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                _data.Remove(i);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.Delete(index);

            return true;
        }

        public bool Delete(string index)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                _data.Remove(i);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.Delete(index);

            return true;
        }

        public void DefineOwnProperty(Descriptor descriptor)
        {
            int i;
            if (TryParseIndex(descriptor.Name, out i))
                SetByIndex(i, descriptor.Get(_owner));

            EnsureBaseStore();
            _baseStore.DefineOwnProperty(descriptor);
        }

        private void EnsureBaseStore()
        {
            if (_baseStore == null)
                _baseStore = new DictionaryPropertyStore(_owner);
        }

        public IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator()
        {
            if (_baseStore != null)
                return _baseStore.GetEnumerator();

            return JsObject.EmptyKeyValues.GetEnumerator();
        }

        public IEnumerable<JsInstance> GetValues()
        {
            var values = _data.Values;
            for (int i = 0; i < values.Count; i++)
            {
                yield return values[i];
            }

            if (_baseStore != null)
            {
                foreach (var value in _baseStore.GetValues())
                {
                    yield return value;
                }
            }
        }

        public IEnumerable<string> GetKeys()
        {
            var keys = _data.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                yield return keys[i].ToString(CultureInfo.InvariantCulture);
            }

            if (_baseStore != null)
            {
                foreach (var key in _baseStore.GetKeys())
                {
                    yield return key;
                }
            }
        }

        public JsArray Concat(JsInstance[] args)
        {
            var newData = new SortedList<int, JsInstance>(_data);
            int offset = _owner.Length;

            foreach (var item in args)
            {
                var array = item as JsArray;
                if (array != null)
                {
                    var propertyStore = (ArrayPropertyStore)array.PropertyStore;

                    foreach (var pair in propertyStore._data)
                    {
                        newData.Add(pair.Key + offset, pair.Value);
                    }

                    offset += array.Length;
                }
                else
                {
                    var @object = item as JsObject;
                    if (
                        @object != null &&
                        _owner.Global.ArrayClass.HasInstance(@object)
                    )
                    {
                        // Array subclass.

                        for (int i = 0; i < @object.Length; i++)
                        {
                            JsInstance value;
                            if (@object.TryGetProperty(i.ToString(CultureInfo.InvariantCulture), out value))
                                newData.Add(offset + i, value);
                        }
                    }
                    else
                    {
                        newData.Add(offset, item);
                        offset++;
                    }
                }
            }

            return new JsArray(_owner.Global, newData, offset, _owner.Global.ArrayClass.Prototype);
        }

        public JsInstance Join(JsInstance separator)
        {
            int length = _owner.Length;
            if (length == 0)
                return JsString.Empty;

            string separatorString = JsInstance.IsUndefined(separator) ? "," : separator.ToString();
            string[] map = new string[length];

            for (int i = 0; i < length; i++)
            {
                JsInstance item;
                map[i] =
                    _data.TryGetValue(i, out item) && !JsInstance.IsNullOrUndefined(item)
                    ? item.ToString()
                    : "";
            }

            return JsString.Create(String.Join(separatorString, map));
        }
    }
}

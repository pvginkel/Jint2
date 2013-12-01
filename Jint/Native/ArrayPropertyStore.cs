﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    internal class ArrayPropertyStore : SparseArray<JsInstance>, IPropertyStore
    {
        private readonly JsObject _owner;
        private DictionaryPropertyStore _baseStore;

        public ArrayPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        public ArrayPropertyStore(JsObject owner, SparseArray<JsInstance> array)
            : base(array)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        public void SetLength(int length, int oldLength)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "New length is out of range");

            for (int i = length; i < oldLength; i++)
            {
                Remove(i);
            }
        }

        public bool HasOwnProperty(int index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _owner.Length && ContainsKey(i);

            if (_baseStore != null)
                return _baseStore.HasOwnProperty(index);

            return false;
        }

        public bool HasOwnProperty(JsInstance index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _owner.Length && ContainsKey(i);

            if (_baseStore != null)
                return _baseStore.HasOwnProperty(index);

            return false;
        }

        public Descriptor GetOwnDescriptor(int index)
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

        private bool TryParseIndex(int index, out int result)
        {
            // Indexes are stored as negative numbers. However, when ResolveIndex
            // can parse the index as an integer, it returns the parsed integer
            // as a positive value. This allows us to have a simple >= 0 check
            // here to be sure that we got a number.

            result = index;
            return index >= 0;
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

        public bool TryGetProperty(int index, out JsInstance result)
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
            return this[index] ?? JsUndefined.Instance;
        }

        public bool TrySetProperty(int index, JsInstance value)
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

            this[index] = value;
        }

        public bool Delete(JsInstance index)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                Remove(i);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.Delete(index);

            return true;
        }

        public bool Delete(int index)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                Remove(i);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.Delete(index);

            return true;
        }

        public void DefineOwnProperty(Descriptor descriptor)
        {
            int i;
            if (TryParseIndex(descriptor.Index, out i))
                SetByIndex(i, descriptor.Get(_owner));

            EnsureBaseStore();
            _baseStore.DefineOwnProperty(descriptor);
        }

        private void EnsureBaseStore()
        {
            if (_baseStore == null)
                _baseStore = new DictionaryPropertyStore(_owner);
        }

        public IEnumerator<KeyValuePair<int, JsInstance>> GetEnumerator()
        {
            if (_baseStore != null)
                return _baseStore.GetEnumerator();

            return JsObject.EmptyKeyValues.GetEnumerator();
        }

        public new IEnumerable<JsInstance> GetValues()
        {
            foreach (var value in base.GetValues())
            {
                yield return value;
            }

            if (_baseStore != null)
            {
                foreach (var value in _baseStore.GetValues())
                {
                    yield return value;
                }
            }
        }

        public new IEnumerable<int> GetKeys()
        {
            foreach (var key in base.GetKeys())
            {
                yield return key;
            }

            if (_baseStore != null)
            {
                foreach (int key in _baseStore.GetKeys())
                {
                    yield return key;
                }
            }
        }

        public JsArray Concat(JsInstance[] args)
        {
            var newArray = new SparseArray<JsInstance>(this);
            int offset = _owner.Length;

            foreach (var item in args)
            {
                var array = item as JsArray;
                if (array != null)
                {
                    var oldArray = (ArrayPropertyStore)array.PropertyStore;

                    foreach (int key in oldArray.GetKeys())
                    {
                        newArray[key + offset] = oldArray[key];
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
                            if (@object.TryGetProperty(i, out value))
                                newArray[offset + 1] = value;
                        }
                    }
                    else
                    {
                        newArray[offset] = item;
                        offset++;
                    }
                }
            }

            return new JsArray(_owner.Global, newArray, offset, _owner.Global.ArrayClass.Prototype);
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
                var item = this[i];
                if (item != null && !JsInstance.IsNullOrUndefined(item))
                    map[i] = item.ToSource();
                else
                    map[i] = String.Empty;
            }

            return JsString.Create(String.Join(separatorString, map));
        }
    }
}

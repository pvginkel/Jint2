using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    internal class ArrayPropertyStore : SparseArray<JsBox>, IPropertyStore
    {
        private readonly JsObject _owner;
        private DictionaryPropertyStore _baseStore;
        private int _length;

        public int Length
        {
            get { return _length; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "New length is out of range");

                for (int i = value; i < _length; i++)
                {
                    this[i] = new JsBox();
                }

                _length = value;
            }
        }

        public ArrayPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        public ArrayPropertyStore(JsObject owner, SparseArray<JsBox> array)
            : base(array)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        private bool ContainsKey(int index)
        {
            return base[index].IsValid;
        }

        public bool HasOwnProperty(int index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _length && ContainsKey(i);

            if (_baseStore != null)
                return _baseStore.HasOwnProperty(index);

            return false;
        }

        public bool HasOwnProperty(JsBox index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return i >= 0 && i < _length && ContainsKey(i);

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

        public Descriptor GetOwnDescriptor(JsBox index)
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

        private bool TryParseIndex(JsBox index, out int result)
        {
            double indexNumber = index.ToNumber();
            result = (int)indexNumber;
// ReSharper disable CompareOfFloatsByEqualityOperator
            return result == indexNumber && result >= 0;
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool TryGetProperty(JsBox index, out JsBox result)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                result = GetByIndex(i);
                return true;
            }

            result = new JsBox();
            return false;
        }

        public bool TryGetProperty(int index, out JsBox result)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                result = GetByIndex(i);
                return true;
            }

            result = new JsBox();
            return false;
        }

        public override JsBox this[int index]
        {
            get
            {
                var result = base[index];
                if (result.IsValid)
                    return result;
                return JsBox.Undefined;
            }
            set
            {
                if (index >= _length)
                    _length = index + 1;

                base[index] = value;
            }
        }

        public JsBox GetByIndex(int index)
        {
            return this[index];
        }

        public bool TrySetProperty(int index, JsBox value)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                SetByIndex(i, value);
                return true;
            }

            return false;
        }

        public bool TrySetProperty(JsBox index, JsBox value)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                SetByIndex(i, value);
                return true;
            }

            return false;
        }

        public void SetByIndex(int index, JsBox value)
        {
            this[index] = value;
        }

        public bool Delete(JsBox index)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                this[i] = new JsBox();
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
                this[i] = new JsBox();
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
                SetByIndex(i, descriptor.Get(JsBox.CreateObject(_owner)));

            EnsureBaseStore();
            _baseStore.DefineOwnProperty(descriptor);
        }

        private void EnsureBaseStore()
        {
            if (_baseStore == null)
                _baseStore = new DictionaryPropertyStore(_owner);
        }

        public IEnumerator<KeyValuePair<int, JsBox>> GetEnumerator()
        {
            if (_baseStore != null)
                return _baseStore.GetEnumerator();

            return JsObject.EmptyKeyValues.GetEnumerator();
        }

        public new IEnumerable<JsBox> GetValues()
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

        public JsObject Concat(JsBox[] args)
        {
            var newArray = new SparseArray<JsBox>(this);
            int offset = _length;

            foreach (var item in args)
            {
                var oldArray = item.FindArrayStore(false);
                if (oldArray != null)
                {
                    foreach (int key in oldArray.GetKeys())
                    {
                        newArray[key + offset] = oldArray.GetByIndex(key);
                    }

                    offset += oldArray._length;
                }
                else
                {
                    JsObject @object;

                    if (
                        item.IsObject &&
                        _owner.Global.ArrayClass.HasInstance(@object = (JsObject)item)
                    )
                    {
                        // Array subclass.

                        int objectLength = (int)@object.GetProperty(Id.length).ToNumber();

                        for (int i = 0; i < objectLength; i++)
                        {
                            JsBox value;
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

            var result = _owner.Global.CreateArray();

            result.PropertyStore = new ArrayPropertyStore(result, newArray)
            {
                Length = offset
            };

            return result;
        }

        public JsBox Join(JsBox separator)
        {
            int length = _length;
            if (length == 0)
                return JsBox.EmptyString;

            string separatorString = separator.IsUndefined ? "," : separator.ToString();
            string[] map = new string[length];

            for (int i = 0; i < length; i++)
            {
                var item = base[i];
                if (item.IsValid && !item.IsNullOrUndefined)
                    map[i] = item.ToString();
                else
                    map[i] = String.Empty;
            }

            return JsString.Box(String.Join(separatorString, map));
        }
    }
}

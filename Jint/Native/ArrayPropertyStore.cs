// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    [DebuggerTypeProxy(typeof(ArrayPropertyStoreDebugView))]
    internal sealed class ArrayPropertyStore : SparseArray<object>, IPropertyStore
    {
        private DictionaryPropertyStore _baseStore;
        private int _length;

        public JsObject Owner { get; private set; }

        public int Length
        {
            get { return _length; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "New length is out of range");

                for (int i = value; i < _length; i++)
                {
                    SetValue(i, null);
                }

                _length = value;
            }
        }

        public ArrayPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
        }

        public ArrayPropertyStore(JsObject owner, SparseArray<object> array)
            : base(array)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
        }

        public object GetOwnProperty(int index)
        {
            object result = GetOwnPropertyRaw(index);

            if (result == null)
                return JsUndefined.Instance;

            var accessor = result as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(Owner);

            return result;
        }

        public object GetOwnProperty(object index)
        {
            object result = GetOwnPropertyRaw(index);

            if (result == null)
                return JsUndefined.Instance;

            var accessor = result as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(Owner);

            return result;
        }

        public object GetOwnPropertyRaw(int index)
        {
            if (index >= 0)
                return GetValue(index);

            if (_baseStore != null)
                return _baseStore.GetOwnPropertyRaw(index);

            return null;
        }

        public object GetOwnPropertyRaw(object index)
        {
            int i;
            if (TryParseIndex(index, out i))
                return GetValue(i);

            if (_baseStore != null)
                return _baseStore.GetOwnPropertyRaw(index);

            return null;
        }

        public void SetPropertyValue(int index, object value)
        {
            if (index >= 0)
            {
                Debug.Assert(GetValue(index) != null);
                Debug.Assert(!(GetValue(index) is PropertyAccessor));

                SetValue(index, value);
            }
            else
            {
                EnsureBaseStore();
                _baseStore.SetPropertyValue(index, value);
            }
        }

        public void DefineOrSetPropertyValue(int index, object value)
        {
            SetValue(index, value);

            if (index >= _length)
                _length = index + 1;
        }

        public void SetPropertyValue(object index, object value)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                Debug.Assert(GetValue(i) != null);
                Debug.Assert(!(GetValue(i) is PropertyAccessor));

                SetValue(i, value);
            }
            else
            {
                EnsureBaseStore();
                _baseStore.SetPropertyValue(index, value);
            }
        }

        public bool DeleteProperty(int index, bool strict)
        {
            if (index >= 0)
            {
                SetValue(index, null);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.DeleteProperty(index, strict);

            return true;
        }

        public bool DeleteProperty(object index, bool strict)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                SetValue(i, null);
                return true;
            }

            if (_baseStore != null)
                return _baseStore.DeleteProperty(index, strict);

            return true;
        }

        public void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            if (index >= 0)
            {
                if (attributes != 0)
                    throw new JintException("Cannot set attributes on array properties");

                Debug.Assert(GetValue(index) == null);

                SetValue(index, value);
                if (index >= _length)
                    _length = index + 1;
            }
            else
            {
                EnsureBaseStore();
                _baseStore.DefineProperty(index, value, attributes);
            }
        }

        public void DefineProperty(object index, object value, PropertyAttributes attributes)
        {
            int i;
            if (TryParseIndex(index, out i))
            {
                if (attributes != 0)
                    throw new JintException("Cannot set attributes on array properties");

                Debug.Assert(GetValue(i) == null);

                SetValue(i, value);
                if (i >= _length)
                    _length = i + 1;
            }
            else
            {
                EnsureBaseStore();
                _baseStore.DefineProperty(index, value, attributes);
            }
        }

        private bool TryParseIndex(object index, out int result)
        {
            double indexNumber = JsValue.ToNumber(index);
            result = (int)indexNumber;
            return result == indexNumber && result >= 0;
        }

        private void EnsureBaseStore()
        {
            if (_baseStore == null)
                _baseStore = new DictionaryPropertyStore(Owner);
        }

        public JsObject Concat(object[] args)
        {
            var newArray = new SparseArray<object>(this);
            int offset = _length;

            foreach (var item in args)
            {
                var oldArray = item.FindArrayStore(false);
                if (oldArray != null)
                {
                    foreach (int key in oldArray.GetKeys())
                    {
                        newArray.SetValue(
                            key + offset,
                            oldArray.GetValue(key)
                        );
                    }

                    offset += oldArray._length;
                }
                else
                {
                    var @object = item as JsObject;

                    if (
                        @object != null &&
                        Owner.Global.ArrayClass.HasInstance(@object)
                    )
                    {
                        // Array subclass.

                        int objectLength = (int)JsValue.ToNumber(@object.GetProperty(Id.length));

                        for (int i = 0; i < objectLength; i++)
                        {
                            object value = @object.GetPropertyRaw(i);
                            if (value != null)
                                newArray.SetValue(offset + 1, value);
                        }
                    }
                    else
                    {
                        newArray.SetValue(offset, item);
                        offset++;
                    }
                }
            }

            var result = Owner.Global.CreateArray();

            result.PropertyStore = new ArrayPropertyStore(result, newArray)
            {
                Length = offset
            };

            return result;
        }

        public object Join(object separator)
        {
            int length = _length;
            if (length == 0)
                return String.Empty;

            string separatorString = JsValue.IsUndefined(separator) ? "," : JsValue.ToString(separator);
            string[] map = new string[length];

            for (int i = 0; i < length; i++)
            {
                var item = GetValue(i);
                if (item != null && item != JsNull.Instance && !(item is JsUndefined))
                    map[i] = item.ToString();
            }

            return String.Join(separatorString, map);
        }

        internal class ArrayPropertyStoreDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly ArrayPropertyStore _container;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DisplayEntry[] Items
            {
                get
                {
                    var entries = new List<DisplayEntry>();

                    foreach (int key in _container.GetKeys())
                    {
                        entries.Add(new DisplayEntry(
                            key,
                            _container.GetOwnPropertyRaw(key)
                        ));
                    }

                    entries.Sort((a, b) => a.Key.CompareTo(b.Key));

                    return entries.ToArray();
                }
            }

            public ArrayPropertyStoreDebugView(ArrayPropertyStore container)
            {
                _container = container;
            }

            [DebuggerDisplay("Key={Key}, Value={Value}")]
            private class DisplayEntry
            {
                public int Key { get; private set; }
                public object Value { get; private set; }

                public DisplayEntry(int key, object value)
                {
                    Key = key;
                    Value = value;
                }
            }
        }
    }
}

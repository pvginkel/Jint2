using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class ArrayShim : IEnumerable<KeyValuePair<int, object>>
    {
        private readonly ArrayShimOptions _options;
        private readonly JsObject _object;
        private readonly ArrayPropertyStore _store;

        public int Length { get; private set; }
        public bool IsArray { get; private set; }

        public object this[int index]
        {
            get
            {
                if (_store != null)
                    return _store.GetValue(index);

                return _object.GetProperty(index);
            }
        }

        public ArrayShim(object value)
            : this(value, ArrayShimOptions.None)
        {
        }

        public ArrayShim(object value, ArrayShimOptions options)
        {
            _options = options;
            _object = value as JsObject;

            if (_object != null)
            {
                _store = _object.FindArrayStore();

                if (_store != null)
                {
                    Length = _store.Length;
                    IsArray = true;
                }
                else if (_object.HasProperty(Id.length))
                {
                    Length = JsValue.ToInt32(_object.GetProperty(Id.length));
                    IsArray = true;
                }
            }
        }

        public bool TryGetProperty(int index, out object result)
        {
            if (_store == null)
                return _object.TryGetProperty(index, out result);

            result = _store.GetValue(index);
            if (result != null)
            {
                var accessor = result as PropertyAccessor;
                if (accessor != null)
                    result = accessor.GetValue(_object);
            }

            return result != null;
        }

        public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
        {
            if ((_options & ArrayShimOptions.IncludeMissing) != 0)
                return GetAllItems();
            return GetItems();
        }

        private IEnumerator<KeyValuePair<int, object>> GetItems()
        {
            if (_store != null)
            {
                foreach (int key in _store.GetKeys())
                {
                    yield return new KeyValuePair<int, object>(key, _store.GetValue(key));
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    var value = _object.GetPropertyRaw(i);
                    if (value != null)
                    {
                        var accessor = value as PropertyAccessor;
                        if (accessor != null)
                            value = accessor.GetValue(_object);

                        yield return new KeyValuePair<int, object>(i, value);
                    }
                }
            }
        }

        private IEnumerator<KeyValuePair<int, object>> GetAllItems()
        {
            if (_store != null)
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return new KeyValuePair<int, object>(i, _store.GetValue(i));
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    var value = _object.GetPropertyRaw(i);
                    if (value != null)
                    {
                        var accessor = value as PropertyAccessor;
                        if (accessor != null)
                            value = accessor.GetValue(_object);
                    }

                    yield return new KeyValuePair<int, object>(i, value ?? JsUndefined.Instance);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

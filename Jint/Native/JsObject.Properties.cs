using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsObject
    {
        public bool HasProperty(string index)
        {
            return HasProperty(Global.ResolveIdentifier(index));
        }

        public bool HasProperty(int index)
        {
            var @object = this;

            while (true)
            {
                if (@object.HasOwnProperty(index))
                    return true;

                @object = @object.Prototype;

                if (@object.IsPrototypeNull)
                    return false;
            }
        }

        public bool HasProperty(object index)
        {
            var @object = this;

            while (true)
            {
                if (@object.HasOwnProperty(index))
                    return true;

                @object = @object.Prototype;

                if (@object.IsPrototypeNull)
                    return false;
            }
        }

        public bool HasOwnProperty(string index)
        {
            return HasOwnProperty(Global.ResolveIdentifier(index));
        }

        public bool HasOwnProperty(int index)
        {
            return GetOwnPropertyRaw(index) != null;
        }

        public bool HasOwnProperty(object index)
        {
            return GetOwnPropertyRaw(index) != null;
        }

        public object GetOwnPropertyRaw(string index)
        {
            return GetOwnPropertyRaw(Global.ResolveIdentifier(index));
        }

        public object GetOwnPropertyRaw(int index)
        {
            if (PropertyStore != null)
                return PropertyStore.GetOwnPropertyRaw(index);

            return null;
        }

        public object GetOwnPropertyRaw(object index)
        {
            if (PropertyStore != null)
                return PropertyStore.GetOwnPropertyRaw(index);

            return null;
        }

        public object GetOwnProperty(string index)
        {
            return GetOwnProperty(Global.ResolveIdentifier(index));
        }

        public object GetOwnProperty(int index)
        {
            object result =
                PropertyStore != null
                ? GetOwnPropertyRaw(index)
                : null;

            if (result == null)
                return ResolveUndefined(index);

            return ResolvePropertyValue(result);
        }

        public object GetOwnProperty(object index)
        {
            var value =
                PropertyStore != null
                ? GetOwnPropertyRaw(index)
                : null;

            if (value == null)
                return JsUndefined.Instance;

            return ResolvePropertyValue(value);
        }

        public object GetProperty(string name)
        {
            return GetProperty(Global.ResolveIdentifier(name));
        }

        public object GetProperty(int index)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
            {
                Trace.WriteLine("Undefined identifier " + Global.GetIdentifier(index));

                return ResolveUndefined(index);
            }

            return ResolvePropertyValue(value);
        }

        public object GetProperty(object index)
        {
            var value = GetPropertyRaw(index);

            if (value == null)
                return JsUndefined.Instance;

            return ResolvePropertyValue(value);
        }

        public bool TryGetProperty(string index, out object result)
        {
            return TryGetProperty(Global.ResolveIdentifier(index), out result);
        }

        public bool TryGetProperty(int index, out object result)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
                result = JsUndefined.Instance;
            else
                result = ResolvePropertyValue(value);

            return value != null;
        }

        public bool TryGetProperty(object index, out object result)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
                result = JsUndefined.Instance;
            else
                result = ResolvePropertyValue(value);

            return value != null;
        }

        private object ResolvePropertyValue(object value)
        {
            Debug.Assert(value != null);

            var accessor = value as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(this);

            return value;
        }

        private object ResolveUndefined(int index)
        {
            // If we're the global scope, perform special handling on JsUndefined.
            if (this == Global.GlobalScope)
                return Global.Engine.ResolveUndefined(Global.GetIdentifier(index), null);

            return JsUndefined.Instance;
        }

        public object GetPropertyRaw(int index)
        {
            if (index == Id.prototype)
            {
                object value = GetOwnPropertyRaw(index);
                if (value != null)
                    return value;

                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            if (index == Id.__proto__)
            {
                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            var @object = this;

            while (true)
            {
                object result = @object.GetOwnPropertyRaw(index);
                if (result != null)
                    return result;

                if (@object.IsPrototypeNull)
                    return null;

                @object = @object.Prototype;
            }
        }

        public object GetPropertyRaw(object index)
        {
            if (this == Global.PrototypeSink || IsPrototypeNull)
                return null;

            var @object = this;

            while (true)
            {
                object result = @object.GetOwnPropertyRaw(index);
                if (result != null)
                    return result;

                if (@object.IsPrototypeNull)
                    return null;

                @object = @object.Prototype;
            }
        }

        public object GetProperty(int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (cacheSlot.Schema != null)
            {
                if (_dictionaryPropertyStore.Schema == cacheSlot.Schema)
                    return _dictionaryPropertyStore.GetOwnPropertyRawUnchecked(cacheSlot.Index);
            }

            return GetPropertySlow(index, ref cacheSlot);
        }

        public object GetPropertySlow(int index, ref DictionaryCacheSlot cacheSlot)
        {
            object value = GetPropertyRaw(index, ref cacheSlot);

            if (value == null)
            {
                Trace.WriteLine("Undefined identifier " + Global.GetIdentifier(index));

                return ResolveUndefined(index);
            }

            return ResolvePropertyValue(value);
        }

        private object GetPropertyRaw(int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (index == Id.prototype)
            {
                object value = GetOwnPropertyRaw(index);
                if (value != null)
                    return value;

                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            if (index == Id.__proto__)
            {
                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            if (PropertyStore != null)
            {
                if (_dictionaryPropertyStore != null)
                {
                    object result = _dictionaryPropertyStore.GetOwnPropertyRaw(index, ref cacheSlot);
                    if (result != null)
                        return result;
                }
                else
                {
                    object result = PropertyStore.GetOwnPropertyRaw(index);
                    if (result != null)
                        return result;
                }
            }

            var @object = Prototype;

            while (!@object.IsPrototypeNull)
            {
                object result = @object.GetOwnPropertyRaw(index);
                if (result != null)
                    return result;

                @object = @object.Prototype;
            }

            return null;
        }

        public void SetProperty(string name, object value)
        {
            SetProperty(Global.ResolveIdentifier(name), value);
        }

        public void SetProperty(int index, object value)
        {
            if (index == Id.__proto__)
            {
                var @object = value as JsObject;
                if (@object != null)
                    Prototype = @object;
                return;
            }

            // CLR objects have their own rules concerning how and when a
            // property is set.

            if (IsClr && _dictionaryPropertyStore == null)
            {
                PropertyStore.SetPropertyValue(index, value);
                return;
            }

            // If we have this property, either set it through the accessor
            // or directly on the property store.

            var currentValue = GetOwnPropertyRaw(index);

            if (currentValue != null)
            {
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                    accessor.SetValue(this, value);
                else
                    PropertyStore.SetPropertyValue(index, value);
                return;
            }

            // Check whether the prototype has an accessor.

            if (!IsPrototypeNull)
            {
                currentValue = Prototype.GetPropertyRaw(index);
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                {
                    accessor.SetValue(this, value);
                    return;
                }
            }

            // Otherwise, we're setting a new property.

            DefineProperty(index, value, PropertyAttributes.None);
        }

        public void SetProperty(object index, object value)
        {
            // CLR objects have their own rules concerning how and when a
            // property is set.

            if (IsClr && _dictionaryPropertyStore == null)
            {
                PropertyStore.SetPropertyValue(index, value);
                return;
            }

            // If we have this property, either set it through the accessor
            // or directly on the property store.

            var currentValue = GetOwnPropertyRaw(index);

            if (currentValue != null)
            {
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                    accessor.SetValue(this, value);
                else
                    PropertyStore.SetPropertyValue(index, value);
                return;
            }

            // Check whether the prototype has an accessor.

            if (!IsPrototypeNull)
            {
                currentValue = Prototype.GetPropertyRaw(index);
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                {
                    accessor.SetValue(this, value);
                    return;
                }
            }

            // Otherwise, we're setting a new property.

            DefineProperty(index, value, PropertyAttributes.None);
        }

        public void SetProperty(int index, object value, ref DictionaryCacheSlot cacheSlot)
        {
            if (cacheSlot.Schema != null)
            {
                if (_dictionaryPropertyStore.Schema == cacheSlot.Schema)
                {
                    _dictionaryPropertyStore.SetPropertyValueUnchecked(cacheSlot.Index, value);
                    return;
                }
            }

            SetPropertySlow(index, value, ref cacheSlot);
        }

        private void SetPropertySlow(int index, object value, ref DictionaryCacheSlot cacheSlot)
        {
            // CLR objects have their own rules concerning how and when a
            // property is set.

            if (IsClr && _dictionaryPropertyStore == null)
            {
                PropertyStore.SetPropertyValue(index, value);
                return;
            }

            // If we have this property, either set it through the accessor
            // or directly on the property store.

            object currentValue = null;

            if (PropertyStore != null)
            {
                if (_dictionaryPropertyStore != null)
                    currentValue = _dictionaryPropertyStore.GetOwnPropertyRaw(index, ref cacheSlot);
                else
                    currentValue = PropertyStore.GetOwnPropertyRaw(index);
            }

            if (currentValue != null)
            {
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                {
                    // Cache slots are disabled for accessors. The thinking here
                    // is that accessors are rare, and this prevents us from
                    // having to do a get in the fast path.
                    cacheSlot = new DictionaryCacheSlot();
                    accessor.SetValue(this, value);
                }
                else
                {
                    PropertyStore.SetPropertyValue(index, value);
                }
                return;
            }

            // Check whether the prototype has an accessor.

            if (!IsPrototypeNull)
            {
                currentValue = Prototype.GetPropertyRaw(index);
                var accessor = currentValue as PropertyAccessor;
                if (accessor != null)
                {
                    accessor.SetValue(this, value);
                    return;
                }
            }

            // Otherwise, we're setting a new property.

            DefineProperty(index, value, PropertyAttributes.None);
        }

        public bool DeleteProperty(string index)
        {
            return DeleteProperty(Global.ResolveIdentifier(index));
        }

        public bool DeleteProperty(int index)
        {
            if (PropertyStore != null)
                return PropertyStore.DeleteProperty(index);

            return true;
        }

        public bool DeleteProperty(object index)
        {
            if (PropertyStore != null)
                return PropertyStore.DeleteProperty(index);

            return true;
        }

        public void DefineProperty(int index, JsFunction @delegate, int argumentCount, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(
                index,
                Global.CreateFunction(Global.GetIdentifier(index), @delegate, argumentCount),
                attributes
            );
        }

        public void DefineProperty(string index, object value, PropertyAttributes attributes)
        {
            DefineProperty(Global.ResolveIdentifier(index), value, attributes);
        }

        public void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(index, value, attributes);
        }

        public void DefineProperty(object index, object value, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(index, value, attributes);
        }

        public void DefineAccessor(int index, JsFunction getter, JsFunction setter, PropertyAttributes attributes)
        {
            var getterObject =
                getter != null
                    ? Global.CreateFunction(null, getter, 0)
                    : null;
            var setterObject =
                setter != null
                    ? Global.CreateFunction(null, setter, 1)
                    : null;

            DefineAccessor(index, getterObject, setterObject, attributes);
        }

        public void DefineAccessor(int index, JsObject getter, JsObject setter)
        {
            DefineAccessor(index, getter, setter, PropertyAttributes.None);
        }

        public void DefineAccessor(string name, JsObject getter, JsObject setter, PropertyAttributes attributes)
        {
            DefineAccessor(Global.ResolveIdentifier(name), getter, setter, attributes);
        }

        public void DefineAccessor(int index, JsObject getter, JsObject setter, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(
                index,
                new PropertyAccessor(getter, setter),
                attributes
            );
        }

        public IEnumerable<int> GetKeys()
        {
            var @object = this;

            while (true)
            {
                if (@object.PropertyStore != null)
                {
                    foreach (int key in @object.PropertyStore.GetKeys())
                    {
                        yield return key;
                    }
                }

                if (@object.IsPrototypeNull)
                    yield break;

                @object = @object.Prototype;
            }
        }
    }
}

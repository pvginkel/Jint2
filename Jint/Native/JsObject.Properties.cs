using System;
using System.Collections;
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

        public bool HasProperty(JsBox index)
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

        public bool HasOwnProperty(JsBox index)
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

        public object GetOwnPropertyRaw(JsBox index)
        {
            if (PropertyStore != null)
                return PropertyStore.GetOwnPropertyRaw(index);

            return null;
        }

        public JsBox GetOwnProperty(string index)
        {
            return GetOwnProperty(Global.ResolveIdentifier(index));
        }

        public JsBox GetOwnProperty(int index)
        {
            object result =
                PropertyStore != null
                ? GetOwnPropertyRaw(index)
                : null;

            if (result == null)
                return ResolveUndefined(index);

            return ResolvePropertyValue(result);
        }

        public JsBox GetOwnProperty(JsBox index)
        {
            var value =
                PropertyStore != null
                ? GetOwnPropertyRaw(index)
                : null;

            if (value == null)
                return JsBox.Undefined;

            return ResolvePropertyValue(value);
        }

        public JsBox GetProperty(string name)
        {
            return GetProperty(Global.ResolveIdentifier(name));
        }

        public JsBox GetProperty(int index)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
                return ResolveUndefined(index);

            return ResolvePropertyValue(value);
        }

        public JsBox GetProperty(JsBox index)
        {
            var value = GetPropertyRaw(index);

            if (value == null)
                return JsBox.Undefined;

            return ResolvePropertyValue(value);
        }

        public bool TryGetProperty(string index, out JsBox result)
        {
            return TryGetProperty(Global.ResolveIdentifier(index), out result);
        }

        public bool TryGetProperty(int index, out JsBox result)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
                result = JsBox.Undefined;
            else
                result = ResolvePropertyValue(value);

            return value != null;
        }

        public bool TryGetProperty(JsBox index, out JsBox result)
        {
            object value = GetPropertyRaw(index);

            if (value == null)
                result = JsBox.Undefined;
            else
                result = ResolvePropertyValue(value);

            return value != null;
        }

        private JsBox ResolvePropertyValue(object value)
        {
            Debug.Assert(value != null);

            var accessor = value as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(JsBox.CreateObject(this));

            return JsBox.FromValue(value);
        }

        private JsBox ResolveUndefined(int index)
        {
            // If we're the global scope, perform special handling on JsUndefined.
            if (this == Global.GlobalScope)
                return Global.Engine.ResolveUndefined(Global.GetIdentifier(index), null);

            return JsBox.Undefined;
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

        public object GetPropertyRaw(JsBox index)
        {
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

        public void SetProperty(string name, JsBox value)
        {
            SetProperty(Global.ResolveIdentifier(name), value);
        }

        public void SetProperty(int index, JsBox value)
        {
            if (index == Id.__proto__)
            {
                if (value.IsObject)
                    Prototype = (JsObject)value;
                return;
            }

            // CLR objects have their own rules concerning how and when a
            // property is set.

            if (_isClr && !(PropertyStore is DictionaryPropertyStore))
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
                    accessor.SetValue(JsBox.CreateObject(this), value);
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
                    accessor.SetValue(JsBox.CreateObject(this), value);
                    return;
                }
            }

            // Otherwise, we're setting a new property.

            DefineProperty(index, value, PropertyAttributes.None);
        }

        public void SetProperty(JsBox index, JsBox value)
        {
            // CLR objects have their own rules concerning how and when a
            // property is set.

            if (_isClr && !(PropertyStore is DictionaryPropertyStore))
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
                    accessor.SetValue(JsBox.CreateObject(this), value);
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
                    accessor.SetValue(JsBox.CreateObject(this), value);
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

        public bool DeleteProperty(JsBox index)
        {
            if (PropertyStore != null)
                return PropertyStore.DeleteProperty(index);

            return true;
        }

        public void DefineProperty(string index, JsFunction @delegate, int argumentCount, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(
                Global.ResolveIdentifier(index),
                Global.CreateFunction(index, @delegate, argumentCount, null),
                attributes
            );
        }

        public void DefineProperty(string index, JsBox value, PropertyAttributes attributes)
        {
            DefineProperty(Global.ResolveIdentifier(index), value, attributes);
        }

        public void DefineProperty(int index, JsBox value, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(index, value.GetValue(), attributes);
        }

        public void DefineProperty(JsBox index, JsBox value, PropertyAttributes attributes)
        {
            EnsurePropertyStore();
            PropertyStore.DefineProperty(index, value.GetValue(), attributes);
        }

        public void DefineAccessor(string name, JsFunction getter, JsFunction setter, PropertyAttributes attributes)
        {
            var getterObject =
                getter != null
                    ? Global.CreateFunction(null, getter, 0, null)
                    : null;
            var setterObject =
                setter != null
                    ? Global.CreateFunction(null, setter, 1, null)
                    : null;

            DefineAccessor(name, getterObject, setterObject, attributes);
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

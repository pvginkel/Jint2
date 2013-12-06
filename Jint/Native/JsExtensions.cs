using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class JsExtensions
    {
        public static ArrayPropertyStore FindArrayStore(this JsBox value)
        {
            return FindArrayStore(value, true);
        }

        public static ArrayPropertyStore FindArrayStore(this JsBox value, bool lookAtPrototype)
        {
            if (!value.IsObject)
                return null;

            var @object = (JsObject)value;

            while (!@object.IsPrototypeNull)
            {
                var propertyStore = @object.PropertyStore as ArrayPropertyStore;
                if (propertyStore != null || !lookAtPrototype)
                    return propertyStore;

                @object = @object.Prototype;
            }

            return null;
        }

        public static ArrayPropertyStore FindArrayStore(this JsObject @object)
        {
            return FindArrayStore(@object, true);
        }

        public static ArrayPropertyStore FindArrayStore(this JsObject @object, bool lookAtPrototype)
        {
            while (!@object.IsPrototypeNull)
            {
                var propertyStore = @object.PropertyStore as ArrayPropertyStore;
                if (propertyStore != null || !lookAtPrototype)
                    return propertyStore;

                @object = @object.Prototype;
            }

            return null;
        }
    }
}

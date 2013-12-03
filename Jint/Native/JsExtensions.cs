using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class JsExtensions
    {
        public static ArrayPropertyStore FindArrayStore(this JsInstance value)
        {
            return FindArrayStore(value, true);
        }

        public static ArrayPropertyStore FindArrayStore(this JsInstance value, bool lookAtPrototype)
        {
            var @object = value as JsObject;
            if (@object == null)
                return null;

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

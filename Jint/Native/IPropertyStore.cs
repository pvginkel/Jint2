using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IPropertyStore
    {
        JsObject Owner { get; }

        object GetOwnPropertyRaw(int index);
        object GetOwnPropertyRaw(object index);
        void SetPropertyValue(int index, object value);
        void SetPropertyValue(object index, object value);
        bool DeleteProperty(int index, bool strict);
        bool DeleteProperty(object index, bool strict);
        void DefineProperty(int index, object value, PropertyAttributes attributes);
        void DefineProperty(object index, object value, PropertyAttributes attributes);
        IEnumerable<int> GetKeys();
    }
}

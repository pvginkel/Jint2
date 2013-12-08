using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IPropertyStore
    {
        object GetOwnPropertyRaw(int index);
        object GetOwnPropertyRaw(JsBox index);
        void SetPropertyValue(int index, JsBox value);
        void SetPropertyValue(JsBox index, JsBox value);
        bool DeleteProperty(int index);
        bool DeleteProperty(JsBox index);
        void DefineProperty(int index, object value, PropertyAttributes attributes);
        void DefineProperty(JsBox index, object value, PropertyAttributes attributes);
        IEnumerable<int> GetKeys();
    }
}

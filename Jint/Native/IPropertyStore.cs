using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IPropertyStore
    {
        void SetLength(int length);
        bool HasOwnProperty(string index);
        bool HasOwnProperty(JsInstance index);
        Descriptor GetOwnDescriptor(string index);
        Descriptor GetOwnDescriptor(JsInstance index);
        bool TryGetProperty(JsInstance index, out JsInstance result);
        bool TryGetProperty(string index, out JsInstance result);
        bool TrySetProperty(string index, JsInstance value);
        bool TrySetProperty(JsInstance index, JsInstance value);
        bool Delete(JsInstance index);
        bool Delete(string index);
        void DefineOwnProperty(Descriptor currentDescriptor);
        IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator();
        IEnumerable<JsInstance> GetValues();
        IEnumerable<string> GetKeys();
    }
}

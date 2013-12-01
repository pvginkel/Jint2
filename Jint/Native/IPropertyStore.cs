using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IPropertyStore
    {
        void SetLength(int length);
        bool HasOwnProperty(int index);
        bool HasOwnProperty(JsInstance index);
        Descriptor GetOwnDescriptor(int index);
        Descriptor GetOwnDescriptor(JsInstance index);
        bool TryGetProperty(JsInstance index, out JsInstance result);
        bool TryGetProperty(int index, out JsInstance result);
        bool TrySetProperty(int index, JsInstance value);
        bool TrySetProperty(JsInstance index, JsInstance value);
        bool Delete(JsInstance index);
        bool Delete(int index);
        void DefineOwnProperty(Descriptor currentDescriptor);
        IEnumerator<KeyValuePair<int, JsInstance>> GetEnumerator();
        IEnumerable<JsInstance> GetValues();
        IEnumerable<int> GetKeys();
    }
}

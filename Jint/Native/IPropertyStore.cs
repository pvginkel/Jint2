using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IPropertyStore
    {
        bool HasOwnProperty(int index);
        bool HasOwnProperty(JsBox index);
        Descriptor GetOwnDescriptor(int index);
        Descriptor GetOwnDescriptor(JsBox index);
        bool TryGetProperty(JsBox index, out JsBox result);
        bool TryGetProperty(int index, out JsBox result);
        bool TrySetProperty(int index, JsBox value);
        bool TrySetProperty(JsBox index, JsBox value);
        bool Delete(JsBox index);
        bool Delete(int index);
        void DefineOwnProperty(Descriptor currentDescriptor);
        IEnumerator<KeyValuePair<int, JsBox>> GetEnumerator();
        IEnumerable<JsBox> GetValues();
        IEnumerable<int> GetKeys();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    internal class JsonPrinter
    {
        private readonly TextWriter _writer;

        private JsonPrinter(TextWriter writer)
        {
            _writer = writer;
        }

        public static void Print(TextWriter writer, object value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            
            new JsonPrinter(writer).Print(value);
        }

        private void Print(object value)
        {
            var arrayStore = value.FindArrayStore();
            if (arrayStore != null)
            {
                _writer.Write('[');

                bool hadOne = false;

                foreach (int key in arrayStore.GetKeys())
                {
                    if (hadOne)
                        _writer.Write(',');
                    else
                        hadOne = true;

                    Print(arrayStore.GetOwnPropertyRaw(key));
                }

                _writer.Write(']');

                return;
            }

            var @object = value as JsObject;
            if (@object != null)
            {
                _writer.Write('{');

                var keys = new Dictionary<string, int>();
                var global = @object.Global;

                foreach (int key in @object.GetKeys())
                {
                    keys.Add(global.GetIdentifier(key), key);
                }

                bool hadOne = false;

                foreach (var key in keys.OrderBy(p => p.Key))
                {
                    if (hadOne)
                        _writer.Write(',');
                    else
                        hadOne = true;

                    Print(key.Key);
                    _writer.Write(':');
                    Print(@object.GetOwnProperty(key.Value));
                }

                _writer.Write('}');

                return;
            }

            string stringValue = value as string;
            if (stringValue != null)
            {
                _writer.Write('\'');
                _writer.Write(EscapeStringLiteral(stringValue));
                _writer.Write('\'');
            }
            else
            {
                _writer.Write(JsValue.ToString(value));
            }
        }

        private string EscapeStringLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace(Environment.NewLine, "\\r\\n");
        }
    }
}

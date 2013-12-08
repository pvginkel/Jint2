using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Support
{
    internal class FastPropertyStoreDebugView
    {
        private readonly FastPropertyStore _container;

        public FastPropertyStoreDebugView(FastPropertyStore container)
        {
            _container = container;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Entry[] Items
        {
            get
            {
                var items = new List<Entry>();

                foreach (int key in _container.GetKeys(false))
                {
                    items.Add(new Entry(key, _container.GetOwnPropertyRaw(key), _container.GetAttributes(key)));
                }

                items.Sort((a, b) => Math.Abs(a.Index).CompareTo(Math.Abs(b.Index)));

                return items.ToArray();
            }
        }

        public class Entry
        {
            public int Index { get; private set; }
            public object Value { get; private set; }
            public PropertyAttributes Attributes { get; private set; }

            public Entry(int index, object value, PropertyAttributes attributes)
            {
                Index = index;
                Value = value;
                Attributes = attributes;
            }

            public override string ToString()
            {
                string value;
                try
                {
                    if (Value == null)
                        value = null;
                    else
                        value = Value.ToString();
                }
                catch
                {
                    value = Value.GetType().FullName;
                }

                return String.Format("Index={0}, Value={1}, Attributes={2}", Index, value, Attributes);
            }
        }
    }
}

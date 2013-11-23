using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Native
{
    internal class JsDictionaryObjectDebugView
    {
        private readonly JsDictionaryObject _container;

        public JsDictionaryObjectDebugView(JsDictionaryObject container)
        {
            _container = container;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, JsInstance>[] Items
        {
            get { return _container.ToArray(); }
        }
    }
}

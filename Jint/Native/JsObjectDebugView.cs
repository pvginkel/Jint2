using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class JsObjectDebugView
    {
        private readonly JsObject _container;

        public JsObjectDebugView(JsObject container)
        {
            _container = container;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, JsBox>[] Items
        {
            get { return _container.ToArray(); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct DictionaryCacheSlot
    {
        private readonly JsSchema _schema;
        private readonly int _index;

        internal JsSchema Schema
        {
            get { return _schema; }
        }

        internal int Index
        {
            get { return _index; }
        }

        internal DictionaryCacheSlot(JsSchema schema, int index)
        {
            _schema = schema;
            _index = index;
        }
    }
}

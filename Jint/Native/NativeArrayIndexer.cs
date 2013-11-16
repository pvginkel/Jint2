using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    class NativeArrayIndexer<T>: INativeIndexer
    {
        private readonly Marshaller _marshaller;

        public NativeArrayIndexer(Marshaller marshaller)
        {
            if (marshaller == null)
                throw new ArgumentNullException("marshaller");
            _marshaller = marshaller;
        }
        #region INativeIndexer Members

        public JsInstance Get(JsInstance that, JsInstance index)
        {
            return _marshaller.MarshalClrValue( _marshaller.MarshalJsValue<T[]>(that)[_marshaller.MarshalJsValue<int>(index)] );
        }

        public void Set(JsInstance that, JsInstance index, JsInstance value)
        {
            _marshaller.MarshalJsValue<T[]>(that)[_marshaller.MarshalJsValue<int>(index)] = _marshaller.MarshalJsValue<T>(value);
        }

        #endregion
    }
}

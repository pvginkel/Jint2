using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class AbstractPropertyStore : IPropertyStore
    {
        public DictionaryPropertyStore BaseStore { get; private set; }

        public AbstractPropertyStore(DictionaryPropertyStore baseStore)
        {
            if (baseStore == null)
                throw new ArgumentNullException("baseStore");

            BaseStore = baseStore;
        }

        public virtual object GetOwnPropertyRaw(int index)
        {
            return BaseStore.GetOwnPropertyRaw(index);
        }

        public virtual object GetOwnPropertyRaw(JsBox index)
        {
            return BaseStore.GetOwnPropertyRaw(index);
        }

        public virtual void SetPropertyValue(int index, JsBox value)
        {
            BaseStore.SetPropertyValue(index, value);
        }

        public virtual void SetPropertyValue(JsBox index, JsBox value)
        {
            BaseStore.SetPropertyValue(index, value);
        }

        public virtual bool DeleteProperty(int index)
        {
            return BaseStore.DeleteProperty(index);
        }

        public virtual bool DeleteProperty(JsBox index)
        {
            return BaseStore.DeleteProperty(index);
        }

        public virtual void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            BaseStore.DefineProperty(index, value, attributes);
        }

        public virtual void DefineProperty(JsBox index, object value, PropertyAttributes attributes)
        {
            BaseStore.DefineProperty(index, value, attributes);
        }

        public virtual IEnumerable<int> GetKeys()
        {
            return BaseStore.GetKeys();
        }
    }
}

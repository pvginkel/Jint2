using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class AbstractPropertyStore : IPropertyStore
    {
        public DictionaryPropertyStore BaseStore { get; private set; }

        public JsObject Owner
        {
            get { return BaseStore.Owner; }
        }

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

        public virtual object GetOwnPropertyRaw(object index)
        {
            return BaseStore.GetOwnPropertyRaw(index);
        }

        public virtual void SetPropertyValue(int index, object value)
        {
            BaseStore.SetPropertyValue(index, value);
        }

        public virtual void SetPropertyValue(object index, object value)
        {
            BaseStore.SetPropertyValue(index, value);
        }

        public virtual bool DeleteProperty(int index, bool strict)
        {
            return BaseStore.DeleteProperty(index, strict);
        }

        public virtual bool DeleteProperty(object index, bool strict)
        {
            return BaseStore.DeleteProperty(index, strict);
        }

        public virtual void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            BaseStore.DefineProperty(index, value, attributes);
        }

        public virtual void DefineProperty(object index, object value, PropertyAttributes attributes)
        {
            BaseStore.DefineProperty(index, value, attributes);
        }

        public virtual IEnumerable<int> GetKeys()
        {
            return BaseStore.GetKeys();
        }
    }
}

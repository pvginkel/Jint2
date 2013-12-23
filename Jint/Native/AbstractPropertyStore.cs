using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

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
            return ((FastPropertyStore)BaseStore).GetOwnPropertyRaw(index);
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

        public virtual bool DeleteProperty(int index)
        {
            return BaseStore.DeleteProperty(index);
        }

        public virtual bool DeleteProperty(object index)
        {
            return BaseStore.DeleteProperty(index);
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

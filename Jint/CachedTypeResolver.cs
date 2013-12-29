using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jint
{
    public class CachedTypeResolver : ITypeResolver
    {
        private readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();
        private static CachedTypeResolver _default;

        public static CachedTypeResolver Default
        {
            get
            {
                lock (typeof(CachedTypeResolver))
                {
                    return _default ?? (_default = new CachedTypeResolver());
                }
            }
        }

        public Type ResolveType(string fullName)
        {
            _lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (_cache.ContainsKey(fullName))
                {
                    return _cache[fullName];
                }

                Type type = null;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(fullName, false, false);

                    if (type != null)
                    {
                        break;
                    }
                }

                _lock.UpgradeToWriterLock(Timeout.Infinite);

                _cache.Add(fullName, type);
                return type;

            }
            finally
            {
                _lock.ReleaseLock();
            }
        }
    }
}

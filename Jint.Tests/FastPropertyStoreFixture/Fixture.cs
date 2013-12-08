using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Support;
using NUnit.Framework;

namespace Jint.Tests.FastPropertyStoreFixture
{
    [TestFixture]
    public class Fixture
    {
        [Test]
        public void Deletes()
        {
            var store = new FastPropertyStore();

            store.Add(0, 0, 0);
            store.Add(20, 0, 20);
            store.Add(1, 0, 1);
            store.Add(21, 0, 21);
            store.Add(2, 0, 2);
            store.Add(22, 0, 22);

            Assert.AreEqual(6, store.Count);
            Assert.AreEqual(0, store.GetOwnPropertyRaw(0));
            Assert.AreEqual(1, store.GetOwnPropertyRaw(1));
            Assert.AreEqual(2, store.GetOwnPropertyRaw(2));
            Assert.AreEqual(20, store.GetOwnPropertyRaw(20));
            Assert.AreEqual(21, store.GetOwnPropertyRaw(21));
            Assert.AreEqual(22, store.GetOwnPropertyRaw(22));

            store.Remove(1);

            Assert.AreEqual(5, store.Count);
            Assert.AreEqual(0, store.GetOwnPropertyRaw(0));
            Assert.AreEqual(2, store.GetOwnPropertyRaw(2));
            Assert.AreEqual(20, store.GetOwnPropertyRaw(20));
            Assert.AreEqual(21, store.GetOwnPropertyRaw(21));
            Assert.AreEqual(22, store.GetOwnPropertyRaw(22));
        }

        [Test]
        public void Random()
        {
            var store = new FastPropertyStore();
            var rand = new Random();

            for (int i = 0; i < 100000; i++)
            {
                while (store.Count > 10)
                {
                    store.Remove(rand.Next() % 200);
                }

                int index = rand.Next() % 200;

                if (store.GetOwnPropertyRaw(index) == null)
                    store.Add(index, 0, index);

                int numberKeys = 0;

                foreach (int key in store.GetKeys(false))
                {
                    numberKeys++;
                    Assert.AreEqual(key, store.GetOwnPropertyRaw(key));
                }

                Assert.AreEqual(store.Count, numberKeys);

                if (i > 1000)
                    Assert.IsTrue(store.Count == 10 || store.Count == 11);
            }
        }
    }
}

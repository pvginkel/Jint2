﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Support;
using NUnit.Framework;

namespace Jint.Tests.SparseArrayFixture
{
    [TestFixture]
    public class Fixture
    {
        [Test]
        public void SimpleInserts()
        {
            var array = new SparseArray<string>();

            // Insert some items.

            for (int i = 0; i < 1000; i++)
            {
                array.SetValue(i, i.ToString());
            }

            // Verify that we can get them back.

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i.ToString(), array.GetValue(i));
            }

            // Verify that we didn't switch to chunks.

            Assert.AreEqual("Values=1280", array.ToString());
        }

        [Test]
        public void InsertWithSpaces()
        {
            var array = new SparseArray<string>();

            for (int i = 0; i < 1000; i++)
            {
                if (i % 2 == 0)
                    array.SetValue(i, i.ToString());
            }

            for (int i = 0; i < 1000; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.AreEqual(i.ToString(), array.GetValue(i));
                }
                else
                {
                    Assert.IsNull(array.GetValue(i));
                }
            }

            // Verify that we didn't switch to chunks even though we have
            // sparsely set items.

            Assert.AreEqual("Values=1280", array.ToString());
        }

        [Test]
        public void CorrectKeysWithInsertsWithSpaces()
        {
            var array = new SparseArray<string>();

            for (int i = 0; i < 1000; i++)
            {
                if (i % 2 == 0)
                    array.SetValue(i, i.ToString());
            }

            int offset = 0;
            foreach (int key in array.GetKeys())
            {
                Assert.AreEqual(offset, key);
                offset += 2;
            }

            // Verify that we didn't switch to chunks even though we have
            // sparsely set items.

            Assert.AreEqual("Values=1280", array.ToString());
        }

        [Test]
        public void GetNegativeShouldNotFail()
        {
            var array = new SparseArray<string>();

            Assert.IsNull(array.GetValue(-10));

            // We shouldn't have allocated items.

            Assert.AreEqual("Values=20", array.ToString());
        }

        [Test]
        public void GettingNonExistingReturnsNull()
        {
            var array = new SparseArray<string>();

            Assert.IsNull(array.GetValue(10000));

            // We shouldn't have allocated items.

            Assert.AreEqual("Values=20", array.ToString());
        }

        [Test]
        public void InsertAtHighIndex()
        {
            var array = new SparseArray<string>();
            int offset = 1000000;

            // Insert some items.

            for (int i = 0; i < 1000; i++)
            {
                array.SetValue(offset + i, i.ToString());
            }

            // Verify that we can get them back.

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i.ToString(), array.GetValue(offset + i));
            }

            // Verify that we've switched to chunks.

            Assert.AreEqual("Chunks=33, ChunkCapacity=37", array.ToString());
        }

        [Test]
        public void VerifyKeysInOrderFromReverseInserts()
        {
            var array = new SparseArray<string>();

            for (int i = 1000; i > 0; i--)
            {
                array.SetValue(i, "");
            }

            int offset = 1;

            foreach (int key in array.GetKeys())
            {
                Assert.AreEqual(offset++, key);
            }

            // Verify the allocations.

            Assert.AreEqual("Chunks=32, ChunkCapacity=37", array.ToString());
        }

        [Test]
        public void VerifyKeysInOrderFromRandomInserts()
        {
            var array = new SparseArray<string>();
            var random = new Random();

            for (int i = 0; i < 1000; i++)
            {
                array.SetValue(random.Next(), "");
            }

            int lastKey = int.MinValue;

            foreach (int key in array.GetKeys())
            {
                Assert.Greater(key, lastKey);
                lastKey = key;
            }
        }
    }
}

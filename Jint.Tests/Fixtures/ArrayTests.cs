using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ArraysTests : FixturesFixture
    {
        public ArraysTests()
            : base(null)
        {
        }

        [TestCase("Arrays.js")]
        public void ShouldRunArraysTests(string script)
        {
            RunFile(script);
        }
    }
}

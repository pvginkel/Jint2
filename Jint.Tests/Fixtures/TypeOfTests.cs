using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class TypeOfTests : FixturesFixture
    {
        public TypeOfTests()
            : base(null)
        {
        }

        [TestCase("TypeOf.js")]
        public void ShouldRunTypeOfTests(string script)
        {
            RunFile(script);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class FunctionAsConstructorTests : FixturesFixture
    {
        public FunctionAsConstructorTests()
            : base(null)
        {
        }

        [TestCase("FunctionAsConstructor.js")]
        public void ShouldRunFunctionAsConstructorTests(string script)
        {
            RunFile(script);
        }
    }
}

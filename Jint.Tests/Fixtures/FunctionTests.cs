using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class FunctionTests : FixturesFixture
    {
        public FunctionTests()
            : base(null)
        {
        }

        [TestCase("Function.js")]
        public void ShouldRunFunctionTests(string script)
        {
            RunFile(script);
        }
    }
}

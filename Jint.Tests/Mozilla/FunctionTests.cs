using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class FunctionTests : MozillaFixture
    {
        public FunctionTests()
            : base("Function")
        {
        }

        [TestCase("15.3.4.3-1.js")]
        [TestCase("15.3.4.4-1.js")]
        [TestCase("arguments-001.js")]
        [TestCase("arguments-002.js")]
        [TestCase("call-001.js")]
        [TestCase("regress-131964.js")]
        [TestCase("regress-137181.js")]
        [TestCase("regress-193555.js")]
        [TestCase("regress-313570.js")]
        [TestCase("regress-49286.js")]
        [TestCase("regress-58274.js")]
        [TestCase("regress-85880.js")]
        [TestCase("regress-94506.js")]
        [TestCase("regress-97921.js")]
        [TestCase("scope-001.js")]
        [TestCase("scope-002.js")]
        public void ShouldRunFunctionTests(string script)
        {
            RunFile(script);
        }
    }
}

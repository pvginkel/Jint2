using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ExecutionContextsTests : MozillaFixture
    {
        public ExecutionContextsTests()
            : base("ExecutionContexts")
        {
        }

        [TestCase("10.1.3.js")]
        [TestCase("10.1.3-1.js")]
        [TestCase("10.1.3-2.js")]
        [TestCase("10.1.4-1.js")]
        [TestCase("10.6.1-01.js")]
        [TestCase("regress-23346.js")]
        [TestCase("regress-448595-01.js")]
        public void ShouldRunExecutionContextsTest(string script)
        {
            RunFile(script);
        }
    }
}

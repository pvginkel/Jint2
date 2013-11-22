using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ExceptionTests : MozillaFixture
    {
        public ExceptionTests()
            : base("Exception")
        {
        }

        [TestCase("15.11.1.1.js")]
        [TestCase("15.11.4.4-1.js")]
        [TestCase("15.11.7.6-001.js")]
        [TestCase("15.11.7.6-002.js")]
        [TestCase("15.11.7.6-003.js")]
        [TestCase("binding-001.js")]
        [TestCase("regress-181654.js")]
        [TestCase("regress-181914.js")]
        [TestCase("regress-58946.js")]
        [TestCase("regress-95101.js")]
        public void ShouldRunExceptionTests(string script)
        {
            RunFile(script);
        }
    }
}

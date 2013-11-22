using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class DateTests : MozillaFixture
    {
        public DateTests()
            : base("Date")
        {
        }

        [TestCase("15.9.1.2-01.js")]
        [TestCase("15.9.3.2-1.js")]
        [TestCase("15.9.4.3.js")]
        [TestCase("15.9.5.3.js")]
        [TestCase("15.9.5.4.js")]
        [TestCase("15.9.5.5.js")]
        [TestCase("15.9.5.5-02.js")]
        [TestCase("15.9.5.6.js")]
        [TestCase("15.9.5.7.js")]
        [TestCase("regress-452786.js")]
        public void ShouldRunDateTests(string script)
        {
            RunFile(script);
        }
    }
}

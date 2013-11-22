using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class NumberFormattingTests : MozillaFixture
    {
        public NumberFormattingTests()
            : base("NumberFormatting")
        {
        }

        [TestCase("tostring-001.js")]
        public void ShouldRunNumberFormattingTests(string script)
        {
            RunFile(script);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class LexicalConventionsTests : MozillaFixture
    {
        public LexicalConventionsTests()
            : base("LexicalConventions")
        {
        }

        [TestCase("7.4-01.js")]
        [TestCase("7.9.1.js")]
        public void ShouldRunLexicalConventionsTests(string script)
        {
            RunFile(script);
        }
    }
}

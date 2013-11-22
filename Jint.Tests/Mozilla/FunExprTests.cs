using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class FunExprTests : MozillaFixture
    {
        public FunExprTests()
            : base("FunExpr")
        {
        }

        [TestCase("fe-001.js")]
        [TestCase("fe-001-n.js")]
        [TestCase("fe-002.js")]
        public void ShouldRunFunExprTests(string script)
        {
            RunFile(script);
        }
    }
}

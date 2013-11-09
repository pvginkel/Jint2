using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests
{
    [TestFixture]
    public class MissingFeaturesFixture
    {
        private JintEngine CreateEngine()
        {
            return new JintEngine();
        }

        private object Test(string code)
        {
            return CreateEngine().Run(code);
        }

        [Test]
        public void IncrementIdentifier()
        {
            Test(@"i++;");
            Test(@"++i;");
            Test(@"i--;");
            Test(@"--i;");
            Assert.AreEqual(2d, Test(@"i = 1; i.j = 1; i.j++; return i.j;"));
            Assert.AreEqual(2d, Test(@"i = 1; i.j = 1; ++i.j; return i.j;"));
        }

        [Test]
        public void BooleanLogic()
        {
            Test(
@"
var i = null;
var j = null;

if (!i && j) {}
");
        }
    }
}

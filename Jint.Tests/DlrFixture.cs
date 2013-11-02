using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests
{
    [TestFixture]
    public class DlrFixture
    {
        private JintEngine CreateEngine()
        {
            return new JintEngine(Options.EcmaScript5 | Options.Strict, JintBackend.Dlr);
        }

        private object Test(string code)
        {
            return CreateEngine().Run(code);
        }

        [Test]
        public void VariableDeclaration()
        {
            Assert.AreEqual(7d, Test(@"var i = 7;"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class DeclarationTests : FixturesFixture
    {
        public DeclarationTests()
            : base(null)
        {
        }

        [TestCase("Declaration.js")]
        public void ShouldRunDeclarationTests(string script)
        {
            RunFile(script);
        }

        protected override JintEngine CreateContext(Action<string> errorAction)
        {
            return CreateContext(errorAction, false);
        }
    }
}

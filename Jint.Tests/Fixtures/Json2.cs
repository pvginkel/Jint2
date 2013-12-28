using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class UnderscoreTests : FixturesFixture
    {
        public UnderscoreTests()
            : base("Underscore")
        {
        }

        protected override object RunFile(JintEngine ctx, string fileName)
        {
            base.RunFile(ctx, fileName);
            
            string suitePath = Path.Combine(
                Path.GetDirectoryName(fileName),
                "underscore-suite.js"
            );

            return ctx.ExecuteFile(suitePath);
        }

        [TestCase("underscore.js")]
        [TestCase("underscore-min.js")]
        public void ShouldRunUnderscoreTests(string script)
        {
            RunFile(script);
        }
    }
}

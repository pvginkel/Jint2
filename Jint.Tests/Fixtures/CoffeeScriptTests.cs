using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    [Ignore("These take very long to run and currently fail")]
    public class CoffeeScriptTests : FixturesFixture
    {
        public CoffeeScriptTests()
            : base("CoffeeScript")
        {
        }

        protected override object RunFile(JintEngine ctx, string fileName)
        {
            base.RunFile(ctx, fileName);
            
            string suitePath = Path.Combine(
                Path.GetDirectoryName(fileName),
                "coffeescript-suite.js"
            );

            return ctx.Run(File.ReadAllText(suitePath));
        }

        [TestCase("coffeescript.js")]
        [TestCase("coffeescript-min.js")]
        public void ShouldRunCoffeeScriptTests(string script)
        {
            RunFile(script);
        }
    }
}

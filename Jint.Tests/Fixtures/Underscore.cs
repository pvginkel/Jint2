using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class Json2Tests : FixturesFixture
    {
        public Json2Tests()
            : base("Json2")
        {
        }

        [TestCase("json2.js")]
        public void ShouldRunJson2Tests(string script)
        {
            RunFile(script);
        }
    }
}

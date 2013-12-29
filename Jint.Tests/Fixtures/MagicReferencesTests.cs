using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class MagicReferencesTests : FixturesFixture
    {
        public MagicReferencesTests()
            : base(null)
        {
        }

        [TestCase("MagicReferences.js")]
        public void ShouldRunMagicReferencesTests(string script)
        {
            RunFile(script);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class FlowTests : FixturesFixture
    {
        public FlowTests()
            : base(null)
        {
        }

        [TestCase("Flow.js")]
        public void ShouldRunFlowTests(string script)
        {
            RunFile(script);
        }
    }
}

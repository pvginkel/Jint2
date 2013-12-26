using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class EvalTests : FixturesFixture
    {
        public EvalTests()
            : base(null)
        {
        }

        [TestCase("Eval.js")]
        public void ShouldRunEvalTests(string script)
        {
            RunFile(script);
        }

        protected override JintEngine CreateContext(Action<string> errorAction)
        {
            return CreateContext(errorAction, false);
        }
    }
}

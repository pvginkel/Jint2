﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class MathTests : FixturesFixture
    {
        public MathTests()
            : base(null)
        {
        }

        [TestCase("Math.js")]
        public void ShouldRunMathTests(string script)
        {
            RunFile(script);
        }
    }
}

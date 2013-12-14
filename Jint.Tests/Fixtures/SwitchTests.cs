﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class SwitchTests : FixturesFixture
    {
        public SwitchTests()
            : base(null)
        {
        }

        [TestCase("Switch.js")]
        public void ShouldSwitchTests(string script)
        {
            RunFile(script);
        }
    }
}

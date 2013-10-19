﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Performance
{
    [TestFixture]
    public class PerformanceFixture
    {
        [Test]
        public void SimpleTest()
        {
            var engine = new JintEngine();

            engine.Run("1 + 3;");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Native;
using NUnit.Framework;

namespace Jint.Tests.Performance
{
    [TestFixture]
    public class PerformanceFixture
    {
        [Test]
        public void SimpleTest()
        {
            var engine = new JintEngine(Options.EcmaScript5 | Options.Strict, JintBackend.Compiled);

            string code;

            using (var stream = GetType().Assembly.GetManifestResourceStream("Jint.Tests.SunSpider.access-fannkuch.js"))
            using (var reader = new StreamReader(stream))
            {
                code = reader.ReadToEnd();
            }

            var result = engine.Run(code);
//@"
//true && false;
//var i = true && false;
//return i;
//");
        }

        [Test]
        public void SimpleMethod()
        {
            var engine = new JintEngine(Options.EcmaScript5 | Options.Strict, JintBackend.Compiled);

            var result = engine.Run(
@"
function myFunction(j, k) {
    return j;
}

true && false;
var i = true && false;
return myFunction(i);
");

            Assert.AreEqual(false, result);
        }

        [Test]
        public void Globals()
        {
            var engine = new JintEngine(Options.EcmaScript5 | Options.Strict, JintBackend.Compiled);

            var result = engine.Run(
@"
i = 0;
var j = i;
function f() {
    i = 3;
    j = 4;
}
");
        }

        [Test]
        public void ForEach()
        {
            var engine = new JintEngine(Options.EcmaScript5 | Options.Strict, JintBackend.Compiled);

            engine.Run(
@"
for (var x in y) {
}
");
        }
    }
}

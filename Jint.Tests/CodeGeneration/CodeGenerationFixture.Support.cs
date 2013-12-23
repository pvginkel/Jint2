using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Compiler;
using Jint.Native;
using NUnit.Framework;
using TypeMarkerPhase = Jint.Bound.TypeMarkerPhase;

namespace Jint.Tests.CodeGeneration
{
    partial class CodeGenerationFixture
    {
        private static void Test(object expected, string script)
        {
            Assert.AreEqual(
                expected,
                new JintEngine().Run(script)
            );
        }
    }
}

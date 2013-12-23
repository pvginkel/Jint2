using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Native;
using NUnit.Framework;

namespace Jint.Tests.CodeGeneration
{
    [TestFixture]
    partial class CodeGenerationFixture
    {
        [Test]
        public void SimpleBody()
        {
            Test(
                7d,
@"
return 7;
"
            );
        }

        [Test]
        public void SetGlobal()
        {
            Test(
                7d,
@"
var i = 7;
return i;
"
            );
        }

        [Test]
        public void Fannkuch()
        {
            Test(JsUndefined.Instance, File.ReadAllText(@"..\..\SunSpider\Tests\access-fannkuch.js"));
        }
    }
}

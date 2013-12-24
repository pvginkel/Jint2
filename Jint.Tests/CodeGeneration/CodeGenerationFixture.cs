using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.CodeGeneration
{
    [TestFixture]
    partial class CodeGenerationFixture : TestBase
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
        public void NewFunction()
        {
            Test(
                7d,
@"
var f = new Function('x', 'return x * 3.5');
return f(2);
"
            );
        }

        [Test]
        public void FunctionResultSquelching()
        {
            Test(
                null,
@"
function f() { }
f();
"
            );
        }
    }
}

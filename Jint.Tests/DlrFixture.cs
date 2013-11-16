using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests
{
    [TestFixture]
    public class DlrFixture : TestBase
    {
        [Test]
        public void VariableDeclaration()
        {
            Assert.AreEqual(7d, Test(@"var i = 7;"));
        }

        [Test]
        public void ShouldCloseOverVariables()
        {
            Test(
@"
var i = 1; // This one isn't closed over because it's of the global this.

function f() {
  i++;
  var j = 2; // This one is closed over.
  
  var g = function() {
    i++;
    j++;
    var k = 3; // This one is closed over.
    
    var h = function() {
      i++;
      j++; // Closed over from f.
      k++; // Closed over from g.
    };
    
    h();

    assert(k, 4);
  };
  
  g();
  
  assert(j, 4);
}

f();

assert(i, 4);
");
        }

        [Test]
        public void ShouldCloseOverParameter()
        {
            Test(
@"
function f(v) {
    function g() {
        assert(v, 7);
    }

  g();
}

f(7);
"
            );
        }

        [Test]
        public void DummyTest()
        {
            Test(
@"
test1: for (var i = 0; i < 3; i++) {
    test2: for (var j = 0; j < 3; j++) {
        break test1; 
    }
}

assert(0, i);
assert(0, j);
"
            );
        }
    }
}

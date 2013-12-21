using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.TypeMarking
{
    [TestFixture]
    public partial class TypeMarkingFixture : TestBase
    {
        [Test]
        public void AssignmentToNumber()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Number }
                },
@"
var i = 0;
"
            );
        }

        [Test]
        public void AssignmentToBoolean()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Boolean }
                },
@"
var i = true;
"
            );
        }

        [Test]
        public void AssignmentToObject()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object }
                },
@"
var i = {};
"
            );
        }

        [Test]
        public void AssignmentToString()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.String }
                },
@"
var i = '';
"
            );
        }

        [Test]
        public void AssignmentToRegex()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object }
                },
@"
var i = /a/;
"
            );
        }

        [Test]
        public void MultipleSameAssignments()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Number }
                },
@"
var i = 0;
i = 1;
"
            );
        }

        [Test]
        public void MultipleDifferentAssignments()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Unknown }
                },
@"
var i = 0;
i = null;
"
            );
        }

        [Test]
        public void CallGivesUnknown()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Unknown },
                    { "f", BoundValueType.Object }
                },
@"
function f() { }
var i = f();
"
            );
        }

        [Test]
        public void NewGivesObject()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object },
                    { "f", BoundValueType.Object }
                },
@"
function f() { }
var i = new f();
"
            );
        }

        [Test]
        public void PropertyAndIndexGiveUnknown()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object },
                    { "j", BoundValueType.Unknown },
                    { "k", BoundValueType.Unknown },
                    { "l", BoundValueType.Unknown }
                },
@"
var i = {};
var j = i.p;
var k = i['p'];
var l = i[0];
"
            );
        }

        [Test]
        public void DifferentAssignmentsDifferentBranches()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Unknown }
                },
@"
if (true) {
    var i = 1;
} else {
    var i = null;
}
"
            );
        }

        [Test]
        public void StringAddResultsInString()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Number },
                    { "j", BoundValueType.String },
                    { "k", BoundValueType.String },
                    { "l", BoundValueType.Number }
                },
@"
var i = 7;
var j = '';
var k = i + j;
var l = i + 3;
"
            );
        }

        [Test]
        public void BinariesThatForceNumeric()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.String },
                    { "j", BoundValueType.String },
                    { "k", BoundValueType.Number }
                },
@"
var i = '';
var j = '';
var k;

k = i & j;
k = i ^ j;
k = i | j;
k = i / j;
k = i << j;
k = i >> j;
k = i >>> j;
k = i % j;
k = i * j;
k = i - j;
"
            );
        }

        [Test]
        public void BinariesThatForceBoolean()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.String },
                    { "j", BoundValueType.String },
                    { "k", BoundValueType.Boolean }
                },
@"
var i = '';
var j = '';
var k;

k = i == j;
k = i != j;
k = i === j;
k = i !== j;
k = i < j;
k = i <= j;
k = i > j;
k = i >= j;
k = i in j;
k = i instanceof j;
"
            );
        }

        [Test]
        public void UnariesThatForceNumeric()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.String },
                    { "j", BoundValueType.Number }
                },
@"
var i = '';
var j;

j = ~i;
j = -i;
j = +i;
"
            );
        }

        [Test]
        public void UnariesThatForceBoolean()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.String },
                    { "j", BoundValueType.Boolean }
                },
@"
var i = '';
var j;

j = !i;
j = delete i.p;
"
            );
        }

        [Test]
        public void UnariesThatForceString()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object },
                    { "j", BoundValueType.String }
                },
@"
var i = {};
var j;

j = typeof i;
"
            );
        }

        [Test]
        public void UnariesThatForceUnknown()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Object },
                    { "j", BoundValueType.Unknown }
                },
@"
var i = {};
var j;

j = void(i);
"
            );
        }

        [Test]
        public void ConditionalAssignment()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Number },
                    { "j", BoundValueType.Unknown },
                    { "k", BoundValueType.Number }
                },
@"
var i = 0;
var j = i > 0 ? true : '';
var k = i > 0 ? 1 : 7;
"
            );
        }

        [Test]
        public void SpecialLogicalConstructs()
        {
            TestBody(
                new Dictionary<string, BoundValueType>
                {
                    { "i", BoundValueType.Unknown },
                    { "j", BoundValueType.Number },
                    { "k", BoundValueType.Unknown },
                    { "l", BoundValueType.Number }
                },
@"
var i = 1 && '';
var j = 1 && 2;
var k = 1 || '';
var l = 1 || 2;
"
            );
        }
    }
}

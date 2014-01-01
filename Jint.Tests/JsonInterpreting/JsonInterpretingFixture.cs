using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.JsonInterpreting
{
    [TestFixture]
    public partial class JsonInterpretingFixture : TestBase
    {
        [Test]
        public void EmptyBody()
        {
            Assert.IsTrue(Test(
                @"",
                @"undefined"
            ));
        }

        [Test]
        public void BodyWithSingleComment()
        {
            Assert.IsTrue(Test(
                @"// comment",
                @"undefined"
            ));
        }

        [TestCase("'hello'")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("undefined")]
        [TestCase("null")]
        [TestCase("7")]
        [TestCase("7.1")]
        public void SimpleConstant(string script)
        {
            Assert.IsTrue(Test(script));
        }

        [TestCase("[]")]
        [TestCase("[1,2]")]
        [TestCase("[1,[2,3]]")]
        public void Arrays(string script)
        {
            Assert.IsTrue(Test(script));
        }

        [TestCase("{}")]
        [TestCase("{'a':1,'b':2}")]
        [TestCase("{'1':2,'3':4}")]
        [TestCase("{'a':{'1':2,'3':4},'b':false}")]
        public void Objects(string script)
        {
            Assert.IsTrue(Test("return " + script, script));
        }

        [TestCase("[{'a':[1,2,3,{'b':'c','d':true},5],'c':undefined}]")]
        public void Compounds(string script)
        {
            Assert.IsTrue(Test("return " + script, script));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests
{
    public class TestBase
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        protected object Test(Options options, string script)
        {
            return Test(options, script, jint => { });
        }

        protected object Test(string script)
        {
            return Test(Options.EcmaScript3 | Options.Strict, script);
        }

        protected object Test(string script, Action<JintEngine> action)
        {
            return Test(Options.EcmaScript3 | Options.Strict, script, action);
        }

        protected object Test(Options options, string script, Action<JintEngine> action)
        {
            var jint = new JintEngine()
                .AllowClr()
                .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
                .SetFunction("fail", new Action<string>(Assert.Fail))
                .SetFunction("istrue", new Action<bool>(Assert.IsTrue))
                .SetFunction("isfalse", new Action<bool>(Assert.IsFalse))
                .SetFunction("print", new Action<string>(Console.WriteLine))
                .SetFunction("alert", new Action<string>(Console.WriteLine))
                .SetFunction("loadAssembly", new Action<string>(assemblyName => Assembly.Load(assemblyName)))
                .DisableSecurity();

            action(jint);

            var sw = new Stopwatch();
            sw.Start();

            var result = jint.Run(script);

            Console.WriteLine(sw.Elapsed);

            return result;
        }
    }
}

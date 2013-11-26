using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public abstract class FixturesFixture : TestBase
    {
        private static readonly string _basePath;
        private static readonly string _libPath;
        private static readonly Dictionary<string, string> _includeCache = new Dictionary<string, string>();

        protected override string BasePath
        {
            get { return _basePath; }
        }

        static FixturesFixture()
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(typeof(FixturesFixture).Assembly.Location));

            _basePath = assemblyDirectory.Parent.Parent.FullName;
            _libPath = Path.Combine(_basePath, "Fixtures", "Lib");
            _basePath = Path.Combine(_basePath, "Fixtures", "Tests");
        }

        protected override string GetInclude(string file)
        {
            string source;

            if (!_includeCache.TryGetValue(file, out source))
            {
                source =
                    GetSpecialInclude(file) ??
                    File.ReadAllText(Path.Combine(_libPath, file));

                _includeCache.Add(file, source);
            }

            return source;
        }

        protected override JintEngine CreateContext(Action<string> errorAction)
        {
            return CreateContext(errorAction, true);
        }

        protected JintEngine CreateContext(Action<string> errorAction, bool allowClr)
        {
            return CreateContext(errorAction, allowClr, Options.EcmaScript5 | Options.Strict);
        }

        protected JintEngine CreateContext(Action<string> errorAction, bool allowClr, Options options)
        {
            var ctx = CreateContext(errorAction, options);

            if (allowClr)
                ctx.AllowClr();

            ctx.SetFunction("assert", new Action<object, object, string>(Assert.AreEqual));
            ctx.SetFunction("fail", new Action<string>(Assert.Fail));
            ctx.SetFunction("istrue", new Action<bool>(Assert.IsTrue));
            ctx.SetFunction("isfalse", new Action<bool>(Assert.IsFalse));
            ctx.SetFunction("print", new Action<string>(Console.WriteLine));
            ctx.SetFunction("alert", new Action<string>(Console.WriteLine));
            ctx.SetFunction("loadAssembly", new Action<string>(assemblyName => Assembly.Load(assemblyName)));
            ctx.DisableSecurity();

            return ctx;
        }

        protected FixturesFixture(string testsPath)
            : base(testsPath)
        {
        }
    }
}

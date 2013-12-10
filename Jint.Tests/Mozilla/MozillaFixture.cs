using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Tests.Support;

namespace Jint.Tests.Mozilla
{
    public abstract class MozillaFixture : TestBase
    {
        private static readonly string _basePath;
        private static readonly string _libPath;
        private static readonly Dictionary<string, string> _includeCache = new Dictionary<string, string>();

        protected override string BasePath
        {
            get { return _basePath; }
        }

        static MozillaFixture()
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(typeof(TestBase).Assembly.Location));

            _basePath = assemblyDirectory.Parent.Parent.FullName;
            _libPath = Path.Combine(_basePath, "Mozilla", "Lib");
            _basePath = Path.Combine(_basePath, "Mozilla", "Tests");
        }

        protected override void RunInclude(JintEngine engine, string fileName)
        {
            string source;

            if (!_includeCache.TryGetValue(fileName, out source))
            {
                source =
                    GetSpecialInclude(fileName) ??
                    File.ReadAllText(Path.Combine(_libPath, fileName));

                _includeCache.Add(fileName, source);
            }

            engine.Run(source, fileName);
        }

        public MozillaFixture(string testsPath)
            : base(testsPath)
        {
        }

        protected override object RunFile(JintEngine ctx, string fileName)
        {
            string shellPath = Path.Combine(
                Path.GetDirectoryName(fileName),
                "shell.js"
            );

            if (File.Exists(shellPath))
                ctx.Run(File.ReadAllText(shellPath), shellPath);

            return base.RunFile(ctx, fileName);
        }

        protected override JintEngine CreateContext(Action<string> errorAction)
        {
            var engine = base.CreateContext(errorAction);

            engine.DisableSecurity();
            RunInclude(engine, "shell.js");
            RunInclude(engine, "environment.js");

            engine.SetFunction("print", new Action<string>(e =>
            {
                if (e.Contains("FAILED"))
                    errorAction(e);
                else
                    Console.WriteLine(e);
            }));

            return engine;
        }
    }
}

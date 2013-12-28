using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.SunSpider
{
    public class SunSpiderFixture : ScriptTestBase
    {
        private static readonly string _basePath;
        private static readonly string _libPath;
        private static readonly Dictionary<string, string> _includeCache = new Dictionary<string, string>();

        protected override string BasePath
        {
            get { return _basePath; }
        }

        static SunSpiderFixture()
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(typeof(ScriptTestBase).Assembly.Location));

            _basePath = assemblyDirectory.Parent.Parent.FullName;
            _libPath = Path.Combine(_basePath, "SunSpider", "Lib");
            _basePath = Path.Combine(_basePath, "SunSpider", "Tests");
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

            engine.Execute(source, fileName);
        }

        public SunSpiderFixture()
            : base(null)
        {
        }

        [TestCase("3d-cube.js")]
        [TestCase("3d-morph.js")]
        [TestCase("3d-raytrace.js")]
        [TestCase("access-binary-trees.js")]
        [TestCase("access-fannkuch.js")]
        [TestCase("access-nbody.js")]
        [TestCase("access-nsieve.js")]
        [TestCase("bitops-3bit-bits-in-byte.js")]
        [TestCase("bitops-bits-in-byte.js")]
        [TestCase("bitops-bitwise-and.js")]
        [TestCase("bitops-nsieve-bits.js")]
        [TestCase("controlflow-recursive.js")]
        [TestCase("crypto-aes.js")]
        [TestCase("crypto-md5.js")]
        [TestCase("crypto-sha1.js")]
        [TestCase("date-format-tofte.js")]
        [TestCase("date-format-xparb.js")]
        [TestCase("math-cordic.js")]
        [TestCase("math-partial-sums.js")]
        [TestCase("math-spectral-norm.js")]
        [TestCase("regexp-dna.js")]
        [TestCase("string-base64.js")]
        [TestCase("string-fasta.js")]
        [TestCase("string-tagcloud.js")]
        [TestCase("string-unpack-code.js")]
        [TestCase("string-validate-input.js")]
        public void ShouldRunSunSpiderScript(string script)
        {
            RunFile(script);
        }

        protected override JintEngine CreateContext(Action<string> errorAction)
        {
            var result = base.CreateContext(errorAction);
            result.DisableSecurity();
            return result;
        }
    }
}

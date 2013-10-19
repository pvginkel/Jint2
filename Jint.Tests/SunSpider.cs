using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Jint.Tests {
    /// <summary>
    /// Summary description for SunSpider
    /// </summary>
    [TestFixture]
    public class SunSpider {

        private static void ExecuteSunSpiderScript(string scriptName)
        {
            const string prefix = "Jint.Tests.SunSpider.";
            var script = prefix + scriptName;

            var assembly = Assembly.GetExecutingAssembly();
            var program = new StreamReader(assembly.GetManifestResourceStream(script)).ReadToEnd();

            var jint = new JintEngine();
            var sw = new Stopwatch();
            sw.Start();

            jint.Run(program);

            Console.WriteLine(sw.Elapsed);
        }

                

        [Test]
        public void ShouldRun3DCube()
        {
            ExecuteSunSpiderScript("3d-cube.js");
        }

        [Test]
        public void ShouldRun3DMorph()
        {
            ExecuteSunSpiderScript("3d-morph.js");
        }

        [Test]
        public void ShouldRun3DRaytrace()
        {
            ExecuteSunSpiderScript("3d-raytrace.js");
        }

        [Test]
        public void ShouldRunAccessBinaryTrees()
        {
            ExecuteSunSpiderScript("access-binary-trees.js");
        }

        [Test]
        public void ShouldRunAccessFannkuch()
        {
            ExecuteSunSpiderScript("access-fannkuch.js");
        }

        [Test]
        public void ShouldRunAccessNbody()
        {
            ExecuteSunSpiderScript("access-nbody.js");
        }

        [Test]
        public void ShouldRunAccessNsieve()
        {
            ExecuteSunSpiderScript("access-nsieve.js");
        }

        [Test]
        public void ShouldRunBitops3BitsInByte()
        {
            ExecuteSunSpiderScript("bitops-3bit-bits-in-byte.js");
        }

        [Test]
        public void ShouldRunBitopsBitsInByte()
        {
            ExecuteSunSpiderScript("bitops-bits-in-byte.js");
        }

        [Test]
        public void ShouldRunBitopsBitwiseAnd()
        {
            ExecuteSunSpiderScript("bitops-bitwise-and.js");
        }

        [Test]
        public void ShouldRunBitopsNsieveBits()
        {
            ExecuteSunSpiderScript("bitops-nsieve-bits.js");
        }

        [Test]
        public void ShouldRunControlflowRecurise()
        {
            ExecuteSunSpiderScript("controlflow-recursive.js");
        }

        [Test]
        public void ShouldRunCryptoAes()
        {
            ExecuteSunSpiderScript("crypto-aes.js");
        }

        [Test]
        public void ShouldRunCrypotMd5()
        {
            ExecuteSunSpiderScript("crypto-md5.js");
        }
        
        [Test]
        public void ShouldRunCruptoSha1()
        {
            ExecuteSunSpiderScript("crypto-sha1.js");
        }

        [Test]
        public void ShouldRunDateFormatTofte()
        {
            ExecuteSunSpiderScript("date-format-tofte.js");
        }

        [Test]
        public void ShouldRunDateFormatXparb()
        {
            ExecuteSunSpiderScript("date-format-xparb.js");
        }

        [Test]
        public void ShouldRunMathCrodic()
        {
            ExecuteSunSpiderScript("math-cordic.js");
        }

        [Test]
        public void ShouldRunMathPartialSums()
        {
            ExecuteSunSpiderScript("math-partial-sums.js");
        }

        [Test]
        public void ShouldRunMathSpecialNorm()
        {
            ExecuteSunSpiderScript("math-spectral-norm.js");
        }

        [Test]
        public void ShouldRunRegexpDna()
        {
            ExecuteSunSpiderScript("regexp-dna.js");
        }

        [Test]
        public void ShouldRunStringBase64()
        {
            ExecuteSunSpiderScript("string-base64.js");
        }
        

        [Test]
        public void ShouldRunStinFasta()
        {
            ExecuteSunSpiderScript("string-fasta.js");
        }

                [Test]
        public void ShouldRunStringTagcloud()
        {
            ExecuteSunSpiderScript("string-tagcloud.js");
        }

                [Test]
        public void ShouldRunStringUnpackCode()
        {
            ExecuteSunSpiderScript("string-unpack-code.js");
        }

        [Test]
        public void ShouldRunStringValidateInput()
        {
            ExecuteSunSpiderScript("string-validate-input.js");
        }

    }
}

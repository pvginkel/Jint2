using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Compiler;
using Jint.Native;
using NUnit.Framework;

namespace Jint.Tests.JsonInterpreting
{
    partial class JsonInterpretingFixture
    {
        private static bool Test(string script)
        {
            return Test(script, script);
        }

        private static bool Test(string script, string expected)
        {
            object result = Compile(script);
            if (result == null)
                return false;

            string actual;

            using (var writer = new StringWriter())
            {
                JsonPrinter.Print(writer, result);
                actual = writer.GetStringBuilder().ToString();
            }

            Assert.AreEqual(expected, actual);

            return true;
        }

        private static object Compile(string script)
        {
            var program = JintEngine.ParseProgram(script);

            if (program == null)
                return JsUndefined.Instance;

            program.Accept(new VariableMarkerPhase());

            var typeSystem = new TypeSystem();
            var scriptBuilder = typeSystem.CreateScriptBuilder(null);
            var bindingVisitor = new BindingVisitor(scriptBuilder);

            program.Accept(bindingVisitor);

            var boundProgram = bindingVisitor.Program;

            var interpreter = new JsonInterpreter(new JintEngine().Global);
            if (boundProgram.Body.Accept(interpreter))
                return interpreter.Result;

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Compiler;
using Jint.Native;
using NUnit.Framework;
using TypeMarkerPhase = Jint.Bound.TypeMarkerPhase;

namespace Jint.Tests.CodeGeneration
{
    partial class CodeGenerationFixture
    {
        private static void Test(object expected, string script)
        {
            var engine = new JintEngine();

            object actual = Compile(script, engine)(new JintRuntime(engine, engine.Options));

            Assert.AreEqual(expected, actual);
        }

        private static Func<JintRuntime, object> Compile(string script, JintEngine engine)
        {
            var programSyntax = JintEngine.Compile(script);

            programSyntax.Accept(new VariableMarkerPhase(new JintEngine()));
            programSyntax.Accept(new Compiler.TypeMarkerPhase());

            var visitor = new BindingVisitor();

            programSyntax.Accept(visitor);

            var program = SquelchPhase.Perform(visitor.Program);

            DefiniteAssignmentPhase.Perform(program);
            TypeMarkerPhase.Perform(program);

            return new CodeGenerator(engine).BuildMainMethod(program);
        }
    }
}

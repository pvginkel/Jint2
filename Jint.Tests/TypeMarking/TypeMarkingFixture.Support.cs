using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Compiler;
using NUnit.Framework;
using TypeMarkerPhase = Jint.Bound.TypeMarkerPhase;

namespace Jint.Tests.TypeMarking
{
    partial class TypeMarkingFixture
    {
        private static void TestBody(Dictionary<string, BoundValueType> expected, string script)
        {
            // We wrap the whole thing in a function because otherwise we don't
            // have locals.

            script = "function __body() {" + Environment.NewLine + script + Environment.NewLine + "}";

            var program = Compile(script);

            var function = FunctionGatherer.Gather(program.Body).Single(p => p.Name == "__body");

            var actual = function.Body.Locals.ToDictionary(p => p.Name, p => p.ValueType);

            if (function.Body.Closure != null)
            {
                foreach (var field in function.Body.Closure.Fields)
                {
                    actual.Add(field.Name, field.ValueType);
                }
            }

            actual.Remove("arguments");

            Assert.AreEqual(FormatVariables(expected), FormatVariables(actual));
        }

        private static object FormatVariables(Dictionary<string, BoundValueType> variables)
        {
            var sb = new StringBuilder();

            foreach (var item in variables.OrderBy(p => p.Key))
            {
                sb.AppendLine(String.Format("{0}: {1}", item.Key, item.Value));
            }

            return sb.ToString();
        }

        private static BoundProgram Compile(string script)
        {
            var programSyntax = JintEngine.Compile(script);

            programSyntax.Accept(new VariableMarkerPhase(new JintEngine()));

            var visitor = new BindingVisitor();

            programSyntax.Accept(visitor);

            var program = SquelchPhase.Perform(visitor.Program);
            DefiniteAssignmentPhase.Perform(program);
            TypeMarkerPhase.Perform(program);
            return program;
        }
    }
}

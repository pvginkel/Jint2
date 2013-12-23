using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Compiler;
using NUnit.Framework;

namespace Jint.Tests.DefiniteAssignment
{
    partial class DefiniteAssignmentFixture
    {
        private static void TestBody(IEnumerable<string> variables, string script)
        {
            // We wrap the whole thing in a function because otherwise we don't
            // have locals.

            script = "function __body() {" + Environment.NewLine + script + Environment.NewLine + "}";

            var program = Compile(script);

            var function = FunctionGatherer.Gather(program).Single(p => p.Name == "__body");

            var definitelyAssigned = function.Body.TypeManager.Types.Where(p => p.DefinitelyAssigned).ToList();
            var assignedLocals = function.Body.Locals.Where(p => definitelyAssigned.Contains(p.Type)).Select(p => p.Name).ToList();
            if (function.Body.Closure != null)
                assignedLocals.AddRange(function.Body.Closure.Fields.Where(p => p.Type.DefinitelyAssigned).Select(p => p.Name));

            // Ignore the arguments local; it's always assigned to.

            assignedLocals.Remove("arguments");

            Assert.That(assignedLocals, Is.EquivalentTo(variables));
        }

        private static BoundProgram Compile(string script)
        {
            var programSyntax = JintEngine.Compile(script);

            programSyntax.Accept(new VariableMarkerPhase(new JintEngine()));

            var visitor = new BindingVisitor();

            programSyntax.Accept(visitor);

            var program = SquelchPhase.Perform(visitor.Program);
            DefiniteAssignmentPhase.Perform(program);
            return program;
        }
    }
}

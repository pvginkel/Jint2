using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Bound;
using Jint.Tests.Support;
using NUnit.Framework;

namespace Jint.Tests.DefiniteAssignment
{
    [TestFixture]
    public partial class DefiniteAssignmentFixture : TestBase
    {
        [Test]
        public void EmptyBody()
        {
            TestBody(
                new string[0],
                "// No body"
            );
        }

        [Test]
        public void SimpleBody()
        {
            TestBody(
                new[] { "i" },
@"
var i;
i = 0;
"
            );
        }

        [Test]
        public void SimpleBodyUnassignedNoRead()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i;
i = 0;
var j;
"
            );
        }

        [Test]
        public void SimpleBodyUnassignedRead()
        {
            TestBody(
                new[] { "i" },
@"
var i;
i = 0;
var j;
i = j;
"
            );
        }

        [Test]
        public void IfWithoutRead()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
if (i > 1) {
    var j = 0;
}
"
            );
        }

        [Test]
        public void IfWithUninitializedRead()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
if (i > 1) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void IfWithAllBranchesAssigned()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
if (i > 1) {
    var j = 0;
} else {
    var j = 1;
}
i = j;
"
            );
        }

        [Test]
        public void IfWithElseWithUninitializedRead()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
if (i > 1) {
    var j = 0;
} else if (i > 2) {
    var j = 1;
}
i = j;
"
            );
        }

        [Test]
        public void IfWithConstantTrue()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
if (true) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void IfWithConstantFalse()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
if (false) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void WhileWithUninitializedRead()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
while (i > 0) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void WhileWithConstantTrue()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void WhileWithConstantFalse()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
while (false) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void DoWhileWithInitializedRead()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
do {
    var j = 0;
} while (i > 0);
i = j;
"
            );
        }

        [Test]
        public void DoWhileWithInitializeInExpression()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
do {
} while ((j = i) > 0);
i = j;
"
            );
        }

        [Test]
        public void DoWhileWithInitializeInExpressionWithBreak()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
var j;
do {
    break;
} while ((j = i) > 0);
i = j;
"
            );
        }

        [Test]
        public void DoWhileWithInitializeInExpressionWithContinue()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
do {
    continue;
} while ((j = i) > 0);
i = j;
"
            );
        }

        [Test]
        public void ForWithInitialization()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; false; ) {
}
"
            );
        }

        [Test]
        public void ForWithUninitializedRead()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; i > 0; ) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ForWithUninitializedReadInIncrement()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; i > 0; j++) {
}
i = j;
"
            );
        }

        [Test]
        public void ForWithInitializeInTest()
        {
            TestBody(
                new[] { "i", "j" },
@"
var j;
for (var i = 0; (j = i) > 0; ) {
}
i = j;
"
            );
        }

        [Test]
        public void ForWithConstantTrue()
        {
            TestBody(
                new[] { "i", "j" },
@"
for (var i = 0; true; ) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ForWithMissingTest()
        {
            TestBody(
                new[] { "i", "j" },
@"
for (var i = 0; ; ) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ForWithConstantFalse()
        {
            TestBody(
                new[] { "i", "j" },
@"
for (var i = 0; true; ) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ForIncrementDoesNotExecuteOnBreak()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; i > 0; j = i) {
    break;
}
i = j;
"
            );
        }

        [Test]
        public void ForIncrementDoesExecuteOnContinue()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; i > 0; j = i) {
    continue;
}
i = j;
"
            );
        }

        [Test]
        public void ForIncrementDoesNotExecuteOnBreakInAllBranches()
        {
            TestBody(
                new[] { "i" },
@"
for (var i = 0; i > 0; j = i) {
    if (i > 0) {
        break;
    } else {
        break;
    }
}
i = j;
"
            );
        }

        [Test]
        public void ForEachInWithUninitializedRead()
        {
            TestBody(
                new[] { "i" },
@"
for (var i in null) {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ForEachInitializesTarget()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
for (var j in null) {
    i = j;
}
i = j;
"
            );
        }

        [Test]
        public void ForEachExecutesExpression()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
for (var i in (j = 0)) {
}
i = j;
"
            );
        }

        [Test]
        public void TryAlwaysExecutes()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
try {
    var j = 0;
} finally {
}
i = j;
"
            );
        }

        [Test]
        public void FinallyAlwaysExecutes()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
try {
} finally {
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void FinallyExecutesWithBreak()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    try {
        break;
    } finally {
        var j = 0;
    }
}
i = j;
"
            );
        }

        [Test]
        public void BreakSkipsCodeButExecutesFinally()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    try {
        break;
    } finally {
        var j = 0;
    }
    var k = 0;
}
i = j;
i = k;
"
            );
        }

        [Test]
        public void BreakOutOfInnerLoop()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    while (true) {
        break;
    }
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ContinueOutOfInnerLoop()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    while (true) {
        continue;
        var k = 0;
    }
    var j = 0;
}
i = j;
i = k;
"
            );
        }

        [Test]
        public void ReturnKillsAllLoops()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
try {
    while (true) {
        while (true) {
            return;
        }
        var j = 0;
    }
} finally {
    i = j;
}
"
            );
        }

        [Test]
        public void ThrowKillsAllLoops()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
try {
    while (true) {
        while (true) {
            throw '';
        }
        var j = 0;
    }
} finally {
    i = j;
}
"
            );
        }

        [Test]
        public void ReturnDoesNotKillOptionalLoop()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
while (true) {
    while (i > 0) {
        return;
    }
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ReturnJumpsToFinally()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
try {
    while (true) {
        while (i > 0) {
            return;
        }
        var j = 0;
    }
} finally {
    i = j;
}
"
            );
        }

        [Test]
        public void CatchTargetAssignmentIsOptional()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
try {
} catch (e) {
}
var i = e;
"
            );
        }

        [Test]
        public void CatchTargetIsAssigned()
        {
            TestBody(
                new[] { "i", "e" },
@"
var i = 0;
try {
} catch (e) {
    var i = e;
}
"
            );
        }

        [Test]
        public void CatchIsOptional()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
try {
} catch (e) {
    var j = 0;
}
var i = j;
var i = e;
"
            );
        }

        [Test]
        public void SwitchWithoutCases()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
switch (i) {
}
"
            );
        }

        [Test]
        public void SwitchExpressionAlwaysGetsExecuted()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
var j;
switch (j = i) {
}
i = j;
"
            );
        }

        [Test]
        public void SwitchWithOnlyDefault()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
switch (i) {
default:
    j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void SwitchWithSingleCaseNoDefault()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
var j;
switch (i) {
case 1:
    j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void SwitchWithAllCasesAssigned()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
switch (i) {
case 1:
    j = 0;
    break;
default:
    j = 1;
    break;
}
i = j;
"
            );
        }

        [Test]
        public void SwitchWithFallThroughAllAssigned()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j, k;
switch (i) {
case 1:
    k = 0;
case 2:
    j = 0;
    break;
default:
    j = 1;
    break;
}
i = j;
i = k;
"
            );
        }

        [Test]
        public void SwitchWithFallThroughNotAllCasesAssigned()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
var j, k;
switch (i) {
case 1:
    j = 0;
case 2:
    k = 0;
    break;
default:
    j = 0;
    break;
}
i = j;
i = k;
"
            );
        }

        [Test]
        public void SwitchWithLastBreakMissing()
        {
            TestBody(
                new[] { "i", "j" },
@"
var i = 0;
var j;
switch (i) {
case 1:
    j = 0;
    break;
default:
    j = 1;
}
i = j;
"
            );
        }

        [Test]
        public void BreakWithLabelSkipsAssigns()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
l: while (true) {
    while (true) {
        break l;
    }
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ContinueWithLabelSkipsAssigns()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
l: while (true) {
    while (true) {
        continue l;
    }
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        [ExpectedException]
        public void BreakOnUnknownLabelThrows()
        {
            TestBody(
                new[] { "i" },
@"
var i = 0;
while (true) {
    while (true) {
        break l;
    }
    var j = 0;
}
i = j;
"
            );
        }

        [Test]
        public void ClosedOverUninitialized()
        {
            TestBody(
                new[] { "f" },
@"
var i;
function f() {
    var j = i;
}
i = 0;
"
            );
        }

        [Test]
        public void ClosedOverInitialized()
        {
            TestBody(
                new[] { "f", "i" },
@"
var i = 0;
function f() {
    var j = i;
}
"
            );
        }
    }
}

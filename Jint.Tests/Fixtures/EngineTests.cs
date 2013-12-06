using System;
using System.Collections.Generic;
using System.IO;
using Jint.Native;
using System.Security.Permissions;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    [TestFixture]
    public class EngineTests : FixturesFixture
    {
        public EngineTests()
            : base("Engine")
        {
        }

        [Test]
        public void ShouldHandleDictionaryObjects()
        {
            var ctx = CreateContext(Assert.Fail);

            var dic = ctx.Global.CreateObject();
            dic["prop1"] = JsNumber.Box(1);
            Assert.IsTrue(dic.HasProperty(JsString.Box("prop1")));
            Assert.IsTrue(dic.HasProperty("prop1"));
            Assert.AreEqual(1, dic["prop1"].ToNumber());
        }

        [Test]
        public void ShouldRunInRun()
        {
            var filename = Path.GetTempFileName();
            File.WriteAllText(filename, "a='bar'");

            var engine = new JintEngine().AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            engine.AllowClr();

            // The DLR compiler won't compile with permissions set
            engine.DisableSecurity();

            engine.SetFunction("load", new Action<string>(delegate(string fileName) { using (var reader = File.OpenText(fileName)) { engine.Run(reader); } }));
            engine.SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run("var a='foo'; load('" + JintEngine.EscapeStringLiteral(filename) + "'); print(a);");

            File.Delete(filename);
        }

        [Test]
        [ExpectedException(typeof(System.Security.SecurityException))]
        public void ShouldNotRunInRun()
        {
            var filename = Path.GetTempFileName();
            File.WriteAllText(filename, "a='bar'");

            var engine = new JintEngine().AddPermission(new FileIOPermission(PermissionState.None));
            engine.AllowClr();
            engine.SetFunction("load", new Action<string>(delegate(string fileName) { using (var reader = File.OpenText(fileName)) { engine.Run(reader); } }));
            engine.SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run("var a='foo'; load('" + JintEngine.EscapeStringLiteral(filename) + "'); print(a);");
        }

        [Test]
        public void ShouldHandleEmptyStatement()
        {
            Assert.AreEqual(1d, new JintEngine().Run(";;;;var i = 1;;;;;;;; return i;;;;;"));
        }

        [Test]
        public void ShouldHandleFor()
        {
            Assert.AreEqual(9d, new JintEngine().Run("var j = 0; for(i = 1; i < 10; i = i + 1) { j = j + 1; } return j;"));
        }

        [Test]
        public void ShouldHandleSwitch()
        {
            Assert.AreEqual(1d, new JintEngine().Run("var j = 0; switch(j) { case 0 : j = 1; break; case 1 : j = 0; break; } return j;"));
            Assert.AreEqual(2d, new JintEngine().Run("var j = -1; switch(j) { case 0 : j = 1; break; case 1 : j = 0; break; default : j = 2; } return j;"));
        }

        [Test]
        public void ShouldHandleVariableDeclaration()
        {
            Assert.AreEqual(null, new JintEngine().Run("var i; return i;"));
            Assert.AreEqual(1d, new JintEngine().Run("var i = 1; return i;"));
            Assert.AreEqual(2d, new JintEngine().Run("var i = 1 + 1; return i;"));
            Assert.AreEqual(3d, new JintEngine().Run("var i = 1 + 1; var j = i + 1; return j;"));
        }

        [Test]
        public void ShouldHandleUndeclaredVariable()
        {
            Assert.AreEqual(1d, new JintEngine().Run("i = 1; return i;"));
            Assert.AreEqual(2d, new JintEngine().Run("i = 1 + 1; return i;"));
            Assert.AreEqual(3d, new JintEngine().Run("i = 1 + 1; j = i + 1; return j;"));
        }

        [Test]
        public void ShouldHandleStrings()
        {
            Assert.AreEqual("hello", new JintEngine().Run("return \"hello\";"));
            Assert.AreEqual("hello", new JintEngine().Run("return 'hello';"));

            Assert.AreEqual("hel'lo", new JintEngine().Run("return \"hel'lo\";"));
            Assert.AreEqual("hel\"lo", new JintEngine().Run("return 'hel\"lo';"));

            Assert.AreEqual("hel\"lo", new JintEngine().Run("return \"hel\\\"lo\";"));
            Assert.AreEqual("hel'lo", new JintEngine().Run("return 'hel\\'lo';"));

            Assert.AreEqual("hel\tlo", new JintEngine().Run("return 'hel\tlo';"));
            Assert.AreEqual("hel/lo", new JintEngine().Run("return 'hel/lo';"));
            Assert.AreEqual("hel//lo", new JintEngine().Run("return 'hel//lo';"));
            Assert.AreEqual("/*hello*/", new JintEngine().Run("return '/*hello*/';"));
            Assert.AreEqual("/hello/", new JintEngine().Run("return '/hello/';"));
        }

        [Test]
        public void ShouldHandleExternalObject()
        {
            Assert.AreEqual(3d,
                new JintEngine()
                    .SetParameter("i", 1)
                    .SetParameter("j", 2)
                    .Run("return i + j;"));
        }

        public bool ShouldBeCalledWithBoolean(TypeCode tc)
        {
            return tc == TypeCode.Boolean;
        }

        [Test]
        public void ShouldHandleEnums()
        {
            Assert.AreEqual(TypeCode.Boolean,
                CreateContext(Assert.Fail).Run("System.TypeCode.Boolean"));

            Assert.AreEqual(true,
                CreateContext(Assert.Fail)
                    .SetParameter("clr", this)
                    .Run("clr.ShouldBeCalledWithBoolean(System.TypeCode.Boolean)"));

        }

        [Test]
        public void ShouldHandleNetObjects()
        {
            Assert.AreEqual("1",
                CreateContext(Assert.Fail) // call Int32.ToString() 
                    .SetParameter("i", 1)
                    .Run("return i.ToString();"));
        }

        [Test]
        public void ShouldReturnDelegateForFunctions()
        {
            const string script = "ccat=function (arg1,arg2){ return arg1+' '+arg2; }";
            JintEngine engine = new JintEngine().SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run(script);
            Assert.AreEqual("Nicolas Penin", engine.CallFunction("ccat", "Nicolas", "Penin"));
        }

        [Test]
        public void ShouldHandleFunctions()
        {
            const string square = @"function square(x) { return x * x; } return square(2);";
            const string fibonacci = @"function fibonacci(n) { if (n == 0) return 0; else return n + fibonacci(n - 1); } return fibonacci(10); ";

            Assert.AreEqual(4d, new JintEngine().Run(square));
            Assert.AreEqual(55d, new JintEngine().Run(fibonacci));
        }

        [Test]
        public void ShouldCreateExternalTypes()
        {
            const string script = @"
                var sb = new System.Text.StringBuilder();
                sb.Append('hi, mom');
                sb.Append(3);	
                sb.Append(true);
                return sb.ToString();
            ";

            var engine = CreateContext(Assert.Fail);

            Assert.AreEqual("hi, mom3True", engine.Run(script));
        }

        [Test]
        [ExpectedException(typeof(JsException))]
        public void ShouldNotAccessClr()
        {
            const string script = @"
                var sb = new System.Text.StringBuilder();
                sb.Append('hi, mom');
                sb.Append(3);	
                sb.Append(true);
                return sb.ToString();
            ";

            Assert.AreEqual("hi, mom3True", new JintEngine().Run(script));
        }

        [ExpectedException(typeof(System.Security.SecurityException))]
        public void SecurityExceptionsShouldNotBeCaught()
        {
            const string script = @"
                try {
                    var sb = new System.Text.StringBuilder();
                    fail('should not have reached this code');
                } 
                catch (e) {
                    fail('should not have reached this code');
                }                
            ";

            new JintEngine().Run(script);
        }

        private static JsString GiveMeJavascript(JsNumber number, JsInstance instance)
        {
            return JsString.Create(number + instance.ToString());
        }

        [Test]
        public void ShouldNotWrapJsInstancesIfExpected()
        {
            var engine = CreateContext(Assert.Fail)
                .SetFunction("evaluate", new Func<JsNumber, JsInstance, JsString>(GiveMeJavascript));

            const string script = @"
                var r = evaluate(3, [1,2]);
                return r;
            ";

            var r = engine.Run(script, false);

            Assert.IsTrue(r is JsString);
            Assert.AreEqual("31,2", r.ToString());
        }

        [Test]
        [ExpectedException(typeof(System.Security.SecurityException))]
        public void ShouldRunInLowTrustMode()
        {
            const string script = @"
                var a = System.Convert.ToInt32(1);
                var b = System.IO.Directory.GetFiles('c:');
            ";

            new JintEngine()
                .AllowClr()
                .Run(script);
        }

        [Test]
        [Ignore]
        public void ShouldAllowSecuritySandBox()
        {
            var userDirectory = Path.GetTempPath();

            const string script = @"
                var b = System.IO.Directory.GetFiles(userDir);
            ";

            new JintEngine()
                .AllowClr()
                .SetParameter("userDir", userDirectory)
                .AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, userDirectory))
                .Run(script);
        }

        [Test]
        [Ignore("With the new architecture the permissions don't correctly work anymore")]
        public void ShouldSetClrProperties()
        {
            // Ensure assembly is loaded
            var a = typeof(System.Windows.Forms.Form);
            var b = a.Assembly; // Force loading in Release mode, otherwise code is optimized
            const string script = @"
                var frm = new System.Windows.Forms.Form();
                frm.Text = 'Test';
                return frm.Text; 
            ";

            var result = new JintEngine()
                .AddPermission(new UIPermission(PermissionState.Unrestricted))
                .AllowClr()
                .Run(script);
            Assert.AreEqual("Test", result.ToString());
        }

        [Test]
        public void ShouldHandleCustomMethods()
        {
            Assert.AreEqual(9d, CreateContext(Assert.Fail)
                .SetFunction("square", new Func<double, double>(a => a * a))
                .Run("return square(3);"));

            CreateContext(Assert.Fail)
                .SetFunction("print", new Action<string>(Console.Write))
                .Run("print('hello');");

            const string script = @"
                function square(x) { 
                    return multiply(x, x); 
                }; 

                return square(4);
            ";

            var result =
                CreateContext(Assert.Fail)
                .SetFunction("multiply", new Func<double, double, double>((x, y) => x * y))
                .Run(script);

            Assert.AreEqual(16d, result);
        }

        [Test]
        public void ShouldHandleDirectNewInvocation()
        {
            Assert.AreEqual("c", CreateContext(Assert.Fail)
                .Run("return new System.Text.StringBuilder('c').ToString();"));
        }

        [Test]
        public void ShouldHandleGlobalVariables()
        {
            const string program = @"
                var i = 3;
                function calculate() {
                    return i*i;
                }
                return calculate();
            ";

            Assert.AreEqual(9d, new JintEngine()
                .Run(program));
        }

        [Test]
        public void ShouldHandleObjectClass()
        {
            const string program = @"
                var userObject = new Object();
                userObject.lastLoginTime = new Date();
                return userObject.lastLoginTime;
            ";

            object result = new JintEngine().Run(program);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<DateTime>(result);
        }

        [Test]
        public void ShouldHandleIndexedProperties()
        {
            const string program = @"
                var userObject = { };
                userObject['lastLoginTime'] = new Date();
                return userObject.lastLoginTime;
            ";

            object result = new JintEngine().Run(program);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<DateTime>(result);
        }

        [Test]
        public void ShouldContinueAfterFunctionCall()
        {
            const string script = @"
                function fib(x) {
                    if (x==0) return 0;
                    if (x==1) return 1;
                    if (x==2) return 2;
                    return fib(x-1) + fib(x-2);
                }

                var x = fib(0);
                
                return 'beacon';
            ";

            Assert.AreEqual("beacon", RunScript(script).ToString());
        }

        [Test]
        public void ShouldRetainGlobalsThroughRuns()
        {
            var jint = new JintEngine();

            jint.Run("i = 3; function square(x) { return x*x; }");

            Assert.AreEqual(3d, jint.Run("return i;"));
            Assert.AreEqual(9d, jint.Run("return square(i);"));
        }

        [Test]
        public void ShouldHandleNativeTypes()
        {

            var jint = CreateContext(Assert.Fail)
                .SetFunction("assert", new Action<object, object, string>(Assert.AreEqual))
                .SetFunction("print", new Action<string>(System.Console.WriteLine))
                .SetParameter("foo", "native string");

            jint.Run(@"
                assert(7, foo.indexOf('string'));            
            ");
        }

        [Test]
        public void ClrNullShouldBeConverted()
        {

            var ctx = CreateContext(Assert.Fail);

            ctx.SetFunction("assert", new Action<object, object, string>(Assert.AreEqual));
            ctx.SetFunction("print", new Action<string>(System.Console.WriteLine));
            ctx.SetParameter("foo", null);

            // strict equlity ecma 262.3 11.9.6 x === y: If type of (x) is null return true.
            ctx.Run(@"
                assert(true, foo == null);
                assert(true, foo === null);
            ");
        }

        [Test]
        public void ShouldHandleStrictMode()
        {
            //Strict mode enabled
            var engine = CreateContext(Assert.Fail, true, Options.Strict)
                .SetFunction("assert", new Action<object, object, string>(Assert.AreEqual));
            engine.Run(@"
            try{
                var test1=function(eval){}
                //should not execute the next statement
                assert(true, false);
            }
            catch(e){
                assert(true, true);
            }
            try{
                (function() {
                    function test2(eval){}
                    //should not execute the next statement
                    assert(true, false);
                })();
            }
            catch(e){
                assert(true, true);
            }");

            //Strict mode disabled
            engine = CreateContext(Assert.Fail, true, Options.EcmaScript3)
                .SetFunction("assert", new Action<object, object, string>(Assert.AreEqual));
            engine.Run(@"
            try{
                var test1=function(eval){}
                assert(true, true);
            }
            catch(e){
                assert(true, false);
            }
            try{
                (function() {
                    function test2(eval){}
                    assert(true, true);
                })();
            }
            catch(e){
                assert(true, false);
            }");
        }

        [Test]
        public void ShouldHandleMultipleRunsInSameScope()
        {
            var jint = CreateContext(Assert.Fail)
                .SetFunction("assert", new Action<object, object, string>(Assert.AreEqual))
                .SetFunction("print", new Action<string>(System.Console.WriteLine));

            jint.Run(@" var g = []; function foo() { assert(0, g.length); }");
            jint.Run(@" foo();");
        }

        [Test]
        public void ShouldHandleClrArrays()
        {
            var values = new int[] { 2, 3, 4, 5, 6, 7 };
            var jint = new JintEngine()
            .SetParameter("a", values)
            .AllowClr();

            Assert.AreEqual(3, jint.Run("a[1];"));
            jint.Run("a[1] = 4");
            Assert.AreEqual(4, jint.Run("a[1];"));
            Assert.AreEqual(4, values[1]);

        }

        [Test]
        public void ShouldHandleClrDictionaries()
        {
            var dic = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            var jint = CreateContext(Assert.Fail);

            jint.SetParameter("dic", dic);

            Assert.AreEqual(1, jint.Run("return dic['a'];"));
            jint.Run("dic['a'] = 4");
            Assert.AreEqual(4, jint.Run("return dic['a'];"));
            Assert.AreEqual(4, dic["a"]);
        }

        [Test]
        public void ShouldEvaluateIndexersAsClrProperties()
        {
            var box = new Box { Width = 10, Height = 20 };

            var jint = CreateContext(Assert.Fail);

            jint.SetParameter("box", box);

            Assert.AreEqual(10, jint.Run("return box.Width"));
            Assert.AreEqual(10, jint.Run("return box['Width']"));

            jint.Run("box['Height'] = 30;");

            Assert.AreEqual(30, box.Height);

            jint.Run("box.Height = 18;");
            
            Assert.AreEqual(18, box.Height);
        }

        [Test]
        public void ShouldEvaluateIndexersAsClrFields()
        {
            var box = new Box { width = 10, height = 20 };

            var jint = CreateContext(Assert.Fail);

            jint.SetParameter("box", box);

            Assert.AreEqual(10, jint.Run("return box.width"));
            Assert.AreEqual(10, jint.Run("return box['width']"));

            jint.Run("box['height'] = 30;");

            Assert.AreEqual(30, box.height);

            jint.Run("box.height = 18;");

            Assert.AreEqual(18, box.height);

        }

        [Test]
        public void ShouldFindOverloadWithNullParam()
        {
            var box = new Box { Width = 10, Height = 20 };

            var jint = CreateContext(Assert.Fail);

            jint.SetFunction("assert", new Action<object, object, string>(Assert.AreEqual));
            jint.SetParameter("box", box);

            jint.Run(@"
                assert(1, Number(box.Foo(1)));
                assert(2, Number(box.Foo(2, null)));    
            ");
        }

        [Test]
        public void ShouldNotThrowOverflowExpcetion()
        {
            var jint = CreateContext(Assert.Fail);
            jint.SetParameter("box", new Box());
            jint.Run("box.Write(new Date);");

        }

        [Test]
        public void ShouldNotReproduceBug85418()
        {
            var engine = new JintEngine();
            engine.SetParameter("a", 4);
            Assert.AreEqual(4, engine.Run("return a"));
            Assert.AreEqual(4d, engine.Run("return 4"));
            Assert.AreEqual(true, engine.Run("return a == 4"));
            Assert.AreEqual(true, engine.Run("return 4 == 4"));
            Assert.AreEqual(true, engine.Run("return a == a"));
        }

        [Test]
        public void ObjectShouldBePassedToDelegates()
        {
            var engine = CreateContext(Assert.Fail);

            engine.SetFunction("render", new Action<object>(s => Console.WriteLine(s)));

            const string script =
                @"
                var contact = {
                    'Name': 'John Doe',
                    'PhoneNumbers': [ 
                    {
                       'Location': 'Home',
                       'Number': '555-555-1234'
                    },
                    {
                        'Location': 'Work',
                        'Number': '555-555-9999 Ext. 123'
                    }
                    ]
                };

                render(contact.Name);
                render(contact.toString());
                render(contact);
            ";

            engine.Run(script);
        }

        [Test]
        public void StaticMemberAfterUndefinedReference()
        {
            var engine = CreateContext(Assert.Fail);

            Assert.AreEqual(System.String.Format("{0}", 1), engine.Run("System.String.Format('{0}', 1)"));
            Assert.AreEqual("undefined", engine.Run("typeof thisIsNotDefined"));
            Assert.AreEqual(System.String.Format("{0}", 1), engine.Run("System.String.Format('{0}', 1)"));
        }

        [Test]
        public void ShouldDetectErrors()
        {
            string errors;
            Assert.IsTrue(JintEngine.HasErrors("var s = @string?;", out errors));
            Assert.IsTrue(JintEngine.HasErrors(")(----", out errors));
        }

        [Test]
        [Ignore]
        public void ShouldNotDetectErrors()
        {
            // todo: fix
            string errors;
            Assert.IsFalse(JintEngine.HasErrors("var s = 'bar'", out errors));
            Assert.IsFalse(JintEngine.HasErrors("", out errors));
            Assert.IsFalse(JintEngine.HasErrors("// comment", out errors));
        }

        [Test]
        [ExpectedException(typeof(JintException))]
        public void RunningInvalidScriptSourceShouldThrow()
        {
            new JintEngine().Run("var s = @string?;");
        }

        [Test]
#if DEBUG
        [Ignore("Catch frame not enabled under debug mode")]
#endif
        public void ClrExceptionsShouldNotBeLost()
        {
            try
            {
                var jint = CreateContext(Assert.Fail);

                jint.SetFunction("foo", new Action(delegate { throw new ArgumentNullException("bar"); }));

                jint.Run(@"foo();");

                Assert.Fail();
            }
            catch (JintException e)
            {
                var ane = e.InnerException as ArgumentNullException;
                Assert.IsNotNull(e);
                Assert.AreEqual("bar", ane.ParamName);
            }
        }

        [Test]
        public void DelegateShouldBeAbleToUseCallFunction()
        {
            var jint = CreateContext(Assert.Fail);

            jint.SetFunction("callme", new Func<double, object>(x => jint.CallFunction("square", x)));

            jint.Run(@"
                square = function(x) { return x*x;}
                assert(9, callme(3));
            ");

            jint = CreateContext(Assert.Fail);

            jint.SetFunction("callme", new Func<Func<bool>, object>(
                callback =>
                {
                    return callback();
                }
            ));

            jint.Run(@"
                assert(true,callme(function() { return true; } ));
            ");
        }

        [Test]
        public void NumberMethodsShouldWorkOnMarshalledNumbers()
        {
            new JintEngine()
                .DisableSecurity()
                .SetFunction("getDouble", new Func<double>(() => { return 11.34543; }))
                .SetFunction("getInt", new Func<int>(() => { return 13; }))
                .SetFunction("print", new Action<string>(s => Console.WriteLine(s)))
                .Run(@"
                    print( getDouble().toFixed(2) );
                    print( getInt().toFixed(2) );
                ");
        }

        [TestCase("AssignBooleanValue.js")]
        [TestCase("AssignProperties.js")]
        [TestCase("CallingANonMethodShouldThrowAnException.js")]
        [TestCase("CascadeEquals.js")]
        [TestCase("CatchNotDefinedVariable.js")]
        [TestCase("CompareNullValues.js")]
        [TestCase("CreateObjectLiterals.js")]
        [TestCase("EvalShouldPass.js")]
        [TestCase("EvaluateConsecutiveIfStatements.js")]
        [TestCase("EvaluateFunctionDeclarationsFirst.js")]
        [TestCase("FunctionPrototypeNotShared.js")]
        [TestCase("FunctionsShouldBeDeclaredInTheirScope.js")]
        [TestCase("HandleAnonymousFunctions.js")]
        [TestCase("HandleAssignment_1.js")]
        [TestCase("HandleAssignment_2.js")]
        [TestCase("HandleBadEnums.js")]
        [TestCase("HandleCommaSeparatedDeclarations.js")]
        [TestCase("HandleFunctionConstructor.js")]
        [TestCase("HandleFunctionsAsObjects_1.js")]
        [TestCase("HandleFunctionsAsObjects_2.js")]
        [TestCase("HandleFunctionScopes.js")]
        [TestCase("HandleInlineCLRMethodCalls.js")]
        [TestCase("HandleLoopScopes.js")]
        [TestCase("HandlePropertiesOnFunctions.js")]
        [TestCase("HandleReturnAsSeparator.js")]
        [TestCase("HandleStaticMethods.js")]
        [TestCase("HandleStructs.js")]
        [TestCase("HandleTheMostSimple.js")]
        [TestCase("IndexerShouldBeEvaluatedBeforeUsed.js")]
        [TestCase("ModifyIteratedCollection.js")]
        [TestCase("NotConflictWithClrMethods.js")]
        [TestCase("NumbersShouldEqualTheirStrings.js")]
        [TestCase("OverrideDefaultFunction.js")]
        [TestCase("ParseCoffeeScript.js")]
        [TestCase("ParseMultilineStrings.js")]
        [TestCase("RandomValuesShouldNotRepeat.js")]
        [TestCase("ReturnUndefined.js")]
        [TestCase("ScopesShouldNotExpand.js")]
        [TestCase("ShortCircuitBooleanOperators.js")]
        [TestCase("StoreFunctionsInArray.js")]
        [TestCase("SupportCasting.js")]
        [TestCase("SupportUtf8VariableNames.js")]
        [TestCase("SwitchFallBackWhenNoBreak.js")]
        [TestCase("UndefinedEqualsToNullShouldBeTrue.js")]
        [TestCase("UseOfUndefinedVariableShouldThrowAnException.js")]
        public void ShouldRunEngineScripts(string script)
        {
            RunFile(script);
        }
    }
}

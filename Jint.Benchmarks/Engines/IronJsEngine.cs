using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronJS.Hosting;
using IronJS.Native;

namespace Jint.Benchmarks.Engines
{
    public class IronJsEngine : IEngine
    {
        public IContext CreateContext()
        {
            return new Context();
        }

        public string Name { get { return "IronJS"; } }

        private class Context : IContext
        {
            private readonly CSharp.Context _context = new CSharp.Context();

            public void ExecuteFile(string test)
            {
                _context.ExecuteFile(test);
            }

            public void Execute(string script)
            {
                _context.Execute(script);
            }

            public void SetFunction(string name, Delegate @delegate)
            {
                int argumentCount = @delegate.Method.GetParameters().Length;

                // Fantastic. CreateFunction is a generic method. The funny thing
                // is that the type constraint on the method is Delegate, which
                // C# doesn't allow. This means that we cannot make SetFunction
                // generic and the only way to allow setting the function through
                // the delegate here is to make a generic method like below.

                var createFunction = typeof(Utils).GetMethod("CreateFunction").MakeGenericMethod(@delegate.GetType());

                var function = createFunction.Invoke(
                    null,
                    new object[]
                    {
                        _context.Environment,
                        argumentCount,
                        @delegate
                    }
                );

                _context.SetGlobal(name, function);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;

namespace Jint.Benchmarks.Engines
{
    internal class JurassicEngine : IEngine
    {
        public IContext CreateContext()
        {
            return new Context();
        }

        public string Name
        {
            get { return "Jurassic"; }
        }

        private class Context : IContext
        {
            private readonly ScriptEngine _engine = new ScriptEngine();

            public void ExecuteFile(string test)
            {
                _engine.ExecuteFile(test);
            }

            public void SetFunction(string name, Delegate @delegate)
            {
                _engine.SetGlobalFunction(name, @delegate);
            }

            public void Execute(string script)
            {
                _engine.Execute(script);
            }
        }
    }
}

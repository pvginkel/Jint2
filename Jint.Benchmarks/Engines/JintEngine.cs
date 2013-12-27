using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Jint.Benchmarks.Engines
{
    public class JintEngine : IEngine
    {
        public IContext CreateContext()
        {
            return new Context();
        }

        public string Name { get { return "Jint 2"; } }

        private class Context : IContext
        {
            private readonly Jint.JintEngine _engine;

            public Context()
            {
                _engine = new Jint.JintEngine();
                _engine.DisableSecurity();
            }

            public void ExecuteFile(string test)
            {
                _engine.Run(File.ReadAllText(test), test);
            }

            public void Execute(string script)
            {
                _engine.Run(script);
            }

            public void SetFunction(string name, Delegate @delegate)
            {
                _engine.SetFunction(name, @delegate);
            }
        }
    }
}

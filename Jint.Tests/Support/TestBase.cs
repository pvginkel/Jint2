using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Jint.Tests.Support
{
    public abstract class TestBase
    {
        private static bool _traceInitialized;

        [TestFixtureSetUp]
        public static void Setup()
        {
            if (_traceInitialized)
                return;

            _traceInitialized = true;


            var toRemove = new List<TraceListener>();

            foreach (TraceListener listener in Trace.Listeners)
            {
                if (listener is DefaultTraceListener)
                    toRemove.Add(listener);
            }

            foreach (var item in toRemove)
            {
                Trace.Listeners.Remove(item);
            }

            Trace.Listeners.Add(new FailTraceListener());
        }

        [DebuggerStepThrough]
        private class FailTraceListener : TraceListener
        {
            public override void Write(string message)
            {
                Console.Write(message);
            }

            public override void WriteLine(string message)
            {
                Console.WriteLine(message);
            }

            public override void Fail(string message)
            {
                Assert.Fail(message);
            }

            public override void Fail(string message, string detailMessage)
            {
                Assert.Fail(message);
            }
        }
    }
}

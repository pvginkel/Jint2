using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Jint.Native
{
    partial class JsSchema
    {
        private static bool _flattenThreadRunning;
        private static PendingFlatten _pendingFlatten;
        private static readonly object _syncRoot = new object();

        private static void QueueFlattenSchema(JsSchema schema)
        {
            bool startThread = false;

            // Push the schema onto the pending queue.

            lock (_syncRoot)
            {
                _pendingFlatten = new PendingFlatten(schema, _pendingFlatten);

                // If the thread isn't running, we need to start it.

                if (!_flattenThreadRunning)
                {
                    _flattenThreadRunning = true;
                    startThread = true;
                }
            }

            if (startThread)
                ThreadPool.QueueUserWorkItem(p => FlattenSchemaThread());
        }

        private static void FlattenSchemaThread()
        {
            while (true)
            {
                // Get the pending queue and reset the field.

                PendingFlatten pending;

                lock (_syncRoot)
                {
                    pending = _pendingFlatten;
                    _pendingFlatten = null;

                    // Quit if we don't have any work.

                    if (pending == null)
                    {
                        _flattenThreadRunning = false;
                        return;
                    }
                }

                // Process the queue.

                while (pending != null)
                {
                    pending.Schema.FlattenSchema();

                    pending = pending.Parent;
                }
            }
        }

        private class PendingFlatten
        {
            public JsSchema Schema { get; private set; }
            public PendingFlatten Parent { get; private set; }

            public PendingFlatten(JsSchema schema, PendingFlatten parent)
            {
                Schema = schema;
                Parent = parent;
            }
        }
    }
}

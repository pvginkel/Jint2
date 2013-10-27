using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpSyntax;

namespace Jint.Backend.Compiled
{
    partial class Visitor
    {
        private class BlockManager
        {
            private int _nextAnonymousLocalId = 1;

            public BlockSyntax Body { get; private set; }
            public ScopeBuilder ScopeBuilder { get; private set; }

            public BlockManager(BlockSyntax body, ScopeBuilder scopeBuilder)
            {
                Body = body;
                ScopeBuilder = scopeBuilder;
            }

            public string GetNextAnonymousLocalName()
            {
                return "anonymousLocal" + _nextAnonymousLocalId++;
            }
        }
    }
}

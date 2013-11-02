using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Jint.Runtime
{
    internal class JintGetMemberBinder : GetMemberBinder
    {
        public JintGetMemberBinder(string name)
            : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Jint.Runtime
{
    internal class JintContext
    {
        private readonly Dictionary<string, JintGetMemberBinder> _getMember = new Dictionary<string, JintGetMemberBinder>();
        private readonly Dictionary<string, JintSetMemberBinder> _setMember = new Dictionary<string, JintSetMemberBinder>();

        public CallSiteBinder GetMember(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            JintGetMemberBinder result;
            if (!_getMember.TryGetValue(name, out result))
            {
                result = new JintGetMemberBinder(name);
                _getMember.Add(name, result);
            }

            return result;
        }

        public CallSiteBinder SetMember(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            JintSetMemberBinder result;
            if (!_setMember.TryGetValue(name, out result))
            {
                result = new JintSetMemberBinder(name);
                _setMember.Add(name, result);
            }

            return result;
        }
    }
}

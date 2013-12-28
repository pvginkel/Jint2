using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal interface IIdentifierManager
    {
        int ResolveIdentifier(string identifier);
        string GetIdentifier(int index);
    }
}

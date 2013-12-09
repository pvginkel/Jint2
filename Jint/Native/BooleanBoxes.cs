using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class BooleanBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;

        public static object Box(bool value)
        {
            return value ? True : False;
        }
    }
}

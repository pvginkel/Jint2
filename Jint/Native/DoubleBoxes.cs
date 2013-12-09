using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class DoubleBoxes
    {
        public static readonly object MinValue = Double.MinValue;
        public static readonly object MaxValue = Double.MaxValue;
        public static readonly object NaN = Double.NaN;
        public static readonly object NegativeInfinity = Double.NegativeInfinity;
        public static readonly object PositiveInfinity = Double.PositiveInfinity;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class BoundNodeExtensions
    {
        public static bool IsAssignable(this BoundNode self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            if (self.Kind == BoundKind.GetMember)
                return true;
            if (self.Kind == BoundKind.GetVariable)
                return ((BoundGetVariable)self).Variable is IBoundWritable;
            return false;
        }
    }
}

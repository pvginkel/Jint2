using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal static class SyntaxUtil
    {
        public static IList<T> ToReadOnly<T>(this IEnumerable<T> self)
        {
            if (self == null)
                return new T[0];

            return new ReadOnlyCollection<T>(self.ToList());
        }
    }
}

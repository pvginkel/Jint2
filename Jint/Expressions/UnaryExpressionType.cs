using System.Text;
using System.Collections.Generic;
using System;

namespace Jint.Expressions
{
    public enum UnaryExpressionType
    {
        TypeOf,
        New,
        Not,
        Negate,
        Positive,
        PrefixPlusPlus,
        PrefixMinusMinus,
        PostfixPlusPlus,
        PostfixMinusMinus,
        Delete,
        Void,
        Inv,
        Unknown
    }
}
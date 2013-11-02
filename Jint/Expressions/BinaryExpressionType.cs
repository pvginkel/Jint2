using System.Text;
using System.Collections.Generic;
using System;

namespace Jint.Expressions
{
    public enum BinaryExpressionType
    {
        And,
        Or,
        NotEqual,
        LesserOrEqual,
        GreaterOrEqual,
        Lesser,
        Greater,
        Equal,
        Minus,
        Plus,
        Modulo,
        Div,
        Times,
        Pow,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXOr,
        Same,
        NotSame,
        LeftShift,
        RightShift,
        UnsignedRightShift,
        InstanceOf,
        In,
        Unknown
    }
}
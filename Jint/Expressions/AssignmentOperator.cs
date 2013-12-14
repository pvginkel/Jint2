using System.Text;
using System.Collections.Generic;
using System;

namespace Jint.Expressions
{
    internal enum AssignmentOperator
    {
        Assign,
        Multiply,
        Divide,
        Modulo,
        Add,
        Subtract,
        LeftShift,
        RightShift,
        UnsignedRightShift,
        BitwiseAnd,
        BitwiseOr,
        BitwiseExclusiveOr,
    }
}
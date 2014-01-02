using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
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
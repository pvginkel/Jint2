using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal enum BoundKind
    {
        Binary,
        Block,
        Body,
        Break,
        Call,
        CallArgument,
        Catch,
        Constant,
        Continue,
        CreateFunction,
        DeleteMember,
        DoWhile,
        Emit,
        Empty,
        ExpressionBlock,
        ExpressionStatement,
        Finally,
        For,
        ForEachIn,
        GetMember,
        GetVariable,
        HasMember,
        If,
        Label,
        New,
        NewBuiltIn,
        RegEx,
        Return,
        SetAccessor,
        SetMember,
        SetVariable,
        Switch,
        SwitchCase,
        Throw,
        Try,
        Unary,
        While
    }
}

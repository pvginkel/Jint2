﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Compiler
{
    internal interface ITypeBuilder
    {
        Type Type { get; }
        IList<IFunctionBuilder> Functions { get; }

        IFunctionBuilder CreateFunction(Type delegateType, string name, string sourceCode);
        FieldInfo CreateCacheSlot(string @object, string member);
    }
}

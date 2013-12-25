using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Support;

namespace Jint.Compiler
{
    internal interface IFunctionBuilder
    {
        ITypeBuilder TypeBuilder { get; }
        MethodInfo Method { get; }

        ILBuilder GetILBuilder();
    }
}

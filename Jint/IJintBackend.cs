using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint
{
    public interface IJintBackend
    {
        JsGlobal Global { get; }
        PermissionSet PermissionSet { get; set; }
        bool AllowClr { get; set; }

        object Run(ProgramSyntax program, bool unwrap);

        object CallFunction(string name, object[] args);

        object CallFunction(JsFunction function, object[] args);

        JsInstance Eval(JsInstance[] arguments);

        JsInstance ExecuteFunction(JsFunction function, JsInstance that, JsInstance[] arguments, JsInstance[] genericParameters);

        int Compare(JsFunction function, JsInstance x, JsInstance y);

        JsFunction CompileFunction(JsInstance[] parameters);

        JsInstance ResolveUndefined(string typeFullName, Type[] generics);
    }
}

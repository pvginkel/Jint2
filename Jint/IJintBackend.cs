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

        JsFunctionResult ExecuteFunction(JsFunction function, JsInstance that, JsInstance[] arguments, Type[] genericParameters);

        int Compare(JsFunction function, JsInstance x, JsInstance y);

        object MarshalJsFunctionHelper(JsFunction func, Type delegateType);

        JsInstance Construct(JsFunction function, JsInstance[] parameters);

        JsFunction CompileFunction(JsInstance[] parameters, Type[] genericArgs);

        JsInstance ResolveUndefined(string typeFullName, Type[] generics);
    }
}

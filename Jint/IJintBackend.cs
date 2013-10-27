﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint
{
    public interface IJintBackend
    {
        IGlobal Global { get; }
        JsScope GlobalScope { get; }
        PermissionSet PermissionSet { get; set; }
        bool AllowClr { get; set; }

        object Run(Program program, bool unwrap);

        object CallFunction(string name, object[] args);

        object CallFunction(JsFunction function, object[] args);

        JsInstance Eval(JsInstance[] arguments);

        JsInstance ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] arguments, Type[] genericParameters);

        int Compare(JsFunction function, JsInstance x, JsInstance y);

        object MarshalJsFunctionHelper(JsFunction func, Type delegateType);

        JsInstance Construct(JsFunction function, JsInstance[] parameters);
    }
}
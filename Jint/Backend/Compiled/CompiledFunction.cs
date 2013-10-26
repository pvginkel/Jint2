using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Backend.Compiled
{
    internal class CompiledFunction : JsFunction
    {
        private readonly CompiledFunctionDelegate _function;

        public CompiledFunction(CompiledFunctionDelegate function, JsObject prototype)
            : base(prototype)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            _function = function;
        }

        public override JsInstance Execute(IJintVisitor visitor, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            visitor.Return(_function(that, parameters));

            return null;
        }

        public override string ToString()
        {
            return String.Format("function {0}() {{ [compiled code] }}", _function.Method.Name);
        }
    }
}

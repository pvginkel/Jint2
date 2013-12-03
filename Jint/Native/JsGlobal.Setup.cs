using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private void DefineProperty(JsObject prototype, string name, JsFunction getFunction, JsFunction setFunction, PropertyAttributes attributes)
        {
            var getJsFunction =
                getFunction != null
                ? CreateFunction(null, getFunction, 0, null)
                : null;
            var setJsFunction =
                setFunction != null
                ? CreateFunction(null, setFunction, 0, null)
                : null;

            var descriptor = new PropertyDescriptor(
                this,
                prototype,
                name,
                getJsFunction,
                setJsFunction,
                attributes
            );

            prototype.DefineOwnProperty(descriptor);
        }

        private static void DefineProperty(JsObject prototype, string name, JsInstance value, PropertyAttributes attributes)
        {
            prototype.DefineOwnProperty(name, value, attributes);
        }

        protected void DefineFunction(JsObject prototype, string name, JsFunction @delegate, int argumentCount, PropertyAttributes attributes)
        {
            prototype.DefineOwnProperty(
                name,
                CreateFunction(name, @delegate, argumentCount, null),
                attributes
            );
        }
    }
}

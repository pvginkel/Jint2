﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        public JsObject CreateDate(DateTime date)
        {
            return CreateDate(JsConvert.ToNumber(date));
        }

        public JsObject CreateDate(double value)
        {
            return (JsObject)DateClass.Construct(
                _runtime,
                new JsInstance[]
                {
                    JsNumber.Create(value)
                }
            );
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure)
        {
            return CreateFunction(name, @delegate, argumentCount, closure, CreateObject(FunctionClass.Prototype));
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure, JsObject prototype)
        {
            return CreateFunction(name, @delegate, argumentCount, closure, prototype, false);
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure, JsObject prototype, bool isClr)
        {
            var result = CreateObject(null, prototype, new JsDelegate(name, @delegate, argumentCount, closure));

            result.SetClass(JsNames.ClassFunction);
            result.SetIsClr(isClr);

            return result;
        }

        public JsObject CreateObject()
        {
            return CreateObject(ObjectClass.Prototype);
        }

        public JsObject CreateObject(JsObject prototype)
        {
            return CreateObject(null, prototype, null);
        }

        public JsObject CreateObject(object value, JsObject prototype)
        {
            return CreateObject(value, prototype, null);
        }

        private JsObject CreateObject(object value, JsObject prototype, JsDelegate @delegate)
        {
            return new JsObject(this, value, prototype, @delegate);
        }

        public JsObject CreateError(JsObject constructor)
        {
            return CreateError(constructor, null);
        }

        public JsObject CreateError(JsObject constructor, string message)
        {
            return (JsObject)constructor.Construct(
                _runtime,
                new JsInstance[] { JsString.Create(message) }
            );
        }

        public JsObject CreateRegExp(string pattern)
        {
            return CreateRegExp(pattern, null);
        }

        public JsObject CreateRegExp(string pattern, string options)
        {
            return (JsObject)RegExpClass.Construct(
                _runtime,
                new JsInstance[]
                {
                    JsString.Create(pattern),
                    JsString.Create(options)
                }
            );
        }

        public JsObject CreateArray()
        {
            return (JsObject)ArrayClass.Construct(
                _runtime,
                JsInstance.EmptyArray
            );
        }
    }
}

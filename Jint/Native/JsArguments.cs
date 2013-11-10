using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public class JsArguments : JsObject
    {
        public const string CalleeName = "callee";

        private readonly ValueDescriptor _calleeDescriptor;

        protected JsFunction Callee
        {
            get { return this[CalleeName] as JsFunction; }
            set { this[CalleeName] = value; }
        }

        public JsArguments(IGlobal global, JsFunction callee, JsInstance[] arguments)
            : base(global.ObjectClass.New())
        {
            _global = global;

            // Add the named parameters
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    DefineOwnProperty(
                        new ValueDescriptor(this, i.ToString(), arguments[i]) { Enumerable = false }
                    );
                }

                _length = arguments.Length;
            }
            else
            {
                _length = 0;
            }

            _calleeDescriptor = new ValueDescriptor(this, CalleeName, callee) { Enumerable = false };
            DefineOwnProperty(_calleeDescriptor);

            DefineOwnProperty(new PropertyDescriptor<JsArguments>(global, this, "length", GetLength) { Enumerable = false });
        }

        private int _length;
        private readonly IGlobal _global;

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return Length;
        }

        /// <summary>
        /// The number of the actually passed arguments
        /// </summary>
        public override int Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        public override string Class
        {
            get { return ClassArguments; }
        }

        public JsInstance GetLength(JsArguments target)
        {
            return _global.NumberClass.New(target._length);
        }
    }
}

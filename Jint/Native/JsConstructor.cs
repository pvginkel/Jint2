using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public abstract class JsConstructor : JsFunction
    {
        /// <summary>
        /// Constructs JsContructor, setting [[Prototype]] property to global.FunctionClass.PrototypeProperty
        /// </summary>
        /// <param name="global">Global</param>
        protected JsConstructor(JsGlobal global)
            : base(global)
        {
        }

        /// <summary>
        /// Special form of the contructor used when constructin JsFunctionConstructor
        /// </summary>
        /// <remarks>This constructor is called when the global.FunctionClass isn't set yet.</remarks>
        /// <param name="global">Global</param>
        /// <param name="prototype">Prototype</param>
        protected JsConstructor(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
        }

        /// <summary>
        /// This method is used to wrap an native value with a js object of the specified type.
        /// </summary>
        /// <remarks>
        /// This method creates a new apropriate js object and stores
        /// </remarks>
        /// <typeparam name="T">A type of a native value to wrap</typeparam>
        /// <param name="value">A native value to wrap</param>
        /// <returns>A js instance</returns>
        public virtual JsInstance Wrap<T>(T value)
        {
            return new JsObject(Global, value, Prototype);
        }

        public override string GetBody()
        {
            return "[native ctor]";
        }
    }
}

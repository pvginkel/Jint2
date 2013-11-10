using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsFunction : JsObject
    {
        public static string CallName = "call";
        public static string ApplyName = "apply";
        public static string ConstructorName = "constructor";
        public static string PrototypeName = "prototype";

        public string Name { get; set; }
        public SyntaxNode Statement { get; set; }
        public List<string> Arguments { get; set; }
        public JsScope Scope { get; set; }

        public JsFunction(IGlobal global, SyntaxNode statement)
            : this(global.FunctionClass.PrototypeProperty)
        {
            Statement = statement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="global"></param>
        public JsFunction(IGlobal global)
            : this(global.FunctionClass.PrototypeProperty)
        {
        }

        /// <summary>
        /// Init new function object with a specified prototype
        /// </summary>
        /// <param name="prototype">prototype for this object</param>
        public JsFunction(JsObject prototype)
            : base(prototype)
        {
            Arguments = new List<string>();
            Statement = new EmptySyntax();
            DefineOwnProperty(PrototypeName, JsNull.Instance, PropertyAttributes.DontEnum);
        }

        public override int Length
        {
            get
            {
                return Arguments.Count;
            }
            set
            {
            }
        }

        public JsObject PrototypeProperty
        {
            get
            {
                return this[PrototypeName] as JsObject;
            }
            set
            {
                this[PrototypeName] = value;
            }
        }

        //15.3.5.3
        public virtual bool HasInstance(JsObject inst)
        {
            if (inst != null && inst != JsNull.Instance && inst != JsNull.Instance)
            {
                return PrototypeProperty.IsPrototypeOf(inst);
            }
            return false;
        }

        //13.2.2
        public virtual JsObject Construct(JsInstance[] parameters, Type[] generics, IGlobal global)
        {
            var instance = global.ObjectClass.New(PrototypeProperty);

            var result = global.Backend.ExecuteFunction(this, instance, parameters, generics).This;

            var obj = result as JsObject;
            if (obj != null && obj != JsUndefined.Instance)
                return obj;

            return instance;
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override object Value
        {
            get { return null; }
            set { }
        }

        public JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters)
        {
            return Execute(global, that, parameters, null);
        }

        public virtual JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (genericArguments != null)
                throw new JintException("This method can't be called as a generic");

            throw new InvalidOperationException();
        }

        public override string Class
        {
            get { return ClassFunction; }
        }

        public override string ToSource()
        {
            return String.Format("function {0} ( {1} ) {{ {2} }}", Name, String.Join(", ", Arguments.ToArray()), GetBody());
        }

        public virtual string GetBody()
        {
            return "/* js code */";
        }

        public override string ToString()
        {
            return ToSource();
        }

        public override bool ToBoolean()
        {
            return true;
        }

        public override double ToNumber()
        {
            return 1;
        }
    }
}

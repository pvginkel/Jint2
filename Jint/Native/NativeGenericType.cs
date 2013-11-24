using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    /// <summary>
    /// This class is used to reflect a native generic.
    /// </summary>
    class NativeGenericType: JsObject
    {
        private Type _reflectedType;

        public NativeGenericType(JsGlobal global, Type reflectedType, JsObject prototype)
            : base(global, prototype)
        {
            if (reflectedType == null)
                throw new ArgumentNullException("reflectedType");
        }

        public override object Value
        {
            get { return _reflectedType; }
            set { _reflectedType = (Type)value; }
        }

        JsConstructor MakeType(Type[] args)
        {
            return Global.Marshaller.MarshalType( _reflectedType.MakeGenericType(args) );
        }
    }
}

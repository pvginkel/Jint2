using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Delegates;
using System.Reflection;

namespace Jint.Native {
    /// <summary>
    /// Wraps a delegate which can be called as a method on an object, with or without parameters.
    /// </summary>
    [Serializable]
    public class ClrImplDefinition<T> : JsFunction
        where T : JsInstance {
        private readonly Delegate _impl;
        private readonly int _length;
        private readonly bool _hasParameters;

        private ClrImplDefinition(bool hasParameters, JsObject prototype)
            : base(prototype) {
            _hasParameters = hasParameters;
        }

        public ClrImplDefinition(Func<T, JsInstance[], JsInstance> impl, JsObject prototype)
            : this(impl, -1, prototype) {
        }

        public ClrImplDefinition(Func<T, JsInstance[], JsInstance> impl, int length, JsObject prototype)
            : this(true, prototype) {
            _impl = impl;
            _length = length;
        }

        public ClrImplDefinition(Func<T, JsInstance> impl, JsObject prototype)
            : this(impl, -1, prototype) {
        }

        public ClrImplDefinition(Func<T, JsInstance> impl, int length, JsObject prototype)
            : this(false, prototype) {
            _impl = impl;
            _length = length;
        }

        public override JsInstance Execute(IJintVisitor visitor, JsDictionaryObject that, JsInstance[] parameters) {
            try {
                JsInstance result;
                if (_hasParameters)
                    result = _impl.DynamicInvoke(new object[] { that, parameters }) as JsInstance;
                else
                    result = _impl.DynamicInvoke(new object[] { that }) as JsInstance;

                visitor.Return(result);
                return result;
            }
            catch (TargetInvocationException e) {
                throw e.InnerException;
            }
            catch (ArgumentException) {
                var constructor = that["constructor"] as JsFunction;
                throw new JsException(visitor.Global.TypeErrorClass.New("incompatible type: " + constructor == null ? "<unknown>" : constructor.Name));
            }
            catch (Exception e) {
                if (e.InnerException is JsException) {
                    throw e.InnerException;
                }

                throw;
            }
        }

        public override int Length {
            get {
                if (_length == -1)
                    return base.Length;
                return _length;
            }
        }

        public override string ToString() {
            return String.Format("function {0}() { [native code] }", _impl.Method.Name);
        }

    }
}

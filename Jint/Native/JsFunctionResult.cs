using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct JsFunctionResult
    {
        private readonly JsInstance _result;
        private readonly JsInstance _this;
        private readonly bool[] _outParameters;

        public JsInstance Result
        {
            get { return _result; }
        }

        public JsInstance This
        {
            get { return _this; }
        }

        public bool[] OutParameters
        {
            get { return _outParameters; }
        }

        public JsFunctionResult(JsInstance result, JsInstance @this)
            : this(result, @this, null)
        {
        }

        public JsFunctionResult(JsInstance result, JsInstance @this, bool[] outParameters)
        {
            _result = result;
            _this = @this;
            _outParameters = outParameters;
        }
    }
}

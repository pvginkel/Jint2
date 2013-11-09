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

        public JsInstance Result
        {
            get { return _result; }
        }

        public JsInstance This
        {
            get { return _this; }
        }

        public JsFunctionResult(JsInstance result, JsInstance @this)
        {
            _result = result;
            _this = @this;
        }
    }
}

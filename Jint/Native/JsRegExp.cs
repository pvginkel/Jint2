using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public class JsRegExp : JsObject
    {
        public bool IsGlobal { get { return this["global"].ToBoolean(); } }
        public bool IsIgnoreCase { get { return (_options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase; } }
        public bool IsMultiLine { get { return (_options & RegexOptions.Multiline) == RegexOptions.Multiline; } }

        private readonly string _pattern;
        private readonly RegexOptions _options;

        public JsRegExp(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
        }

        public JsRegExp(JsGlobal global, string pattern, JsObject prototype)
            : this(global, pattern, false, false, false, prototype)
        {
        }

        public JsRegExp(JsGlobal global, string pattern, bool g, bool i, bool m, JsObject prototype)
            : base(global, prototype)
        {
            _options = RegexOptions.ECMAScript;

            if (m)
                _options |= RegexOptions.Multiline;
            if (i)
                _options |= RegexOptions.IgnoreCase;

            _pattern = pattern;
        }

        public string Pattern
        {
            get { return _pattern; }
        }

        public Regex Regex
        {
            get { return new Regex(_pattern, _options); }
        }

        public RegexOptions Options
        {
            get { return _options; }
        }

        public override object Value
        {
            get { return null; }
        }

        public override string ToSource()
        {
            return "/" + _pattern.ToString() + "/";
        }

        public override string Class
        {
            get { return ClassRegexp; }
        }

        public override bool IsClr
        {
            get { return false; }
        }
    }
}

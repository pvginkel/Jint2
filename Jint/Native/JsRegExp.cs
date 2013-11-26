﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public class JsRegExp : JsObject
    {
        public bool IsGlobal
        {
            get { return (_options & JsRegExpOptions.Global) != 0; }
        }

        public bool IsIgnoreCase
        {
            get { return (_options & JsRegExpOptions.IgnoreCase) != 0; }
        }

        public bool IsMultiLine
        {
            get { return (_options & JsRegExpOptions.Multiline) != 0; }
        }


        private readonly string _pattern;
        private readonly RegexOptions _parsedOptions;
        private readonly JsRegExpOptions _options;

        internal JsRegExp(JsGlobal global, string pattern, JsRegExpOptions options, JsObject prototype)
            : base(global, null, prototype)
        {
            _options = options;
            _parsedOptions = RegexOptions.ECMAScript;

            if (options.HasFlag(JsRegExpOptions.Multiline))
                _parsedOptions |= RegexOptions.Multiline;
            if (options.HasFlag(JsRegExpOptions.IgnoreCase))
                _parsedOptions |= RegexOptions.IgnoreCase;

            _pattern = pattern;

            this["source"] = JsString.Create(pattern);
            this["lastIndex"] = JsNumber.Create(0);
            this["global"] = JsBoolean.Create(options.HasFlag(JsRegExpOptions.Global));
        }

        public string Pattern
        {
            get { return _pattern; }
        }

        public Regex Regex
        {
            get { return new Regex(_pattern, _parsedOptions); }
        }

        public RegexOptions Options
        {
            get { return _parsedOptions; }
        }

        public override object Value
        {
            get { return null; }
        }

        public override string ToSource()
        {
            return "/" + _pattern + "/";
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

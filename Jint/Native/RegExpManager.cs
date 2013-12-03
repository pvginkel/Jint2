using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    internal class RegExpManager
    {
        private readonly string _pattern;
        private readonly RegexOptions _parsedOptions;
        private readonly RegExpOptions _options;

        public bool IsGlobal
        {
            get { return (_options & RegExpOptions.Global) != 0; }
        }

        public bool IsIgnoreCase
        {
            get { return (_options & RegExpOptions.IgnoreCase) != 0; }
        }

        public bool IsMultiLine
        {
            get { return (_options & RegExpOptions.Multiline) != 0; }
        }

        public RegExpManager(string pattern, RegExpOptions options)
        {
            _options = options;
            _parsedOptions = RegexOptions.ECMAScript;

            if (options.HasFlag(RegExpOptions.Multiline))
                _parsedOptions |= RegexOptions.Multiline;
            if (options.HasFlag(RegExpOptions.IgnoreCase))
                _parsedOptions |= RegexOptions.IgnoreCase;

            _pattern = pattern;
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

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('/');
            sb.Append(_pattern);
            sb.Append('/');

            if (IsGlobal)
                sb.Append('g');
            if (IsIgnoreCase)
                sb.Append('i');
            if (IsMultiLine)
                sb.Append('m');

            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    internal class RegexManager
    {
        private static readonly ConcurrentDictionary<RegexKey, Regex> _regexCache = new ConcurrentDictionary<RegexKey, Regex>();

        private static DictionaryCacheSlot _inputCacheSlot;
        private static DictionaryCacheSlot _indexCacheSlot;

        private readonly string _pattern;
        private readonly JsRegexOptions _options;

        public Regex Regex { get; private set; }

        public bool IsGlobal
        {
            get { return (_options & JsRegexOptions.Global) != 0; }
        }

        public int LastIndex { get; set; }

        public RegexManager(string pattern, string options)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            _pattern = pattern;
            _options = JsRegexOptions.None;

            if (!String.IsNullOrEmpty(options))
            {
                foreach (char c in options)
                {
                    switch (c)
                    {
                        case 'm': _options |= JsRegexOptions.Multiline; break;
                        case 'i': _options |= JsRegexOptions.IgnoreCase; break;
                        case 'g': _options |= JsRegexOptions.Global; break;
                    }
                }
            }

            var parsedOptions = RegexOptions.ECMAScript;

            if (_options.HasFlag(JsRegexOptions.Multiline))
                parsedOptions |= RegexOptions.Multiline;
            if (_options.HasFlag(JsRegexOptions.IgnoreCase))
                parsedOptions |= RegexOptions.IgnoreCase;

            Regex = _regexCache.GetOrAdd(
                new RegexKey(pattern, parsedOptions),
                p => new Regex(p.Pattern, p.Options | RegexOptions.Compiled)
            );
        }

        public RegexManager(string pattern, JsRegexOptions options)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            _options = options;
            _pattern = pattern;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('/');
            sb.Append(_pattern);
            sb.Append('/');

            if (IsGlobal)
                sb.Append('g');
            if ((_options & JsRegexOptions.IgnoreCase) != 0)
                sb.Append('i');
            if ((_options & JsRegexOptions.Multiline) != 0)
                sb.Append('m');

            return sb.ToString();
        }

        public JsObject Exec(JintRuntime runtime, string input)
        {
            var lastIndex = IsGlobal ? LastIndex : 0;
            if (lastIndex >= input.Length)
                return null;

            var match = Regex.Match(input, lastIndex);
            if (!match.Success)
                return null;

            var array = runtime.Global.CreateArray();
            var store = array.FindArrayStore();

            array.SetProperty(Id.index, (double)match.Index, ref _indexCacheSlot);
            array.SetProperty(Id.input, input, ref _inputCacheSlot);

            if (IsGlobal)
                LastIndex = match.Index + match.Length;

            for (int i = 0; i < match.Groups.Count; i++)
            {
                store.DefineOrSetPropertyValue(i, match.Groups[i].Value);
            }

            return array;
        }

        private struct RegexKey : IEquatable<RegexKey>
        {
            private readonly RegexOptions _options;
            private readonly string _pattern;

            public string Pattern
            {
                get { return _pattern; }
            }

            public RegexOptions Options
            {
                get { return _options; }
            }

            public RegexKey(string pattern, RegexOptions options)
                : this()
            {
                _pattern = pattern;
                _options = options;
            }

            public override bool Equals(object obj)
            {
                if (obj is RegexKey)
                    return Equals((RegexKey)obj);

                return false;
            }

            public bool Equals(RegexKey other)
            {
                return _pattern == other._pattern && _options == other._options;
            }

            public override int GetHashCode()
            {
                return _pattern.GetHashCode() * 31 + _options.GetHashCode();
            }
        }
    }
}

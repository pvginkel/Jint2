using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public sealed class JsString
    {
        // Between 100 and 10000, there isn't a real difference in execution
        // time for the string-validate-input test. However, used memory is
        // considerably less when the limit is set to 100.
        private const int Limit = 100;

        private JsString _before;
        private string _value;
        private JsString _after;

        public static readonly JsString Empty = new JsString(String.Empty);

        private JsString(string value)
        {
            _value = value;
        }

        private JsString(JsString before, JsString after)
        {
            _before = before;
            _after = after;
        }

        public static object Concat(string left, string right)
        {
            // We only start using the JsString construct when the total string
            // length goes beyond the limit.

            if (left.Length + right.Length <= Limit)
                return left + right;

            return new JsString(new JsString(left), new JsString(right));
        }

        public static object Concat(JsString left, string right)
        {
            return new JsString(left, new JsString(right));
        }

        public static object Concat(string left, JsString right)
        {
            return new JsString(new JsString(left), right);
        }

        public static object Concat(JsString left, JsString right)
        {
            return new JsString(left, right);
        }

        public static object Concat(string left, object right)
        {
            var rightString = right as string;
            if (rightString != null)
                return Concat(left, rightString);
            var rightJsString = right as JsString;
            if (rightJsString != null)
                return Concat(left, rightJsString);
            return Concat(left, JsValue.ToString(right));
        }

        public static object Concat(JsString left, object right)
        {
            var rightString = right as string;
            if (rightString != null)
                return Concat(left, rightString);
            var rightJsString = right as JsString;
            if (rightJsString != null)
                return Concat(left, rightJsString);
            return Concat(left, JsValue.ToString(right));
        }

        public static object Concat(object left, string right)
        {
            var leftString = left as string;
            if (leftString != null)
                return Concat(leftString, right);
            var leftJsString = left as JsString;
            if (leftJsString != null)
                return Concat(leftJsString, right);
            return Concat(JsValue.ToString(left), right);
        }

        public static object Concat(object left, JsString right)
        {
            var leftString = left as string;
            if (leftString != null)
                return Concat(leftString, right);
            var leftJsString = left as JsString;
            if (leftJsString != null)
                return Concat(leftJsString, right);
            return Concat(JsValue.ToString(left), right);
        }

        public override string ToString()
        {
            if (_value == null)
                Flatten();

            return _value;
        }

        private void Flatten()
        {
            var sb = new StringBuilder();

            Flatten(sb, _before);
            Flatten(sb, _after);

            _value = sb.ToString();

            _before = null;
            _after = null;
        }

        private static void Flatten(StringBuilder sb, JsString node)
        {
            while (true)
            {
                if (node._value != null)
                {
                    sb.Append(node._value);
                    break;
                }

                Flatten(sb, node._before);
                node = node._after;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Parser
{
    internal static class ES3Util
    {
        private static readonly Encoding Latin1 = Encoding.GetEncoding("iso-8859-1");
        private const string ValidOctalChars = "01234567";
        private const string ValidDigitChars = "0123456789";
        private const string ValidHexChars = "0123456789abcdefABCDEF";

        public static string ExtractString(string text, bool skipQuotes)
        {
            // https://developer.mozilla.org/en/Core_JavaScript_1.5_Guide/Literals#String Literals    

            if (skipQuotes)
                Debug.Assert((text[0] == '"' || text[0] == '\'') && text[0] == text[text.Length - 1]);

            var sb = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                if (skipQuotes && (i == 0 || i == text.Length - 1))
                    continue;

                char c = text[i];

                if (c == '\\')
                {
                    i++;
                    c = text[i];

                    switch (c)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                            string code = ExtractCode(text, ref i, 3, ValidOctalChars);
                            sb.Append(Latin1.GetChars(new[] { Convert.ToByte(code, 8) })[0]);
                            break;

                        case 'x':
                            i++;
                            code = ExtractCode(text, ref i, 2, ValidHexChars);
                            sb.Append(Latin1.GetChars(new[] { Convert.ToByte(code, 16) })[0]);
                            break;

                        case 'u':
                            i++;
                            code = ExtractCode(text, ref i, 4, ValidHexChars);
                            sb.Append((char)int.Parse(code, NumberStyles.AllowHexSpecifier));
                            break;

                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'v': sb.Append('\v'); break;
                        case '\'': sb.Append('\''); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '\r': if (text[i + 1] == '\n') i++; break;
                        case '\n': break;
                        default: sb.Append(c); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string ExtractCode(string text, ref int offset, int maxLength, string validChars)
        {
            int length = Math.Min(maxLength, text.Length - offset);
            for (int i = 0; i < length; i++)
            {
                if (validChars.IndexOf(text[offset + i]) == -1)
                {
                    length = i;
                    break;
                }
            }

            Debug.Assert(length > 0);

            string result = text.Substring(offset, length);

            // We need to subtract one from the offset because the for loop
            // does an i++.
            offset += length - 1;

            return result;
        }
    }
}

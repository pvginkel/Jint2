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
                        case '8':
                        case '9':
                            string code = text.Substring(i, 3);
                            char parsed = Latin1.GetChars(new[] { Convert.ToByte(code, 8) })[0];
                            // insert decoded char
                            sb.Append(parsed);
                            // skip encoded char
                            i += 2;
                            break;

                        case 'x':
                            code = text.Substring(i + 1, 2);
                            parsed = Latin1.GetChars(new[] { Convert.ToByte(code, 16) })[0];
                            sb.Append(parsed);
                            i += 2;
                            break;

                        case 'u':
                            parsed = (char)int.Parse(
                                text.Substring(i + 1, 4),
                                NumberStyles.AllowHexSpecifier
                            );
                            sb.Append(parsed);
                            i += 4;
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
    }
}

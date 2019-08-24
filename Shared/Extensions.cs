using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebble
{
    static class WebViewExtensions
    {
        public static string Escape(this string text)
        {
            return "'" + text.OrEmpty().Remove("\r").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("'", "\\'").Replace("\n", "\\n") + "'";
        }

    }
}

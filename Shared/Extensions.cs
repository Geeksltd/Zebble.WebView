namespace Zebble;

using Olive;

static class WebViewExtensions
{
    public static string Escape(this string text)
    {
        return "'" + text.OrEmpty().Remove("\r").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("'", "\\'").Replace("\n", "\\n") + "'";
    }
}

namespace Zebble
{
    using System.Runtime.InteropServices;

    partial class WebViewRenderer
    {
        const int URLMON_OPTION_USERAGENT = 0x10000001;

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);


        static void SetDefaultUserAgent()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4501.0 Safari/537.36 Edg/91.0.866.0";

            if (Services.CssEngine.Platform == DevicePlatform.IOS)
                userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.3 Mobile/15E148 Safari/604.1";

            if (Services.CssEngine.Platform == DevicePlatform.Android)
                userAgent = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.5481.153 Mobile Safari/537.36";

            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
        }
    }
}
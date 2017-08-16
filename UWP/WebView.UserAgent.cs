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
            var userAgent = "Mozilla/5.0 (compatible; MSIE 10.0; WebView/3.0; Windows Phone 10.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 925)";

            if (Services.CssEngine.Platform == DevicePlatform.IOS)
                userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25";

            if (Services.CssEngine.Platform == DevicePlatform.Android)
                userAgent = "Mozilla/5.0 (Linux; Android 4.4.2; en-us; SAMSUNG SM-G900T Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Version/1.6 Chrome/28.0.1500.94 Mobile Safari/537.36";

            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
        }
    }
}
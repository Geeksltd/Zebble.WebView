namespace Zebble
{
    public partial class WebView
    {
        internal readonly AsyncEvent ScrollBouncesChanged = new AsyncEvent();

        public IosWebViewConfiguration WebViewConfiguration { get; } = new IosWebViewConfiguration();

        bool scrollBounces;
        public bool Bounces
        {
            get => scrollBounces;
            set
            {
                scrollBounces = value;
                ScrollBouncesChanged.Raise();
            }
        }
    }

    public class IosWebViewConfiguration
    {
        public bool AllowsInlineMediaPlayback { get; set; }
        public WebKit.WKAudiovisualMediaTypes MediaTypesRequiringUserActionForPlayback { get; set; } = WebKit.WKAudiovisualMediaTypes.None;
    }
}
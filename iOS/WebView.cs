namespace Zebble
{
    public partial class WebView
    {
        internal readonly AsyncEvent AllowsInlineMediaPlaybackChanged = new AsyncEvent();
        internal readonly AsyncEvent ScrollBouncesChanged = new AsyncEvent();


        bool allowsInlineMediaPlayback;
        public bool AllowsInlineMediaPlayback
        {
            get => allowsInlineMediaPlayback;
            set
            {
                allowsInlineMediaPlayback = value;
                AllowsInlineMediaPlaybackChanged.Raise();
            }
        }

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
}
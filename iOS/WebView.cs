namespace Zebble
{
    public partial class WebView
    {
        internal readonly AsyncEvent AllowsInlineMediaPlaybackChanged = new AsyncEvent();

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
    }
}
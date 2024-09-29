namespace Zebble;

partial class WebView
{
    public class NavigatingEventArgs
    {
        public bool Cancel { get; set; }
        public string Url { get; set; }
    }
}
namespace Zebble
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    class WebViewRenderer : INativeRenderer
    {
        Android.Views.View Result;

        public Task<Android.Views.View> Render(Renderer renderer)
        {
            Result = new AndroidWebView((WebView)renderer.View);
            return Task.FromResult(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}
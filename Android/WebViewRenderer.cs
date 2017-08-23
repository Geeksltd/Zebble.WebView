namespace Zebble
{
    using System.Threading.Tasks;

    class WebViewRenderer : INativeRenderer
    {
        Android.Views.View Result;

        public Task<Android.Views.View> Render(Renderer renderer)
        {
            Result = new WebViewContainer((WebView)renderer.View);
            return Task.FromResult(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}
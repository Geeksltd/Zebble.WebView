namespace Zebble
{
    using System.Threading.Tasks;
    using Zebble.AndroidOS;

    class WebViewRenderer : INativeRenderer
    {
        Android.Views.View Result;

        public async Task<Android.Views.View> Render(Renderer renderer)
        {
            var view = (WebView)renderer.View;
            return await Task.FromResult(new AndroidControlWrapper<AndroidWebView>(view, new AndroidWebView(view)));
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}
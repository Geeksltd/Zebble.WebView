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
            var wrapper = new AndroidControlWrapper<AndroidWebView>(view, new AndroidWebView(view));
            Result = await (wrapper as IZebbleAndroidControl).Render();
            return Result;
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}
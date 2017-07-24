namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WebViewRenderer : ICustomRenderer
    {
        Plugin.WebView View;
        Android.Views.View Result;

        public Task<Android.Views.View> Render(object view)
        {
            View = (Plugin.WebView)view;
            Result = new AndroidWebView(View);

            return Task.FromResult(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
            View = null;
        }
    }
}
namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using UIKit;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WebViewRenderer : ICustomRenderer
    {
        Plugin.WebView View;
        UIView Result;

        public Task<UIView> Render(object view)
        {
            View = (Plugin.WebView)view;
            Result = new IosWebView(View);

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
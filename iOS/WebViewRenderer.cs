namespace Zebble.Plugin.Renderer
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using UIKit;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WebViewRenderer : INativeRenderer
    {
        UIView Result;

        public Task<UIView> Render(Renderer renderer)
        {
            Result = new IosWebView((WebView)renderer.View);
            return Task.FromResult(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}
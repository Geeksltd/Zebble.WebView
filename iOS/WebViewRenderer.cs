namespace Zebble;

using System;
using System.Threading.Tasks;
using UIKit;

class WebViewRenderer : INativeRenderer
{
    UIView Result;

    public Task<UIView> Render(Renderer renderer)
    {
        var view = (WebView)renderer.View;
        Result = new IosWebView(view, view.WebViewConfiguration);
        return Task.FromResult(Result);
    }

    public void Dispose()
    {
        Result?.Dispose();
        Result = null;
			
			GC.SuppressFinalize(this);
    }
}
namespace Zebble;

using Android.Runtime;
using System;
using System.Threading.Tasks;
using Zebble.AndroidOS;

class WebViewRenderer : INativeRenderer
{
    Android.Views.View Result;

    [Preserve]
    public WebViewRenderer() { }

    public async Task<Android.Views.View> Render(Renderer renderer)
    {
        var view = (WebView)renderer.View;
        return await Task.FromResult(new AndroidWebViewWithControlWrapper(view, new AndroidWebView(view)));
    }

    public void Dispose()
    {
        Result?.Dispose();
        Result = null;

        GC.SuppressFinalize(this);
    }
}

class AndroidWebViewWithControlWrapper : AndroidControlWrapper<AndroidWebView>, IIndependentZebbleAndroidGestureView
{
    public AndroidWebViewWithControlWrapper(View view, AndroidWebView control) : base(view, control) { }

    [Preserve]
    public AndroidWebViewWithControlWrapper(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
}
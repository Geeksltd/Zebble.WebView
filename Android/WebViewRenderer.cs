namespace Zebble
{
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
            return await Task.FromResult(new AndroidWebViewWithConrolWrapper(view, new AndroidWebView(view)));
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
			
			GC.SuppressFinalize(this);
        }
    }

    class AndroidWebViewWithConrolWrapper : AndroidControlWrapper<AndroidWebView>, IIndependentZebbleAndroidGestureView
    {
        public AndroidWebViewWithConrolWrapper(View view, AndroidWebView control) : base(view, control) { }

        [Preserve]
        public AndroidWebViewWithConrolWrapper(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
    }
}
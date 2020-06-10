namespace Zebble
{
    using Android.Runtime;
    using Android.Webkit;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    class AndroidWebViewClient : WebViewClient
    {
        public AndroidWebView WebView;
         
        [Preserve]
        public AndroidWebViewClient() : base() { }

        [Preserve]
        public AndroidWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public override async void OnPageFinished(Android.Webkit.WebView native, string url)
        {
            if (!IsDead(out var view))
            {
                await view.LoadFinished.RaiseOn(Thread.Pool);

                var absoluteUri = new Uri(url).AbsoluteUri;
                if (absoluteUri != view.Url && view.BrowserNavigated.IsHandled())
                {
                    var html = await EvaluateJavascript("document.body.innerHTML");
                    view.OnBrowserNavigated(url, html);
                }
            }

            base.OnPageFinished(native, url);
        }

        public override async void OnReceivedError(Android.Webkit.WebView native, [GeneratedEnum] ClientError errorCode,
            string description, string failingUrl)
        {
            if (!IsDead(out var v))
                await v.LoadingError.RaiseOn(Thread.Pool, description);

            base.OnReceivedError(native, errorCode, description, failingUrl);
        }

        public override async void OnReceivedError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (!IsDead(out var v))
                await v.LoadingError.RaiseOn(Thread.Pool, error.Description);
            base.OnReceivedError(view, request, error);
        }

        bool IsDead(out WebView view)
        {
            view = null;
            if (WebView == null || WebView.Dead) return true;
            view = WebView.View;
            return false;
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView native, string url)
        {
            if (!IsDead(out var v))
                if (url.HasValue() && v.OnBrowserNavigating(url)) return true;

            return base.ShouldOverrideUrlLoading(native, url);
        }

        public Task<string> EvaluateJavascript(string script)
        {
            if (IsDead(out var v)) return null;
            WebView.LoadUrl("javascript:JsInterface.Run(" + script + ")");
            return WebView.JavascriptInterface.TaskSource.Task;
        }

        public void EvaluateJavascriptFunction(string function, string[] args)
        {
            if (IsDead(out var v)) return ;
            WebView.LoadUrl("javascript:" + function + "(" + args.Select(x => x.Escape()).ToString(",") + ")");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WebView = null;
        }
    }
}
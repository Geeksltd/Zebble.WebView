namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Foundation;
    using WebKit;

    class IosWebView : WKWebView
    {
        WebView View;
        NSUrlRequest Request;

        public IosWebView(WebView view) : base(view.GetFrame(), new WKWebViewConfiguration())
        {
            View = view;

            View.SourceChanged.HandleActionOn(Device.UIThread, Refresh);
            View.EvaluatedJavascript = script => RunJavascript(script);
            View.EvaluatedJavascriptFunction += (s, a) => Device.UIThread.Run(() => EvaluateJavascriptFunction(s, a));
            Refresh();
            NavigationDelegate = new IosWebViewNavigationDelegate(this, View, Request);
        }

        async Task<string> RunJavascript(string script)
        {
            var result = await EvaluateJavaScriptAsync(script);
            return result.ToString();
        }

        async Task<string> EvaluateJavascriptFunction(string function, string[] args)
        {
            var result = await EvaluateJavaScriptAsync(function + "(" + args.ToString(",") + ")");
            return result.ToString();
        }

        void Refresh()
        {
            if (View.Url?.Contains(":") == true)
            {
                Request = new NSUrlRequest(new NSUrl(View.Url));
                LoadRequest(Request);
            }
            else
            {
                Request = new NSUrlRequest();
                LoadHtmlString(View.GetExecutableHtml().OrEmpty(), null);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) View = null;
            base.Dispose(disposing);
        }
    }

    class IosWebViewNavigationDelegate : WKNavigationDelegate
    {
        WebView View;
        NSUrlRequest Request;
        WKWebView WebView;

        public IosWebViewNavigationDelegate(WKWebView webView, WebView view, NSUrlRequest request)
        {
            View = view;
            Request = request;
            WebView = webView;
        }

        public override async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (Request.Url?.AbsoluteString != View.Url)
            {
                if (View.BrowserNavigated != null)
                {
                    var html = await WebView.EvaluateJavaScriptAsync("document.body.innerHTML");
                    Device.ThreadPool.RunAction(() => View.OnBrowserNavigated(Request.Url.AbsoluteString, html.ToString()));
                }
            }

            await View.LoadFinished.RaiseOn(Device.ThreadPool);
        }

        public override async void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            await View.LoadingError.RaiseOn(Device.ThreadPool, error.Description);
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            var url = webView.Url;
            if (url != null && url.AbsoluteString.HasValue() && View.OnBrowserNavigating(url.AbsoluteString)) WebView.StopLoading();
        }
    }
}
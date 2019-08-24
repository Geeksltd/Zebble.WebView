namespace Zebble
{
    using Foundation;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using WebKit;

    class IosWebView : WKWebView
    {
        WebView View;
        NSUrlRequest Request;

        public IosWebView(WebView view) : base(view.GetFrame(), new WKWebViewConfiguration())
        {
            View = view;

            view.AllowsInlineMediaPlaybackChanged.HandleOn(Thread.UI, OnAllowsInlineMediaPlaybackChanged);
            View.SourceChanged.HandleActionOn(Thread.UI, Refresh);
            View.EvaluatedJavascript = script => Thread.UI.Run(() => RunJavascript(script));
            View.InvokeJavascriptFunction += (s, a) => Thread.UI.Run(() => EvaluateJavascriptFunction(s, a));
            Refresh();
            NavigationDelegate = new IosWebViewNavigationDelegate(View);
        }

        Task OnAllowsInlineMediaPlaybackChanged()
        {
            Configuration.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
            Configuration.AllowsInlineMediaPlayback = View.AllowsInlineMediaPlayback;

            return Task.CompletedTask;
        }

        async Task<string> RunJavascript(string script)
        {
            var result = await EvaluateJavaScriptAsync(script);
            return result?.ToString() ?? "";
        }

        void EvaluateJavascriptFunction(string function, string[] args)
        {
            EvaluateJavaScriptAsync(function + "(" + args.Select(x => x.Escape()).ToString(",") + ")").RunInParallel();
        }

        void Refresh()
        {
            if (View?.Url?.Contains(":") == true)
            {
                Request = new NSUrlRequest(new NSUrl(View.Url));
                LoadRequest(Request);
            }
            else if (View != null)
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

        public IosWebViewNavigationDelegate(WebView view) => View = view;

        public override async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            var url = webView.Url?.AbsoluteString?.ToString();

            if (url != View.Url)
            {
                if (View.BrowserNavigated.IsHandled())
                {
                    var html = await webView.EvaluateJavaScriptAsync("document.body.innerHTML");
                    Thread.Pool.RunAction(() => View.OnBrowserNavigated(url, html.ToString()));
                }
            }

            while (webView.IsLoading) await Task.Delay(Animation.OneFrame);
            await View.LoadFinished.RaiseOn(Thread.Pool);
        }

        public override async void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            await View.LoadingError.RaiseOn(Thread.Pool, error.Description);
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            var url = navigationAction.Request?.Url?.AbsoluteString;

            if (View.OnBrowserNavigating(url)) decisionHandler(WKNavigationActionPolicy.Cancel);
            else decisionHandler(WKNavigationActionPolicy.Allow);
        }
    }
}
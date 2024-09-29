namespace Zebble;

using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebKit;
using Olive;

class IosWebView : WKWebView
{
    WebView View;
    NSUrlRequest Request;
    bool Dead => View == null || View.IsDisposing;

    public IosWebView(WebView view, WebViewConfiguration configuration) : base(view.GetFrame(),
        new WKWebViewConfiguration
        {
            AllowsPictureInPictureMediaPlayback = true,
            AllowsInlineMediaPlayback = configuration.AllowsInlineMediaPlayback,
            MediaTypesRequiringUserActionForPlayback = configuration.MediaTypesRequiringUserActionForPlayback ? WKAudiovisualMediaTypes.All : WKAudiovisualMediaTypes.None
        })
    {
        View = view;

        View.ScrollBouncesChanged.HandleOnUI(OnScrollBouncesChanged);
        View.SourceChanged.HandleOnUI(Refresh);
        View.EvaluatedJavascript = script => Thread.UI.Run(() => RunJavascript(script));
        View.InvokeJavascriptFunction += (s, a) => Thread.UI.Run(() => EvaluateJavascriptFunction(s, a));
        Refresh();
        NavigationDelegate = new IosWebViewNavigationDelegate(View);
    }

    void OnScrollBouncesChanged()
    {
        if (Dead) return;
        ScrollView.Bounces = View.Bounces;
    }

    async Task<string> RunJavascript(string script)
    {
        if (Dead) return "";

        return (await EvaluateJavaScriptAsync(script))?.ToString() ?? "";
    }

    void EvaluateJavascriptFunction(string function, string[] args)
    {
        EvaluateJavaScriptAsync(function + "(" + args.Select(x => x.Escape()).ToString(",") + ")").RunInParallel();
    }

    void Refresh()
    {
        if (Dead) return;

        if (View.Url?.Contains(":") == true)
        {
            Request = new NSUrlRequest(View.Url.ToNsUrl());
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

    public IosWebViewNavigationDelegate(WebView view) => View = view;

    protected override void Dispose(bool disposing)
    {
        View = null;
        base.Dispose(disposing);
    }

    bool Dead => View == null || View.IsDisposing;

    public override async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
    {
        if (Dead) return; // Disposed.

        var url = webView.Url?.AbsoluteString?.ToString();

        if (View.BrowserNavigated.IsHandled())
        {
            var html = await webView.EvaluateJavaScriptAsync("document.body.innerHTML");
            if (Dead) return;

            View.OnBrowserNavigated(url, html.ToString());
        }

        while (webView.IsLoading) await Task.Delay(Animation.OneFrame);
        if (!Dead)
            View.LoadFinished.RaiseOn(Thread.Pool).RunInParallel();
    }

    public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
    {
        if (Dead) return; // Disposed.
        View.LoadingError.RaiseOn(Thread.Pool, error.Description);
    }

    public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
    {
        if (Dead)
        {
            decisionHandler(WKNavigationActionPolicy.Cancel);
            return; // Disposed.
        }

        var url = navigationAction.Request?.Url?.AbsoluteString;

        if (View.OnBrowserNavigating(url)) decisionHandler(WKNavigationActionPolicy.Cancel);
        else decisionHandler(WKNavigationActionPolicy.Allow);
    }
}
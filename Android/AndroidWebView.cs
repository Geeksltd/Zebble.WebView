namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Android.Runtime;
    using Android.Views;
    using Android.Webkit;
    using Java.Interop;
    using Zebble.AndroidOS;

    class AndroidWebView : Android.Webkit.WebView, IZebbleAndroidControl
    {
        AndroidWebViewClient Client;

        public Zebble.WebView View;
        public JavaScriptResult JavascriptInterface;

        public AndroidWebView(Zebble.WebView view) : base(UIRuntime.CurrentActivity)
        {
            View = view;

            Settings.JavaScriptEnabled = true;
            AddJavascriptInterface(JavascriptInterface = new JavaScriptResult(View), "JsInterface");
            SetWebViewClient(Client = new AndroidWebViewClient { WebView = this });

            View.SourceChanged.HandleActionOn(Device.UIThread, Refresh);
            View.EvaluatedJavascript += s => Device.UIThread.Run(() => EvaluateJavascript(s));
            View.EvaluatedJavascriptFunction += (s, a) => Device.UIThread.Run(() =>
            {
                EvaluateJavascriptFunction(s, a);
                return Task.FromResult("");
            });

            Refresh();
        }

        public Task<Android.Views.View> Render() => Task.FromResult<Android.Views.View>(this);

        async Task<string> EvaluateJavascript(string script) => await Client.EvaluateJavascript(script);

        void EvaluateJavascriptFunction(string function, string[] args) => Client.EvaluateJavascriptFunction(function, args);

        void Refresh()
        {
            if (View.Url?.Contains(":") == true) LoadUrl(View.Url);
            else LoadDataWithBaseURL("", View.GetExecutableHtml().OrEmpty(), "text/html", "utf-8", "");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Client?.Dispose();
            Client = null;
            View = null;
        }

        public override bool OnTouchEvent(MotionEvent eventArgs)
        {
            //Enable scrolling inside WebView when placed in a ScrollView
            RequestDisallowInterceptTouchEvent(disallowIntercept: true);
            return base.OnTouchEvent(eventArgs);
        }
    }

    class AndroidWebViewClient : WebViewClient
    {
        public AndroidWebView WebView;

        [Preserve]
        public AndroidWebViewClient() : base() { }

        public override async void OnPageFinished(Android.Webkit.WebView native, string url)
        {
            await WebView.View.LoadFinished.RaiseOn(Device.ThreadPool);

            var absoluteUri = new Uri(url).AbsoluteUri;
            if (absoluteUri != WebView.View.Url)
            {
                var html = await EvaluateJavascript("document.body.innerHTML");
                WebView.View.OnBrowserNavigated(url, html);
            }

            base.OnPageFinished(native, url);
        }

        public override async void OnReceivedError(Android.Webkit.WebView native, [GeneratedEnum] ClientError errorCode,
            string description, string failingUrl)
        {
            await WebView.View.LoadingError.RaiseOn(Device.ThreadPool, description);
            base.OnReceivedError(native, errorCode, description, failingUrl);
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView native, string url)
        {
            if (url.HasValue() && WebView.View.OnBrowserNavigating(url)) return true;
            return base.ShouldOverrideUrlLoading(native, url);
        }

        public Task<string> EvaluateJavascript(string script)
        {
            WebView.LoadUrl("javascript:JsInterface.Run(" + script + ")");
            return WebView.JavascriptInterface.TaskSource.Task;
        }

        public void EvaluateJavascriptFunction(string function, string[] args)
        {
            WebView.LoadUrl("javascript:" + function + "(" + args.ToString(",") + ")");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WebView = null;
        }
    }

    class JavaScriptResult : Java.Lang.Object
    {
        Zebble.WebView View;

        public TaskCompletionSource<string> TaskSource = new TaskCompletionSource<string>();

        public JavaScriptResult(Zebble.WebView view) => View = view;

        public JavaScriptResult(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }

        [Export, JavascriptInterface]
        public void Run(string scriptResult)
        {
            var oldSource = TaskSource;
            TaskSource = new TaskCompletionSource<string>();
            oldSource.TrySetResult(scriptResult);
        }

        protected override void Dispose(bool disposing)
        {
            View = null;
            TaskSource = null;
            base.Dispose(disposing);
        }
    }
}
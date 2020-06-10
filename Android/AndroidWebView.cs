namespace Zebble
{
    using Android.Runtime;
    using Android.Views;
    using System;
    using System.Threading.Tasks;
    using Zebble.AndroidOS;

    class AndroidWebView : Android.Webkit.WebView, IZebbleAndroidControl
    {
        AndroidWebViewClient Client;

        public Zebble.WebView View;
        public JavaScriptResult JavascriptInterface;
        internal bool Dead => View == null || View.IsDisposing;

        [Preserve]
        public AndroidWebView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public AndroidWebView(Zebble.WebView view) : base(UIRuntime.CurrentActivity)
        {
            try
            {
                View = view;

                Settings.JavaScriptEnabled = true;
                AddJavascriptInterface(JavascriptInterface = new JavaScriptResult(View), "JsInterface");
                SetWebViewClient(Client = new AndroidWebViewClient { WebView = this });

                View.SourceChanged.HandleOnUI( Refresh);
                View.EvaluatedJavascript += s => Thread.UI.Run(() => EvaluateJavascript(s));
                View.InvokeJavascriptFunction += (s, a) => Thread.UI.Run(() => EvaluateJavascriptFunction(s, a));

                Refresh();
            }
            catch (Exception ex)
            {
                Zebble.Alert.Show(ex.Message);
            }
        }

        public Task<Android.Views.View> Render() => Task.FromResult<Android.Views.View>(this);

        async Task<string> EvaluateJavascript(string script) => await Client.EvaluateJavascript(script);

        void EvaluateJavascriptFunction(string function, string[] args) => Client.EvaluateJavascriptFunction(function, args);

        void Refresh()
        {
            if (Dead) return;

            if (View.Url?.Contains(":") == true) LoadUrl(View.Url);
            else
            {
                var html = View.GetExecutableHtml().OrEmpty();
                LoadDataWithBaseURL("", html, "text/html", "utf-8", "");
            }
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
            // Enable scrolling inside WebView when placed in a ScrollView
            RequestDisallowInterceptTouchEvent(disallowIntercept: true);
            return base.OnTouchEvent(eventArgs);
        }
    }
}
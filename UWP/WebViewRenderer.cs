namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using controls = Windows.UI.Xaml.Controls;
    using Olive;

    partial class WebViewRenderer : INativeRenderer
    {
        WebView View;
        controls.WebView Result;

        public async Task<FrameworkElement> Render(Renderer renderer)
        {
            View = (WebView)renderer.View;

            View.SourceChanged.HandleOn(Thread.UI, () => Reload());
            View.EvaluatedJavascript += x => Thread.UI.Run(() => EvaluateJavascript(x));
            View.InvokeJavascriptFunction += (s, a) => Thread.UI.Post(() => EvaluateJavascriptFunction(s, a));
            CreateBrowser();
            Reload();

            return Result;
        }

        void CreateBrowser()
        {
            SetDefaultUserAgent();

            Result = new controls.WebView(controls.WebViewExecutionMode.SeparateThread);
            Result.Loaded += async (s, e) => await View.LoadFinished.RaiseOn(Thread.Pool);

            Result.NavigationStarting += (s, e) =>
            {
                if (e.Uri != null && View.OnBrowserNavigating(e.Uri.ToString())) Result.Stop();
            };

            Result.NavigationCompleted += async (s, e) =>
            {
                if (View.BrowserNavigated.IsHandled())
                {
                    var html = (await EvaluateJavascript("document.documentElement.outerHTML")).ToStringOrEmpty();
                    var url = e.Uri.ToStringOrEmpty();

                    Thread.Pool.RunAction(() => View.OnBrowserNavigated(url, html));
                }
            };

            Result.LoadCompleted += Browser_LoadCompleted;
            Result.NavigationFailed += Browser_NavigationFailed;
        }

        async void Browser_NavigationFailed(object sender, controls.WebViewNavigationFailedEventArgs args)
        {
            var error = args.WebErrorStatus.ToString();

            await View.LoadingError.RaiseOn(Thread.Pool, error);
        }

        void Browser_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs args)
        {
            // TODO: Find if it was an error, and raise the event.
        }

        Task<string> EvaluateJavascript(string script) => Result.InvokeScriptAsync("eval", new string[] { script }).AsTask();

        async void EvaluateJavascriptFunction(string function, string[] args)
        {
            try
            {
                await Result.InvokeScriptAsync(function, args).AsTask();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Exception from HRESULT: 0x80020101")
                {
                    Log.For(this).Error(ex, "Syntax error in the javascript invoking function '" + function + "' with params:\n" +
                        args.ToLinesString());
                }
                else
                {
                    Log.For(this).Error(ex, "EvaluateJavascriptFunction() failed.");
                }
            }
        }

        public controls.WebView Render() => Result;

        void Reload()
        {
            if (View.Url?.Contains(":") == true) Result.Navigate(GetUri());
            else Result.NavigateToString(View.GetExecutableHtml().OrEmpty());
        }

        public Uri GetUri()
        {
            var path = View.Url;

            if (path.Contains(":")) return path.AsUri();

            var notFond = Device.IO.AbsolutePath("Images/Icons/not-found.png").AsUri();

            path = path.OrEmpty();

            var file = Device.IO.File(path.TrimStart("/").Replace("/", "\\"));
            if (!file.Exists() && file.Extension.OrEmpty().ToLower().IsAnyOf(".gif", ".png", ".jpg", ".jpeg", ".webp"))
            {
                Log.For(this).Error("Image file does not exist: " + file);
                return notFond;
            }

            return file.FullName.AsUri();
        }

        public void Dispose()
        {
            Result = null;
            View = null;
        }
    }
}
namespace Zebble
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using controls = Windows.UI.Xaml.Controls;

    [EditorBrowsable(EditorBrowsableState.Never)]
    partial class WebViewRenderer : INativeRenderer
    {
        WebView View;
        controls.WebView Result;

        public async Task<FrameworkElement> Render(Zebble.Renderer renderer)
        {
            View = (WebView)renderer.View;

            View.SourceChanged.HandleOn(Device.UIThread, () => Reload());
            View.EvaluatedJavascript += x => Device.UIThread.Run(() => EvaluateJavascript(x));
            View.EvaluatedJavascriptFunction += (s, a) => Device.UIThread.Run(() => EvaluateJavascriptFunction(s, a));
            CreateBrowser();
            Reload();

            return Result;
        }

        void CreateBrowser()
        {
            SetDefaultUserAgent();

            Result = new controls.WebView(controls.WebViewExecutionMode.SeparateThread);
            Result.Loaded += async (s, e) => await View.LoadFinished.RaiseOn(Device.ThreadPool);

            Result.NavigationStarting += (s, e) =>
            {
                if (e.Uri != null && View.OnBrowserNavigating(e.Uri.ToString())) Result.Stop();
            };

            Result.NavigationCompleted += async (s, e) =>
            {
                if (View.BrowserNavigated != null)
                {
                    var html = (await EvaluateJavascript("document.documentElement.outerHTML")).ToStringOrEmpty();
                    var url = e.Uri.ToStringOrEmpty();

                    Device.ThreadPool.RunAction(() => View.OnBrowserNavigated(url, html));
                }
            };

            Result.LoadCompleted += Browser_LoadCompleted;
            Result.NavigationFailed += Browser_NavigationFailed;
        }

        async void Browser_NavigationFailed(object sender, controls.WebViewNavigationFailedEventArgs args)
        {
            var error = args.WebErrorStatus.ToString();

            await View.LoadingError.RaiseOn(Device.ThreadPool, error);
        }

        void Browser_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs args)
        {
            // TODO: Find if it was an error, and raise the event.
        }

        Task<string> EvaluateJavascript(string script) => Result.InvokeScriptAsync("eval", new string[] { script }).AsTask();

        Task<string> EvaluateJavascriptFunction(string function, string[] args)
        {
            return Result.InvokeScriptAsync(function, args).AsTask();
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
                Device.Log.Error("Image file does not exist: " + file);
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
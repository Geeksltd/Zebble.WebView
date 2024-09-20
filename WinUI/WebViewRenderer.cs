namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.Web.WebView2.Core;
    using controls = Microsoft.UI.Xaml.Controls;
    using Olive;
    using System.Linq;

    partial class WebViewRenderer : INativeRenderer
    {
        WebView View;
        controls.WebView2 Result;  // Use WebView2 instead of WebView

        public async Task<FrameworkElement> Render(Renderer renderer)
        {
            View = (WebView)renderer.View;

            View.SourceChanged.HandleOn(Thread.UI, () => Reload());
            View.EvaluatedJavascript += x => Thread.UI.Run(() => EvaluateJavascript(x));
            View.InvokeJavascriptFunction += (s, a) => Thread.UI.Post(() => EvaluateJavascriptFunction(s, a));

            Result = new controls.WebView2();
            await Result.EnsureCoreWebView2Async();  // Ensure WebView2 is initialized

            // Handle navigation events after CoreWebView2 is initialized
            Result.CoreWebView2.NavigationStarting += (s, e) =>
            {
                if (e.Uri != null && View.OnBrowserNavigating(e.Uri.ToString()))
                    Result.CoreWebView2.Stop();
            };

            Result.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (View.BrowserNavigated.IsHandled())
                {
                    var html = (await EvaluateJavascript("document.documentElement.outerHTML")).ToStringOrEmpty();
                    var url = Result.Source?.ToStringOrEmpty() ?? string.Empty;  // Use Result.Source for URL

                    Thread.Pool.RunAction(() => View.OnBrowserNavigated(url, html));
                }
            };

            Reload();

            return Result;
        }

        Task<string> EvaluateJavascript(string script)
        {
            return Result.ExecuteScriptAsync(script).AsTask();  // WebView2 ExecuteScriptAsync for JavaScript execution
        }

        async void EvaluateJavascriptFunction(string function, string[] args)
        {
            try
            {
                string script = $"{function}({string.Join(",", args.Select(x => $"\"{x}\""))});";
                await Result.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("0x80020101"))
                {
                    Log.For(this).Error(ex, $"Syntax error in the JavaScript invoking function '{function}' with params:\n" + args.ToLinesString());
                }
                else
                {
                    Log.For(this).Error(ex, "EvaluateJavascriptFunction() failed.");
                }
            }
        }

        public controls.WebView2 Render() => Result;  // Return WebView2

        void Reload()
        {
            var url = View.Url;
            if (url.IsEmpty()) return;

            if (url.IsUrl())
                Result.Source = new Uri(url);  // Use Uri object for WebView2
            else
                Result.NavigateToString(View.GetExecutableHtml().OrEmpty());
        }

        public Uri GetUri()
        {
            var path = View.Url;

            if (path.Contains(":")) return new Uri(path);

            var notFound = new Uri(Device.IO.AbsolutePath("Images/Icons/not-found.png"));

            var file = Device.IO.File(path.TrimStart("/").Replace("/", "\\"));
            if (!file.Exists() && file.Extension.OrEmpty().ToLower().IsAnyOf(".gif", ".png", ".jpg", ".jpeg", ".webp"))
            {
                Log.For(this).Error("Image file does not exist: " + file);
                return notFound;
            }

            return new Uri(file.FullName);
        }

        public void Dispose()
        {
            Result = null;
            View = null;

            GC.SuppressFinalize(this);
        }
    }
}
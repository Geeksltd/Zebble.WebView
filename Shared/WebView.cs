﻿namespace Zebble
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Olive;

    public partial class WebView : View, IRenderedBy<WebViewRenderer>, FormField.IControl
    {
        string url, html, ResourceNamespace;
        Assembly ResourceAssembly;
        public readonly AsyncEvent BrowserNavigated = new AsyncEvent();
        public readonly AsyncEvent<NavigatingEventArgs> BrowserNavigating = new AsyncEvent<NavigatingEventArgs>();

        /// <summary>
        /// Parameters are Error code and error text.
        /// </summary>
        public readonly AsyncEvent<string> LoadingError = new AsyncEvent<string>();
        public readonly AsyncEvent LoadFinished = new AsyncEvent();
        public readonly AsyncEvent SourceChanged = new AsyncEvent();

        internal Func<string, Task<string>> EvaluatedJavascript;
        internal Action<string, string[]> InvokeJavascriptFunction;

        public WebView() => Css.Height(100);
        public WebView(string url) => Url = url;
        public WebView(Assembly resourceAssembly, string resourceNamespace)
        {
            ResourceAssembly = resourceAssembly;
            ResourceNamespace = resourceNamespace;
        }

        public bool MergeExternalResources { get; set; } = true;

        public object Value { get => html; set => html = value.ToStringOrEmpty(); }

        public string Html
        {
            get => html;
            set => SetHtml(value).RunInParallel();
        }

        public Task SetHtml(string value)
        {
            html = value;
            if (value.HasValue()) url = null;
            return SourceChanged.Raise();
        }

        public string Url { get => url; set => SetUrl(value); }

        public Task SetUrl(string value)
        {
            url = value;
            if (value.HasValue()) html = null;
            return SourceChanged.Raise();
        }

        public void OnBrowserNavigated(string newUrl, string newContent)
        {
            url = newUrl;
            html = newContent;
            BrowserNavigated.Raise();
        }

        public bool OnBrowserNavigating(string newUrl)
        {
            if (!BrowserNavigating.IsHandled()) return false;
            var eventArgs = new NavigatingEventArgs { Cancel = false, Url = newUrl };
            BrowserNavigating.Raise(eventArgs);
            return eventArgs.Cancel;
        }

        public void EvaluateJavaScript(string script) => EvaluatedJavascript?.Invoke(script);

        /// <summary>
        /// Evaluates a javascript block of code and returns its result
        /// </summary>
        /// <param name="script">The code to evaluate</param>
        /// <returns>The result of the evaluated code</returns>
        public Task<string> EvaluateJavaScriptAsync(string script) => EvaluatedJavascript?.Invoke(script);

        public void EvaluateJavaScriptFunction(string function, string[] args) => InvokeJavascriptFunction?.Invoke(function, args);

        public string GetExecutableHtml()
        {
            try
            {
                var html = Html;

                if (html.IsEmpty())
                {
                    if (Url.IsEmpty() || Url.Contains(":")) return null;

                    var file = Device.IO.File(Url);
                    if (!file.Exists()) return "File not found: " + Url;
                    html = file.ReadAllText();
                }

                if (html.Lacks("<html", caseSensitive: false)) return html;
                html = html.RemoveBefore("<html", caseSensitive: false);

                return new ResourceInliner(html)
                {
                    ResourceAssembly = ResourceAssembly,
                    ResourceNamespace = ResourceNamespace,
                    MergeExternalResources = MergeExternalResources
                }
                .Inline();
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex;
            }
        }

        public override void Dispose()
        {
            LoadingError?.Dispose();
            LoadFinished?.Dispose();
            BrowserNavigated?.Dispose();
            SourceChanged?.Dispose();

            EvaluatedJavascript = null;
            InvokeJavascriptFunction = null;

            base.Dispose();
        }
    }
}
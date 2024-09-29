namespace Zebble;

using Android.Runtime;
using Android.Webkit;
using Java.Interop;
using System;
using System.Threading.Tasks;

class JavaScriptResult : Java.Lang.Object
{
    Zebble.WebView View;

    public TaskCompletionSource<string> TaskSource = new();

    public JavaScriptResult(Zebble.WebView view) => View = view;

    [Preserve]
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
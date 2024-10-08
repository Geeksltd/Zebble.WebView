﻿namespace Zebble;

public partial class WebView
{
    internal readonly AsyncEvent ScrollBouncesChanged = new();

    bool scrollBounces;
    public bool Bounces
    {
        get => scrollBounces;
        set
        {
            scrollBounces = value;
            ScrollBouncesChanged.Raise();
        }
    }
}
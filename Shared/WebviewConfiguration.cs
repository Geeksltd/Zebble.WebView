using System;
using System.Collections.Generic;
using System.Text;

namespace Zebble
{
    public class WebViewConfiguration
    {
        public bool AllowsInlineMediaPlayback { get; set; }
        public bool MediaTypesRequiringUserActionForPlayback { get; set; } = false;
    }
}

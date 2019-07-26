using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace PassboltSharp.Helpers
{
    internal static class HttpHelper
    {
        internal static bool TryGetValue(this HttpResponseHeaders headers, string key, out string value)
        {
            value = null;
            if (!headers.TryGetValues(key, out var result))
                return false;

            value = result.FirstOrDefault();
            return true;
        }

        internal static string GetValue(this HttpResponseHeaders headers, string key)
        {
            return headers.GetValues(key).FirstOrDefault();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace PassboltSharp.Helpers
{
    internal static class Validator
    {
        internal static bool ValueEquals(this HttpResponseHeaders headers, string key, string value)
        {
            return headers.TryGetValue(key, out var test) && test == value;
        }
    }
}

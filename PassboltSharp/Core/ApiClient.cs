using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PassboltSharp.Models;

namespace PassboltSharp.Core
{
    /// <summary>
    /// Implementation of a Passbolt API client.
    /// </summary>
    public class ApiClient
    {
        internal readonly Uri Path;
        internal readonly string Version = "v2";
        internal readonly ILogger Logger = null;

        public ApiClient(Uri apiPath)
        {
            Path = apiPath;
        }

        public ApiSession GetSession() => new ApiSession(this);
    }
}

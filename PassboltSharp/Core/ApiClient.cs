using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PassboltSharp.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of a Passbolt API client.
    /// </summary>
    internal class ApiClient : IDisposable
    {
        private readonly Uri _apiPath;
        private readonly HttpClientHandler _handler;
        internal readonly HttpClient Client;
        internal readonly ILogger Logger = null;

        private readonly string _apiVersion;
        public HttpRequestHeaders Headers => Client.DefaultRequestHeaders;

        public CookieContainer Cookies
        {
            get => _handler.CookieContainer;
            private set => _handler.CookieContainer = value;
        }

        public ApiClient(Uri apiPath, string apiVersion = "v2")
        {
            _apiPath = apiPath;
            _apiVersion = apiVersion;
            _handler = new HttpClientHandler {CookieContainer = new CookieContainer()};
            Client = new HttpClient(_handler);
        }

        public void ResetCookies()
        {
            Cookies = new CookieContainer();
        }

        public string GetCsrfToken()
        {
            var collection = Cookies.GetCookies(new Uri(_apiPath, "/csrfToken"));
            return collection.Count != 1 ? null : Cookies.GetCookies(new Uri(_apiPath, "/csrfToken"))[0].Value;
        }

        /// <summary>
        /// Gets data from the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Get(string path) =>
            ApiRequest.Build(this, new Uri(_apiPath, path + GetVersionQuery()), HttpMethod.Get);

        /// <summary>
        /// Performs a post operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <param name="obj">Object to send to the server.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Post(string path, object obj) =>
            ApiRequest.Build(this, new Uri(_apiPath, path + GetVersionQuery()), HttpMethod.Post).WithJson(obj);

        /// <summary>
        /// Performs a put operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <param name="obj">Object to send to the server.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Put(string path, object obj) =>
            ApiRequest.Build(this, new Uri(_apiPath, path + GetVersionQuery()), HttpMethod.Put).WithJson(obj);

        /// <summary>
        /// Performs a delete operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Delete(string path) =>
            ApiRequest.Build(this, new Uri(_apiPath, path + GetVersionQuery()), HttpMethod.Get);

        private string GetVersionQuery() => $"?api-version={_apiVersion}";

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}

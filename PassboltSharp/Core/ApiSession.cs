using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PassboltSharp.Core.Auth;
using PassboltSharp.Models;

namespace PassboltSharp.Core
{
    public class ApiSession : IDisposable
    {
        private readonly ApiClient _apiClient;

        internal readonly HttpClient Client;
        private readonly HttpClientHandler _handler;

        public CookieContainer Cookies
        {
            get => _handler.CookieContainer;
            private set => _handler.CookieContainer = value;
        }

        public HttpRequestHeaders Headers => Client.DefaultRequestHeaders;

        /// <summary>
        /// Gets the parent <see cref="ApiClient"/>'s <see cref="ILogger"/>.
        /// </summary>
        internal ILogger Logger => _apiClient.Logger;

        public ApiSession(ApiClient apiClient)
        {
            _apiClient = apiClient;

            _handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            Client = new HttpClient(_handler);
        }

        public void ResetCookies()
        {
            Cookies = new CookieContainer();
        }

        public string GetCsrfToken()
        {
            var collection = Cookies.GetCookies(_apiClient.Path);
            return (from Cookie cookie in collection
                where cookie.Name == "csrfToken"
                select cookie.Value).FirstOrDefault();
        }

        private string GetVersionQuery() => $"?api-version={_apiClient.Version}";

        /// <summary>
        /// Gets data from the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Get(string path) =>
            ApiRequest.Build(this, new Uri(_apiClient.Path, path + GetVersionQuery()), HttpMethod.Get);

        /// <summary>
        /// Performs a post operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <param name="obj">Object to send to the server.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Post(string path, object obj) =>
            ApiRequest.Build(this, new Uri(_apiClient.Path, path + GetVersionQuery()), HttpMethod.Post).WithJson(obj);

        /// <summary>
        /// Performs a put operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <param name="obj">Object to send to the server.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Put(string path, object obj) =>
            ApiRequest.Build(this, new Uri(_apiClient.Path, path + GetVersionQuery()), HttpMethod.Put).WithJson(obj);

        /// <summary>
        /// Performs a delete operation on the server.
        /// </summary>
        /// <param name="path">Relative path of the API from the root path.</param>
        /// <returns>API response with header, data, and HTTP results.</returns>
        internal ApiRequest Delete(string path) =>
            ApiRequest.Build(this, new Uri(_apiClient.Path, path + GetVersionQuery()), HttpMethod.Get);

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}

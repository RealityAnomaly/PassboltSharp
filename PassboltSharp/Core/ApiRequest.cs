using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PassboltSharp.Core
{
    internal class ApiRequest
    {
        private readonly ApiSession _session;
        private readonly HttpRequestMessage _message;

        private ApiRequest(ApiSession session, HttpRequestMessage message)
        {
            _session = session;
            _message = message;
        }

        internal static ApiRequest Build(ApiSession session, Uri path, HttpMethod method)
        {
            return new ApiRequest(session, new HttpRequestMessage(method, path));
        }

        internal ApiRequest WithJson(object value)
        {
            _message.Content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
            return this;
        }

        /// <summary>
        /// Adds a CSRF (cross-site request forgery) token to the request headers.
        /// </summary>
        internal ApiRequest WithCsrfToken() => WithHeader("X-CSRF-Token", _session.GetCsrfToken());

        /// <summary>
        /// Adds a header to the request.
        /// </summary>
        /// <param name="key">Name of the header.</param>
        /// <param name="value">Value of the header.</param>
        internal ApiRequest WithHeader(string key, string value)
        {
            _message.Headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Adds a header with multiple values to the request.
        /// </summary>
        /// <param name="key">Name of the header.</param>
        /// <param name="values">Values of the header.</param>
        internal ApiRequest WithHeaders(string key, IEnumerable<string> values)
        {
            _message.Headers.Add(key, values);
            return this;
        }

        /// <summary>
        /// Asynchronously invokes the request and returns the response.
        /// </summary>
        /// <typeparam name="T">Type of the JSON object to deserialise.</typeparam>
        internal async Task<ApiResponse<T>> SendAsync<T>()
        {
            var response = await _session.Client.SendAsync(_message);
            return await ApiResponse<T>.BuildAsync(response);
        }

        /// <summary>
        /// Asynchronously invokes the request and returns a raw JObject as the body.
        /// </summary>
        internal async Task<ApiResponse<JObject>> SendAsync()
        {
            var response = await _session.Client.SendAsync(_message);
            return await ApiResponse<JObject>.BuildAsync(response);
        }
    }
}

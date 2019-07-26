using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassboltSharp.Core
{
    internal class ApiResponse<T>
    {
        internal readonly HttpResponseMessage Response;

        internal ApiResponseHeader Header;
        internal T Body;

        private ApiResponse(HttpResponseMessage response)
        {
            Response = response;
        }

        internal void ThrowIfFailed()
        {
            if (Response.IsSuccessStatusCode) return;

            if (Header == null || string.IsNullOrWhiteSpace(Header.Message)) throw new Exception($"The request to the API failed without any server information. Error code: {Response.StatusCode}");
            throw new Exception(Header.Message);
        }

        internal static async Task<ApiResponse<T>> BuildAsync(HttpResponseMessage response)
        {
            var obj = new ApiResponse<T>(response);
            JsonConvert.PopulateObject(await response.Content.ReadAsStringAsync(), obj);

            return obj;
        }
    }
}

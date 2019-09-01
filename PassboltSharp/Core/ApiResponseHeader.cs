using System;
using Newtonsoft.Json;

namespace PassboltSharp.Core
{
    internal struct ApiResponseHeader
    {
        [JsonProperty("id")]
        internal Guid Id;
        [JsonProperty("code")]
        internal int Code;
        [JsonProperty("message")]
        internal string Message;
        [JsonProperty("servertime")]
        internal long ServerTime;
        [JsonProperty("status")]
        internal string Status;
        [JsonProperty("title")]
        internal string Title;
        [JsonProperty("url")]
        internal string Url;
    }
}

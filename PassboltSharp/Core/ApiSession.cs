using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassboltSharp.Core.Auth;
using PassboltSharp.Models;

namespace PassboltSharp.Core
{
    public class ApiSession : IDisposable
    {
        private readonly ApiClient _client;

        // State
        internal ApiState State = ApiState.None;
        private GpgAuth _auth;

        public ApiSession(Uri apiPath)
        {
            _client = new ApiClient(apiPath);
        }

        /// <summary>
        /// Authenticates this API session to the server.
        /// </summary>
        public async Task Authenticate(string passphrase)
        {
            await _auth.Authenticate(passphrase);

            State = ApiState.Authenticated;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}

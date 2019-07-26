using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace PassboltSharp.Core.Auth
{
    /// <summary>
    /// Implementation of Passbolt MFA (multi-factor authentication)
    /// </summary>
    internal class MfaAuth
    {
        // API paths
        private const string URL_MFA = "/mfa";
        private const string URL_MFA_VERIFY_TOTP = URL_MFA + "/verify/totp.json";
        private const string URL_MFA_VERIFY_YUBIKEY = URL_MFA + "/verify/yubikey.json";

        private ApiClient _client;
        private MfaAuthProviderType _provider;

        internal MfaAuth(ApiClient client)
        {
            _client = client;
        }

        /**
            internal void GetMfaChallenge(out object response)
            {
            
            }
            */

        /// <summary>
        /// Verifies the MFA challenge with the specified response.
        /// </summary>
        /// <param name="otp">The one-time password from the user's security token.</param>
        /// <returns>Whether the server accepted the response.</returns>
        internal async Task<bool> VerifyMfaChallenge(string otp)
        {
            _client.Headers.Add("X-CSRF-Token", _client.GetCsrfToken());
            var response = await _client.Post(GetVerifyApiPath(), GetVerifyPostData(otp))
                .WithCsrfToken()
                .SendAsync<JObject>();

            if (!response.Response.IsSuccessStatusCode)
            {
                _client.Logger?.LogError($"There was a problem with MFA authentication. Server returned code {response.Response.StatusCode}.");
                return false;
            }

            return true;
        }

        internal bool IsMfaRequired(ApiResponse<JObject> response)
        {
            if (response.Response.StatusCode != HttpStatusCode.Forbidden) return false;
            if (!response.Header.Url.StartsWith("/mfa/verify")) return false;
            return true;
        }

        /// <summary>
        /// Selects the provider from the providers specified in the response.
        /// You should call <see cref="IsMfaRequired"/> first to check if the server is requesting MFA.
        /// </summary>
        /// <param name="response">The API response.</param>
        internal void SelectFrom(ApiResponse<JObject> response)
        {
            // Deserialise provider types and add them to the list
            var types = response.Body["providers"].Select(p => (MfaAuthProviderType)Enum.Parse(typeof(MfaAuthProviderType), p.ToObject<string>().ToLower())).ToList();
            _provider = GetSelectedProvider(types);
        }

        internal MfaAuthProviderType GetSelectedProvider(IList<MfaAuthProviderType> providers)
        {
            if (!providers.Any())
                throw new Exception("The server requested MFA, but provided no valid MFA providers for use.");

        }

        /// <summary>
        /// Returns formatted data to post to the server.
        /// </summary>
        /// <param name="otp">The one-time password.</param>
        /// <returns>The formatted data.</returns>
        internal Dictionary<string, dynamic> GetVerifyPostData(string otp)
        {
            switch (_provider)
            {
                case MfaAuthProviderType.YubiKey:
                    return new Dictionary<string, dynamic> {{"hotp", otp}};
                case MfaAuthProviderType.Totp:
                    return new Dictionary<string, dynamic> {{"totp", otp}};
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the API path for verification by the MFA auth type.
        /// </summary>
        /// <returns>The relative API path.</returns>
        internal string GetVerifyApiPath()
        {
            switch (_provider)
            {
                case MfaAuthProviderType.YubiKey:
                    return URL_MFA_VERIFY_YUBIKEY;
                case MfaAuthProviderType.Totp:
                    return URL_MFA_VERIFY_TOTP;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

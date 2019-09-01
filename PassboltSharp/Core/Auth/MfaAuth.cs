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
    public class MfaAuth
    {
        // API paths
        private const string URL_MFA = "/mfa";
        private const string URL_MFA_VERIFY_ERROR = URL_MFA + "/verify/error.json";
        private const string URL_MFA_VERIFY_TOTP = URL_MFA + "/verify/totp.json";
        private const string URL_MFA_VERIFY_YUBIKEY = URL_MFA + "/verify/yubikey.json";

        private readonly ApiSession _session;
        private readonly MfaAuthProviderType _type;

        /// <summary>
        /// Performs multifactor authentication on a partially authenticated GpgAuth session.
        /// </summary>
        /// <param name="session">The session on which authentication is in progress.</param>
        /// <param name="type">The type of authentication provider to use.</param>
        public MfaAuth(ApiSession session, MfaAuthProviderType type)
        {
            _session = session;
            _type = type;
        }

        /// <summary>
        /// Verifies the MFA challenge with the specified response.
        /// </summary>
        /// <param name="otp">The one-time password from the user's security token.</param>
        /// <returns>Whether the server accepted the response.</returns>
        public async Task<bool> VerifyMfaChallenge(string otp)
        {
            var response = await _session.Post(GetVerifyApiPath(), GetVerifyPostData(otp))
                .WithCsrfToken()
                .SendAsync<JObject>();

            if (!response.Response.IsSuccessStatusCode)
            {
                _session.Logger?.LogError($"There was a problem with MFA authentication. Server returned code {response.Response.StatusCode}.");
                return false;
            }

            return true;
        }

        internal static bool IsMfaRequired(ApiResponse<JObject> response)
        {
            if (response.Response.StatusCode != HttpStatusCode.Forbidden) return false;
            if (response.Header?.Url != URL_MFA_VERIFY_ERROR) return false;
            return true;
        }

        internal static IList<MfaAuthProviderType> GetProviderTypesFrom(ApiResponse<JObject> response)
        {
            var providers = new List<MfaAuthProviderType>();
            foreach (var p in response.Body["providers"])
            {
                var str = p.ToObject<string>().ToLower();
                if (str.EndsWith(URL_MFA_VERIFY_TOTP))
                    providers.Add(MfaAuthProviderType.Totp);
                else if (str.EndsWith(URL_MFA_VERIFY_YUBIKEY))
                    providers.Add(MfaAuthProviderType.YubiKey);
                else
                    throw new Exception($"Unknown MFA provider '{p}'.");
            }

            return providers;
        }

        /// <summary>
        /// Returns formatted data to post to the server.
        /// </summary>
        /// <param name="otp">The one-time password.</param>
        /// <returns>The formatted data.</returns>
        private Dictionary<string, dynamic> GetVerifyPostData(string otp)
        {
            switch (_type)
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
        private string GetVerifyApiPath()
        {
            switch (_type)
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

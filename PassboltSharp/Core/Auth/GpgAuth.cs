using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PassboltSharp.Helpers;
using PassboltSharp.Models;
using PgpCore;

namespace PassboltSharp.Core.Auth
{
    /// <summary>
    /// Implementation of the GpgAuth protocol.
    /// </summary>
    internal class GpgAuth : IDisposable
    {
        // API paths
        private const string URL_AUTH = "/auth";
        private const string URL_VERIFY = URL_AUTH + "/verify.json";
        private const string URL_CHECKSESSION = URL_AUTH + "/checkSession.json";
        private const string URL_LOGIN = URL_AUTH + "/login.json";
        private const string URL_LOGOUT = URL_AUTH + "/logout.json";

        // Crypto
        private readonly GpgKey _clientKey;
        private readonly GpgKey _serverKey;
        private readonly PGP _pgp;

        // State
        private readonly ApiClient _client;
        private readonly MfaAuth _mfa;
        private GpgAuthState _state = GpgAuthState.Logout;

        public GpgAuth(ApiClient client, GpgKey clientKey, GpgKey serverKey)
        {
            _pgp = new PGP();
            _clientKey = clientKey;
            _serverKey = serverKey;

            _client = client;
            _mfa = new MfaAuth(_client);
        }

        /// <summary>
        /// Authenticates the user using GpgAuth.
        /// </summary>
        /// <param name="passphrase">Passphrase protecting the user's private key.</param>
        public async Task<GpgAuthSessionState> Authenticate(string passphrase)
        {
            try
            {
                var result = await VerifySession();
                switch (result)
                {
                    case GpgAuthSessionState.Invalid:
                        _client.Logger?.LogDebug($"GpgAuth session invalid. Starting authentication with client fingerprint {_clientKey.Fingerprint}.");

                        // Get and decrypt the token. (Stage 1)
                        var token = await GetAndDecryptToken(passphrase);
                        _client.Logger?.LogDebug("GpgAuth Token decrypted successfully.");

                        // Verify it with the server. (Stage 2)
                        await VerifyToken(token);
                        _client.Logger?.LogDebug("Server accepts the GpgAuth token.");

                        // Check to ensure the session is now valid
                        var check = await Authenticate(passphrase);
                        if (check == GpgAuthSessionState.Invalid)
                            throw new Exception("Server returned an invalid session. Halting authentication.");
                        return check;
                    case GpgAuthSessionState.MfaRequired:
                        _client.Logger?.LogDebug("Server returned GpgAuth MFA required.");
                        // MFA is required
                        // The user must call GetMfaChallenge and respond to it with VerifyMfaChallenge
                        // Then they can call Authenticate again and get the Valid state returned
                        return result;
                    case GpgAuthSessionState.Valid:
                        _client.Logger?.LogDebug("Server GpgAuth session is valid.");
                        return result;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _client.Logger?.LogError(e, e.Message);
                return GpgAuthSessionState.Invalid;
            }
        }

        public async Task GetMfaChallenge()
        {

        }

        public async Task VerifyMfaChallenge()
        {

        }

        /// <summary>
        /// Logs the user out using GpgAuth.
        /// </summary>
        public async Task<bool> Logout()
        {
            _state = GpgAuthState.Logout;

            try
            {
                var result = await _client.Get(URL_LOGOUT).SendAsync();
                result.ThrowIfFailed();
                _client.Logger?.LogInformation("Successfully logged out of the GpgAuth session.");

                return true;
            }
            catch (Exception e)
            {
                _client.Logger?.LogError(e, "An exception occurred while logging out of the GpgAuth session.");
                return false;
            }
            finally
            {
                _client.ResetCookies();
            }
        }

        /// <summary>
        /// Validates the server's key by asking it to authenticate to the client.
        /// </summary>
        private async Task VerifyServer()
        {
            var token = GpgAuthToken.NewToken();
            var encryptedToken = await _pgp.EncryptStringAsync(token.Token, _serverKey);

            var auth = new Dictionary<string, dynamic>
            {
                {"gpg_auth", new Dictionary<string, dynamic>
                {
                    {"keyid", _clientKey.Fingerprint},
                    {"server_verify_token", encryptedToken}
                }}
            };

            var result = await _client.Post(URL_LOGIN, auth).SendAsync();
            result.ThrowIfFailed();

            GpgAuthHeaders.Validate(result.Response.Headers, GpgAuthState.DecryptToken);
            var verifyToken = new GpgAuthToken(result.Response.Headers.GetValue("x-gpgauth-verify-response"));
            if (verifyToken.Token != token.Token)
                throw new Exception("The server failed to prove it can use the advertised OpenPGP key.");
        }

        /// <summary>
        /// Retrieves a token from the server, and decrypts it.
        /// </summary>
        /// <param name="passphrase">The passphrase to use to decrypt the client key.</param>
        /// <returns>The decrypted token.</returns>
        private async Task<GpgAuthToken> GetAndDecryptToken(string passphrase)
        {
            var auth = new Dictionary<string, dynamic>
            {
                {"gpg_auth", new Dictionary<string, dynamic>
                {
                    {"keyid", _clientKey.Fingerprint},
                }}
            };

            var result = await _client.Post(URL_LOGIN, auth).SendAsync();
            GpgAuthHeaders.Validate(result.Response.Headers, _state);

            // Decrypt the token
            var encrypted = result.Response.Headers.GetValue("x-gpgauth-user-auth-token");
            var decrypted = await _pgp.DecryptStringAsync(encrypted, _clientKey, passphrase);

            // Validate the token
            var token = new GpgAuthToken(decrypted);
            _state = GpgAuthState.VerifyToken;
            return token;
        }

        /// <summary>
        /// Sends the decrypted client token back to the server for verification.
        /// </summary>
        /// <param name="token">Decrypted client token, obtained from <see cref="GetAndDecryptToken"/>.</param>
        private async Task VerifyToken(GpgAuthToken token)
        {
            var auth = new Dictionary<string, dynamic>
            {
                {"gpg_auth", new Dictionary<string, dynamic>
                {
                    {"keyid", _clientKey.Fingerprint},
                    {"user_token_result", token.Token}
                }}
            };

            var result = await _client.Post(URL_LOGIN, auth).SendAsync();
            GpgAuthHeaders.Validate(result.Response.Headers, _state);

            // We're done, nice!
            _state = GpgAuthState.Complete;
        }

        private async Task<GpgAuthSessionState> VerifySession()
        {
            if (_client.Cookies.Count == 0)
                return GpgAuthSessionState.Invalid;

            try
            {
                var result = await _client.Get(URL_CHECKSESSION).SendAsync();
                _state = GpgAuthState.Complete;

                return _mfa.IsMfaRequired(result) ? GpgAuthSessionState.MfaRequired : GpgAuthSessionState.Valid;
            }
            catch (Exception e)
            {
                _client.Logger?.LogError(e, e.Message);
                return GpgAuthSessionState.Invalid;
            }
        }

        public void Dispose()
        {
            _pgp?.Dispose();
        }
    }
}

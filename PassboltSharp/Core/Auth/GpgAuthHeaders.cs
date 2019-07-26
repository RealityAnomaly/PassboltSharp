using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using PassboltSharp.Helpers;

namespace PassboltSharp.Core.Auth
{
    internal class GpgAuthHeaders
    {
        private readonly HttpResponseHeaders _headers;
        private readonly GpgAuthState _state;

        private GpgAuthHeaders(HttpResponseHeaders headers, GpgAuthState state)
        {
            _headers = headers;
            _state = state;
        }

        /// <summary>
        /// Validates the recieved headers are valid in this state.
        /// </summary>
        /// <param name="headers">The response headers.</param>
        /// <param name="state"></param>
        public static void Validate(HttpResponseHeaders headers, GpgAuthState state)
        {
            var validator = new GpgAuthHeaders(headers, state);
            validator.Validate();
        }

        private void Validate()
        {
            if (_headers == null || !_headers.Any(h => h.Key.StartsWith("x-gpgauth")))
                throw new Exception("No GPGAuth headers set.");

            if (!_headers.TryGetValue("x-gpgauth-version", out var version) || version != "1.3.0")
                throw new Exception($"The version of GPGAuth provided by the server ({version}) is not supported.");

            if (_headers.TryGetValue("x-gpgauth-error", out _))
            {
                if (_headers.TryGetValue("x-gpgauth-debug", out var debug))
                    throw new Exception(debug);

                throw new Exception("There was an error during authentication. Enable debug mode for more information.");
            }

            // Throws an exception if x-gpgauth-progress does not match our client state
            ThrowIfInvalidStage();

            // Verify the correct headers are set in each stage.
            switch (_state)
            {
                case GpgAuthState.Logout:
                    ThrowIfHeaderNotEquals("x-gpgauth-authenticated", "false");
                    break;
                case GpgAuthState.DecryptToken: // "Stage 1"
                    ThrowIfHeaderNotEquals("x-gpgauth-authenticated", "false");
                    ThrowIfHeaderSet("x-gpgauth-user-token");
                    ThrowIfHeaderNotSet("x-gpgauth-verify-response");
                    ThrowIfHeaderSet("x-gpgauth-refer");
                    break;
                case GpgAuthState.VerifyToken: // "Stage 2"
                    ThrowIfHeaderNotEquals("x-gpgauth-authenticated", "false");
                    ThrowIfHeaderNotSet("x-gpgauth-user-token");
                    ThrowIfHeaderSet("x-gpgauth-verify-response");
                    ThrowIfHeaderSet("x-gpgauth-refer");
                    break;
                case GpgAuthState.Complete:
                    ThrowIfHeaderNotEquals("x-gpgauth-authenticated", "true");
                    ThrowIfHeaderSet("x-gpgauth-user-token");
                    ThrowIfHeaderSet("x-gpgauth-verify-response");
                    ThrowIfHeaderNotSet("x-gpgauth-refer");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        /// <summary>
        /// Throws an exception if the header value is not <see cref="value"/>.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">The correct value of the header.</param>
        private void ThrowIfHeaderNotEquals(string key, string value)
        {
            if (!_headers.ValueEquals(key, value))
                throw new Exception($"{key} should be set to {value} during the {_state.ToString().ToLower()} stage.");
        }

        /// <summary>
        /// Throws an exception if the header value is set.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        private void ThrowIfHeaderSet(string key)
        {
            if (_headers.TryGetValue(key, out var value))
                throw new Exception($"{key} should not be set during the {_state.ToString().ToLower()} stage. Its value was {value}.");
        }

        /// <summary>
        /// Throws an exception if the header value is not set or is whitespace.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        private void ThrowIfHeaderNotSet(string key)
        {
            if (!_headers.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                throw new Exception($"{key} should be set during the {_state.ToString().ToLower()} stage.");
        }

        private void ThrowIfInvalidStage() => ThrowIfHeaderNotEquals("x-gpgauth-progress", ClientStateToServer(_state));

        private static string ClientStateToServer(GpgAuthState state)
        {
            switch (state)
            {
                case GpgAuthState.Logout:
                    return "logout";
                case GpgAuthState.DecryptToken:
                    return "stage0";
                case GpgAuthState.VerifyToken:
                    return "stage1";
                case GpgAuthState.Complete:
                    return "complete";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}

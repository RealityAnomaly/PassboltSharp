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
            if (_headers == null || !_headers.Any(h => h.Key.StartsWith("X-GPGAuth")))
                throw new Exception("No GPGAuth headers set.");

            if (!_headers.TryGetValue("X-GPGAuth-Version", out var version) || version != "1.3.0")
                throw new Exception($"The version of GPGAuth provided by the server ({version}) is not supported.");

            if (_headers.TryGetValue("X-GPGAuth-Error", out _))
            {
                if (_headers.TryGetValue("X-GPGAuth-Debug", out var debug))
                    throw new Exception(debug);

                throw new Exception("There was an error during authentication. Enable debug mode for more information.");
            }

            // Throws an exception if x-gpgauth-progress does not match our client state
            ThrowIfInvalidStage();

            // Verify the correct headers are set in each stage.
            switch (_state)
            {
                case GpgAuthState.Logout:
                    ThrowIfHeaderNotEquals("X-GPGAuth-Authenticated", "false");
                    break;
                case GpgAuthState.VerifyServer:
                    ThrowIfHeaderNotEquals("X-GPGAuth-Authenticated", "false");
                    ThrowIfHeaderSet("X-GPGAuth-User-Token");
                    ThrowIfHeaderNotSet("X-GPGAuth-Verify-Response");
                    ThrowIfHeaderSet("X-GPGAuth-Refer");
                    break;
                case GpgAuthState.DecryptToken: // "Stage 1"
                    ThrowIfHeaderNotEquals("X-GPGAuth-Authenticated", "false");
                    ThrowIfHeaderSet("X-GPGAuth-User-Token");
                    ThrowIfHeaderSet("X-GPGAuth-Verify-Response");
                    ThrowIfHeaderSet("X-GPGAuth-Refer");
                    break;
                case GpgAuthState.VerifyToken: // "Stage 2"
                    ThrowIfHeaderNotEquals("X-GPGAuth-Authenticated", "false");
                    ThrowIfHeaderNotSet("X-GPGAuth-User-Token");
                    ThrowIfHeaderSet("X-GPGAuth-Verify-Response");
                    ThrowIfHeaderSet("X-GPGAuth-Refer");
                    break;
                case GpgAuthState.Complete:
                    ThrowIfHeaderNotEquals("X-GPGAuth-Authenticated", "true");
                    ThrowIfHeaderSet("X-GPGAuth-User-Token");
                    ThrowIfHeaderSet("X-GPGAuth-Verify-Response");
                    ThrowIfHeaderNotSet("X-GPGAuth-Refer");
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
                case GpgAuthState.VerifyServer:
                    return "stage0";
                case GpgAuthState.DecryptToken:
                    return "stage1";
                case GpgAuthState.VerifyToken:
                    return "stage2";
                case GpgAuthState.Complete:
                    return "complete";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}

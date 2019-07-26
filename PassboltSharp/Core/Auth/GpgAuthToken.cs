using System;

namespace PassboltSharp.Core.Auth
{
    internal class GpgAuthToken
    {
        internal string Token;
        private const string Version = "gpgauthv1.3.0";

        internal GpgAuthToken(string token)
        {
            Validate(token);
            Token = token;
        }

        internal static GpgAuthToken NewToken()
        {
            return new GpgAuthToken($"{Version}|36|{Guid.NewGuid()}|{Version}");
        }

        private static void Validate(string token)
        {
            var sections = token.Split('|');
            if (sections.Length != 4)
                throw new Exception("The user authentication token is not in the right format.");
            if (sections[0] != sections[3] && sections[0] != Version)
                throw new Exception("PassboltSharp does not support this GPGAuth version.");
            if (sections[1] != "36")
                throw new Exception($"PassboltSharp does not support GPGAuth token nonce longer than 36 characters: {sections[2]}.");
            if (!Guid.TryParse(sections[2], out _))
                throw new Exception("PassboltSharp does not support GPGAuth token nonce that are not UUIDs.");
        }
    }
}

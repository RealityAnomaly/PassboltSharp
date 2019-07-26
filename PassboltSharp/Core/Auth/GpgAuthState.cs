namespace PassboltSharp.Core.Auth
{
    internal enum GpgAuthState
    {
        Logout,
        DecryptToken, // "Stage 1"
        VerifyToken, // "Stage 2"
        Complete,
    }
}

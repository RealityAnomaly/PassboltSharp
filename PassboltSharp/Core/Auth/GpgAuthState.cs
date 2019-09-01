namespace PassboltSharp.Core.Auth
{
    internal enum GpgAuthState
    {
        Logout,
        VerifyServer, // stage 0
        DecryptToken, // stage 1
        VerifyToken, // stage 2
        Complete,
    }
}

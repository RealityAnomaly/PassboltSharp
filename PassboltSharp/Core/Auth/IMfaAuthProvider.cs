using System.Threading.Tasks;

namespace PassboltSharp.Core.Auth
{
    /// <summary>
    /// Implement this provider in your application.
    /// </summary>
    public interface IMfaAuthProvider
    {
        /// <summary>
        /// Called asynchronously to request an OTP from this provider.
        /// </summary>
        /// <returns>The OTP, after the user has authenticated.</returns>
        Task<string> GetOtpAsync();

        /// <summary>
        /// Displays a MFA error on the user interface. Should not block the thread.
        /// </summary>
        /// <param name="error">The error returned from the server.</param>
        void DisplayError(string error);
    }
}

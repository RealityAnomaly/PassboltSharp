using System;
using System.Threading.Tasks;
using PassboltSharp.Core;
using PassboltSharp.Core.Auth;
using PassboltSharp.Models;

namespace PassboltCli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Running API test");
            var clientKey = GpgKey.FromFile(@"C:\Users\josephmarsden\Documents\Development\Test\passbolt_client_public.asc",
                @"C:\Users\josephmarsden\Documents\Development\Test\passbolt_client_private.asc");
            var serverKey = GpgKey.FromFile(@"C:\Users\josephmarsden\Documents\Development\Test\passbolt_server_public.asc");

            clientKey.Fingerprint = "92BCE51A7AB62E5B60631C6C68397B90B85B546A";
            serverKey.Fingerprint = "A876BCFE2406F19ADB6C81A45C2B1175950963B4";

            var client = new ApiClient(new Uri("https://passbolt.internal.arctarus.co.uk"));
            using (var session = client.GetSession())
            {
                var auth = new GpgAuth(session, clientKey, serverKey);
                GpgAuthSessionState result;

                while (true)
                {
                    Console.Write("Password: ");
                    var password = CliUtilities.ReadPassword();
                    result = await auth.Authenticate(password);

                    if (result == GpgAuthSessionState.Invalid)
                        continue;
                    break;
                }

                switch (result)
                {
                    case GpgAuthSessionState.MfaRequired:
                        var mfa = new MfaAuth(session, MfaAuthProviderType.Totp);
                        var providers = auth.MfaProviders;

                        while (true)
                        {
                            Console.Write("Enter your MFA token: ");
                            var token = Console.ReadLine();
                            if (!await mfa.VerifyMfaChallenge(token))
                            {
                                Console.Write("Invalid token.");
                                continue;
                            }

                            break;
                        }

                        if (await auth.VerifySession() != GpgAuthSessionState.Valid)
                        {
                            Console.WriteLine("MFA error occurred");
                            return;
                        }

                        break;
                    case GpgAuthSessionState.Valid:
                        return;
                }

                Console.WriteLine("Authentication successful.");
            }
        }
    }
}

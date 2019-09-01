using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace PassboltCli
{
    internal static class CliUtilities
    {
        internal static SecureString ReadPassword()
        {
            var password = new SecureString();
            while (true)
            {
                var info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Enter)
                {
                    Console.Write('\n');
                    break;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (password.Length <= 0) continue;

                    password.RemoveAt(password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (info.KeyChar != '\u0000')
                {
                    password.AppendChar(info.KeyChar);
                    Console.Write('*');
                }
            }

            return password;
        }
    }
}

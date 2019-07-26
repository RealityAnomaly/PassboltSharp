using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PassboltSharp.Models;
using PgpCore;

namespace PassboltSharp.Helpers
{
    internal static class PgpHelper
    {
        internal static async Task<string> DecryptStringAsync(this PGP pgp, string data, GpgKey key, string password)
        {
            return await DecryptStringAsync(pgp, data, Encoding.UTF8.GetBytes(key.PrivateKey), password);
        }

        internal static async Task<string> DecryptStringAsync(this PGP pgp, string data, byte[] key, string password)
        {
            using (var ks = new MemoryStream(key))
            using (var ds = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var os = new MemoryStream())
            {
                await pgp.DecryptStreamAsync(ds, os, ks, password);
                return Encoding.UTF8.GetString(os.ToArray());
            }
        }

        internal static async Task<string> EncryptStringAsync(this PGP pgp, string data, GpgKey key)
        {
            return await EncryptStringAsync(pgp, data, Encoding.UTF8.GetBytes(key.ArmoredKey));
        }

        internal static async Task<string> EncryptStringAsync(this PGP pgp, string data, byte[] key, bool armor = true, bool integrityCheck = true)
        {
            using (var ks = new MemoryStream(key))
            using (var ds = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var os = new MemoryStream())
            {
                await pgp.EncryptStreamAsync(ds, os, ks, armor, integrityCheck);
                return Encoding.UTF8.GetString(os.ToArray());
            }
        }
    }
}

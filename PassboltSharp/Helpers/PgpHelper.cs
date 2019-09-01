using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;
using PassboltSharp.Models;
using PgpCore;

namespace PassboltSharp.Helpers
{
    internal static class PgpHelper
    {
        internal static async Task<string> DecryptStringAsync(this PGP pgp, string data, GpgKey key, SecureString password)
        {
            return await DecryptStringAsync(pgp, data, Encoding.UTF8.GetBytes(key.PrivateKey), password);
        }

        internal static async Task<string> DecryptStringAsync(this PGP pgp, string data, byte[] key, SecureString password)
        {
            using (var ks = new MemoryStream(key))
            using (var ds = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var os = new MemoryStream())
            {
                var bStrPtr = Marshal.SecureStringToBSTR(password);
                var decryptedStr = Marshal.PtrToStringBSTR(bStrPtr);
                Marshal.ZeroFreeBSTR(bStrPtr);

                await pgp.DecryptStreamAsync(ds, os, ks, decryptedStr);
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

        internal static string FingerprintFromKeyPacket(byte[] contents)
        {
            var digest = new Sha1Digest();
            digest.Update(0x99);
            digest.Update((byte)(contents.Length >> 8));
            digest.Update((byte)contents.Length);
            digest.BlockUpdate(contents, 0, contents.Length);

            var bytes = new byte[digest.GetDigestSize()];
            digest.DoFinal(bytes, 0);

            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                builder.AppendFormat("{0:x2}", b);
            return builder.ToString().ToUpper();
        }
    }
}

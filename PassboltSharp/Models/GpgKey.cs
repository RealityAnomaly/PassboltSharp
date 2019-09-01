using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Digests;
using PassboltSharp.Helpers;

namespace PassboltSharp.Models
{
    public class GpgKey : BaseResource
    {
        public Guid UserId;
        public string ArmoredKey;
        public string PrivateKey;
        public int Bits;
        public string Uid;
        public string KeyId;
        public string Fingerprint;
        public GpgKeyType Type;
        public DateTime Expires;
        public DateTime KeyCreated;
        public DateTime Deleted;

        public static GpgKey FromFile(string publicKeyPath, string privateKeyPath = null)
        {
            var key = new GpgKey {ArmoredKey = File.ReadAllText(publicKeyPath)};

            // retrieve the fingerprint (why does it have to be so hard?)
            // TODO: find out a way to do this and avoid file path part
            var contents = PgpCore.Utilities.ReadPublicKey(publicKeyPath).PublicKeyPacket.GetEncodedContents();
            key.Fingerprint = PgpHelper.FingerprintFromKeyPacket(contents);

            if (!string.IsNullOrWhiteSpace(privateKeyPath))
                key.PrivateKey = File.ReadAllText(privateKeyPath);

            return key;
        }
    }
}

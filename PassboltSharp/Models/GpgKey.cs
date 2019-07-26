using System;
using System.Collections.Generic;
using System.Text;

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
    }
}

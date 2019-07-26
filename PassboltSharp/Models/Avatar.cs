using System;

namespace PassboltSharp.Models
{
    public class Avatar : BaseResource
    {
        public Guid UserId;
        public string FileName;
        public int FileSize;
        public string MimeType;
        public string Extension;
        public string Hash;
        public string Path;
        public string Adapter;
        public AvatarUrl Url;
    }
}

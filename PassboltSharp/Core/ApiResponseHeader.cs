using System;

namespace PassboltSharp.Core
{
    internal class ApiResponseHeader
    {
        internal Guid Id;
        internal int Code;
        internal string Message;
        internal DateTime ServerTime;
        internal string Status;
        internal string Title;
        internal string Url;
    }
}

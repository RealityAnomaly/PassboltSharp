using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Core.Auth
{
    public enum GpgAuthSessionState
    {
        Invalid,
        MfaRequired,
        Valid,
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    public class Profile : BaseResource
    {
        public Guid UserId;
        public string FirstName;
        public string LastName;
    }
}

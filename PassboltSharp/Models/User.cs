using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    public class User : BaseResource
    {
        public Guid RoleId;
        public string Username;
        public bool Active;
        public bool Deleted;
        public Profile Profile;
        public IEnumerable<User> GroupUsers;
        public Role Role;
        public GpgKey GpgKey;
        public DateTime LastLoggedIn;
    }
}

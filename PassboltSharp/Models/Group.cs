using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    public class Group : BaseResource
    {
        public string Name;
        public DateTime Deleted;
        public Guid CreatedBy;
        public Guid ModifiedBy;
        public IEnumerable<User> GroupsUsers;
    }
}

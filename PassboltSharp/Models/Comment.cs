using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    public class Comment : BaseResource
    {
        public Guid ParentId;
        public string Content;
        public Guid CreatedBy;
        public Guid ModifiedBy;
        public Guid UserId;
        public User Creator;
        public IEnumerable<Comment> Children;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    /// <summary>
    /// Resource objects allow you to perform recurring charges, and to track multiple charges, that are associated with the same customer.
    /// The API allows you to create, delete, and update your customers.
    /// You can retrieve individual customers as well as a list of all your customers.
    /// </summary>
    public class Resource : BaseResource
    {
        public Guid CreatedBy;
        public User Creator;
        public bool Deleted;
        public string Description;
        public Favorite Favorite;
        public Guid ModifiedBy;
        public User Modifier;
        public string Name;
        public Permission Permission;
        public string Uri;
        public string Username;
    }
}

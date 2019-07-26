using System;
using System.Collections.Generic;
using System.Text;

namespace PassboltSharp.Models
{
    public class BaseResource
    {
        public Guid Id;
        public DateTime Created;
        public DateTime Modified;

        internal BaseResource() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentCreation.Api.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public List<User> Users { get; set; }

        public class User
        {
            public string Name { get; set; }

            public string Password { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentCreation.Api.Models
{
    public class Address
    {
        public string SelectedAddress { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Town { get; set; }

        public string Postcode { get; set; }

        public bool IsAutomaticallyFound
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedAddress);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class Address
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string DisplayAdress1 { get; set; }
        public string DisplayAdress2 { get; set; }
        public string Description => $"{Street} {Number}, {City}";
    }
}

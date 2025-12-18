using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarSharing.Core
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; } // e.g., "Tesla"
        public string Model { get; set; } // e.g., "Model 3"
        public int Year { get; set; }
        public decimal PricePerHour { get; set; }
        public string Location { get; set; }

        public string Status { get; set; } = "Available";
    }
}

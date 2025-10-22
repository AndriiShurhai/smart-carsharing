using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarSharing.Core
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign Key to User
        public int CarId { get; set; }  // Foreign Key to Car
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalCost { get; set; }
    }
}

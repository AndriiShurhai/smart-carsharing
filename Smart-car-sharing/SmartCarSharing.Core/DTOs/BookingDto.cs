using System;

namespace SmartCarSharing.Core.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string CarMake { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalCost { get; set; }

        // Повне ім'я авто для відображення
        public string CarName => $"{CarMake} {CarModel}";
    }
}
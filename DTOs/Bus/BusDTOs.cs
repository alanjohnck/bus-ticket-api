namespace BusBookingSystem.API.DTOs.Bus
{
    // GET /api/buses/:busId
    public class BusDetailsDto
    {
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string BusCategory { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public OperatorSummaryDto Operator { get; set; } = new();
        public List<string> Amenities { get; set; } = new();
    }

    public class OperatorSummaryDto
    {
        public Guid OperatorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
    }

    // GET /api/buses/:busId/amenities
    public class BusAmenitiesDto
    {
        public Guid BusId { get; set; }
        public List<string> Amenities { get; set; } = new();
    }

    // GET /api/buses/:busId/seat-layout
    public class SeatLayoutDto
    {
        public Guid LayoutId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public string Deck { get; set; } = string.Empty;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBooked { get; set; }
    }

    public class BusSeatLayoutResponseDto
    {
        public Guid BusId { get; set; }
        public int TotalSeats { get; set; }
        public List<SeatLayoutDto> LowerDeck { get; set; } = new();
        public List<SeatLayoutDto> UpperDeck { get; set; } = new();
    }

    // GET /api/buses/:busId/reviews
    public class BusReviewDto
    {
        public Guid ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BusReviewsResponseDto
    {
        public Guid BusId { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<BusReviewDto> Reviews { get; set; } = new();
    }
}

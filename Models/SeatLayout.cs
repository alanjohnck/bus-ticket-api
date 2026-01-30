using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class SeatLayout
    {
        [Key]
        public Guid LayoutId { get; set; }
        public Guid BusId { get; set; }
        public Bus Bus { get; set; }

        public string SeatNumber { get; set; }
        public SeatType SeatType { get; set; }
        public DeckType Deck { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool IsAvailable { get; set; }
    }
}
namespace BusBookingSystem.API.Models
{
    public enum UserRole { Passenger, Operator, Admin }

    public enum BusType { AC, NonAC }

    public enum BusCategory { Sleeper, Seater, SemiSleeper }

    public enum SeatType { Lower, Upper, Window, Aisle }

    public enum DeckType { Lower, Upper }

    public enum TripStatus { Scheduled, InTransit, Completed, Cancelled }

    public enum BookingStatus { Confirmed, Cancelled, Completed }

    public enum PaymentMethod { Card, UPI, Wallet, NetBanking }

    public enum PaymentStatus { Pending, Completed, Failed, Refunded }

    public enum RefundStatus { Pending, Processed, Rejected }

    public enum NotificationType { Email, SMS, Push }

    public enum DiscountType { Percentage, Flat }
}
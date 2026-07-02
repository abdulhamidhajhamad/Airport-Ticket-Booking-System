using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Domain.Entities;

public class Booking : EntityBase
{
    public Guid PassengerId { get; init; }
    public Guid FlightId { get; init; }
    public FlightClass FlightClass { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public DateTime? CancellationDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public decimal Price { get; set; }
    public BookingStatus Status { get; set; }
    public string PassengerName { get; init; } = string.Empty;
    public string PassengerPassportNumber { get; init; } = string.Empty;
}
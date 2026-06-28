
class Booking : EntityBase
{
    public Guid PassengerId { get; init; }
    public Guid FlightId { get; init; }
    public FlightClass FlightClass { get; init; }
    public DateTime BookingDate { get; init; } = DateTime.UtcNow;
    public DateTime? CancellationDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public decimal Price { get; init; }
    public BookingStatus Status { get; set; } 
}
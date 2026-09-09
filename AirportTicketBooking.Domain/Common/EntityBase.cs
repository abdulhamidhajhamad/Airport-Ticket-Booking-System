namespace AirportTicketBooking.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
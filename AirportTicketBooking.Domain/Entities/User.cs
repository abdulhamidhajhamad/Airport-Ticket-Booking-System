using AirportTicketBooking.Domain.Common;

namespace AirportTicketBooking.Domain.Entities;

public abstract class User : EntityBase
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
}
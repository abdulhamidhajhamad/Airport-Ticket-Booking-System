using AirportTicketBooking.Domain.Common;

namespace AirportTicketBooking.Domain.Entities;

public abstract class User : EntityBase
{
    public string Name { get; init; }=string.Empty;
    public string Email { get; init; }=string.Empty;
    public string Password { get; init; }=string.Empty;
}
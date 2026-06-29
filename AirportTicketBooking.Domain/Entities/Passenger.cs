using AirportTicketBooking.Domain.Common;

namespace AirportTicketBooking.Domain.Entities;

public class Passenger : User
{
    public string PassportNumber { get; init; }
}
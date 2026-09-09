using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Domain.Entities;

public class Flight : EntityBase
{
    [CsvColumn(0)]
    public required string FlightNumber { get; init; }=string.Empty;

    [CsvColumn(1)]
    public required string DepartureAirport { get; init; }=string.Empty;

    [CsvColumn(2)]
    public string ArrivalAirport { get; init; }=string.Empty;

    [CsvColumn(5)]
    [FutureDate]
    public DateTime DepartureDateTime { get; init; }

    public int MaxPassengers { get; init; }

    [CsvColumn(6)]
    [FutureDate]

    public DateTime ArrivalDateTime { get; init; }

    [CsvColumn(3)]
    public string DepartureCountry { get; init; }=string.Empty;

    [CsvColumn(4)]
    public string ArrivalCountry { get; init; }=string.Empty;

    [CsvPriceMapping(FlightClass.Economy, 7)]
    [CsvPriceMapping(FlightClass.Business, 8)]
    [CsvPriceMapping(FlightClass.FirstClass, 9)]
    public required Dictionary<FlightClass, decimal> Prices { get; init; }
}
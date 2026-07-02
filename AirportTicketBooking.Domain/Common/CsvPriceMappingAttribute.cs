using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Domain.Common;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class CsvPriceMappingAttribute : Attribute
{
    public FlightClass Class { get; }
    public int Index { get; }

    public CsvPriceMappingAttribute(FlightClass flightClass, int index)
    {
        Class = flightClass;
        Index = index;
    }
}
namespace AirportTicketBooking.Domain.Common;

[AttributeUsage(AttributeTargets.Property)]
public class CsvColumnAttribute : Attribute
{
    public int Index { get; }
    public CsvColumnAttribute(int index) => Index = index;
}
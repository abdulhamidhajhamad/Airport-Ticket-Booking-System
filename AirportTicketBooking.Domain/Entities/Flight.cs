class Flight : EntityBase
{
    public string FlightNumber { get; init; }
    public string DepartureAirport { get; init; }
    public string ArrivalAirport { get; init; }
    public DateTime DepartureDateTime { get; init; }
    public int MaxPassengers { get; init; }
    public string DepartureCountry { get; init; }
    public string ArrivalCountry { get; init; }    
    public Dictionary<FlightClass,decimal> Prices { get; init; }

}
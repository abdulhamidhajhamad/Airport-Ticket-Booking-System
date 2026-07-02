using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Domain.Interfaces;

namespace AirportTicketBooking.Services;
public class FlightService
{
    private readonly IRepository<Flight> _flightRepository;
    public FlightService(IRepository<Flight> flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public IEnumerable<Flight> Search(
        string? DepartureAirport = null,
        string? ArrivalAirport = null,
        DateTime? DepartureDate = null,
        FlightClass? flightClass = null,
        decimal? maxPrice = null,
        string? DepartureCountry = null,
        string? ArrivalCountry = null
    )
    {
        var flights = _flightRepository.GetAll();
        if (!string.IsNullOrEmpty(DepartureAirport))
        {
            flights = flights.Where(f => f.DepartureAirport.Equals(DepartureAirport, StringComparison.OrdinalIgnoreCase));
        }
        if(!string.IsNullOrEmpty(ArrivalAirport))
        {
            flights = flights.Where(f => f.ArrivalAirport.Equals(ArrivalAirport, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(DepartureCountry))
        {
            flights = flights.Where(f => f.DepartureCountry.Equals(DepartureCountry, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(ArrivalCountry))
        {
            flights = flights.Where(f => f.ArrivalCountry.Equals(ArrivalCountry, StringComparison.OrdinalIgnoreCase));
        }
        if (DepartureDate.HasValue)
        {
            flights = flights.Where(f => f.DepartureDateTime.Date == DepartureDate.Value.Date);
        }
        if (flightClass.HasValue)
        {
            flights = flights.Where(f => f.Prices.ContainsKey(flightClass.Value));
        }
        if (maxPrice.HasValue && flightClass.HasValue)
        {
            flights = flights.Where(f => f.Prices.ContainsKey(flightClass.Value) && f.Prices[flightClass.Value] <= maxPrice.Value);
        }
        else if (maxPrice.HasValue)
        {
            flights = flights.Where(f => f.Prices.Values.Any(p => p <= maxPrice.Value));
        }
        return flights;
    }


}
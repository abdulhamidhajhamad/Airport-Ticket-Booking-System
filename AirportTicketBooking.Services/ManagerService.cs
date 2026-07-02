using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Domain.Interfaces; 
using AirportTicketBooking.Services.Utilities;

namespace AirportTicketBooking.Services;

public class ManagerService
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Flight> _flightRepository; 
    private readonly FlightService _flightService;

    public ManagerService(
        IRepository<Booking> bookingRepository, 
        IRepository<Flight> flightRepository, 
        FlightService flightService)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _flightService = flightService;
    }

    public IEnumerable<Booking> FilterBookings(
        string? DepartureAirport = null,
        string? ArrivalAirport = null,
        DateTime? DepartureDate = null,
        FlightClass? flightClass = null,
        string? passengerPassportNumber = null,
        string? DepartureCountry = null,
        string? ArrivalCountry = null,
        Guid? flightId = null,
        decimal? maxPrice = null
    )
    {
        if (maxPrice < 0)
        {
            throw new ArgumentException("Max price cannot be negative.");
        }

        var matchingFlights = _flightService.Search(DepartureAirport, ArrivalAirport, DepartureDate, null, null, DepartureCountry, ArrivalCountry);
        var matchingFlightIds = matchingFlights.Select(f => f.Id).ToHashSet();
        var bookings = _bookingRepository.GetAll().Where(b => matchingFlightIds.Contains(b.FlightId));

        if (flightClass.HasValue)
        {
            bookings = bookings.Where(b => b.FlightClass == flightClass.Value);
        }
        if (flightId.HasValue)
        {
            bookings = bookings.Where(b => b.FlightId == flightId.Value);
        }
        if (maxPrice.HasValue)
        {
            bookings = bookings.Where(b => b.Price <= maxPrice.Value);
        }
        if (!string.IsNullOrEmpty(passengerPassportNumber))
        {
            bookings = bookings.Where(b => b.PassportNumber.Equals(passengerPassportNumber, StringComparison.OrdinalIgnoreCase));
        }

        return bookings;
    }

    public void ImportFlightsFromCsv(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file {filePath} does not exist.");
        }

        var lines = File.ReadLines(filePath);
        
        foreach (var line in lines.Skip(1)) 
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Split(',');

            if (columns.Length < 10) continue;

            try
            {
                Flight flight = CsvReflectionParser.ParseRow(columns);

                if (flight.DepartureDateTime >= flight.ArrivalDateTime) continue;
                
                if (flight.Prices.Values.Any(p => p < 0)) continue;

                if (string.IsNullOrWhiteSpace(flight.FlightNumber) || 
                    string.IsNullOrWhiteSpace(flight.DepartureAirport) || 
                    string.IsNullOrWhiteSpace(flight.ArrivalAirport))
                {
                    continue;
                }

                flight.Id = Guid.NewGuid();

                _flightRepository.Add(flight);
            }
            catch
            {
                continue; 
            }
        }

        _flightRepository.Save();
    }
}
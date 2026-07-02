namespace AirportTicketBooking.Domain.ManagerServices;

public class ManagerService
{
    private readonly IRepository<Booking> _bookingRepository;
private readonly FlightService _flightService;
    public ManagerService(IRepository<Booking> bookingRepository, FlightService flightService)
    {
        _bookingRepository = bookingRepository;
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
        if(maxPrice<0)
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
        if(maxPrice.HasValue)
        {
            bookings = bookings.Where(b => b.Price <= maxPrice.Value);
        }
        if (!string.IsNullOrEmpty(passengerPassportNumber))
        {
            bookings = bookings.Where(b => b.PassportNumber.Equals(passengerPassportNumber, StringComparison.OrdinalIgnoreCase));
        }
        return bookings;
    }
}
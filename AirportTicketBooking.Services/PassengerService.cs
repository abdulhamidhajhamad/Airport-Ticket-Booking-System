namespace AirportTicketBooking.passengerServices;

public class PassengerService
{
    private readonly BookingService _bookingService;
    private readonly FlightService _flightService;

    public PassengerService(BookingService bookingService, FlightService flightService)
    {
        _bookingService = bookingService;
        _flightService = flightService;
    }
    public IEnumerable<Booking> GetMyBookings(string passengerPassportNumber) => _bookingService.GetAllBookings().Where(p => p.PassportNumber == passengerPassportNumber);
    public bool CancelMyBooking(Guid bookingId)
    {
        return _bookingService.CancelBooking(bookingId);
    }
    public bool ModifyMyBooking(Guid bookingId, FlightClass flightClass)
    {
        return _bookingService.ModifyBooking(bookingId, flightClass);
    }
    public bool CreateMyBooking(Guid flightId, FlightClass flightClass, string passengerName, string passengerPassportNumber)
    {
        return _bookingService.CreateBooking(flightId, flightClass, passengerName, passengerPassportNumber);
    }
    public IEnumerable<Flight> SearchFlights(string? DepartureAirport = null, string? ArrivalAirport = null, DateTime? DepartureDate = null, FlightClass? flightClass = null, decimal? maxPrice = null, string? DepartureCountry = null, string? ArrivalCountry = null)
    {
        return _flightService.Search(DepartureAirport, ArrivalAirport, DepartureDate, flightClass, maxPrice, DepartureCountry, ArrivalCountry);
    }
}
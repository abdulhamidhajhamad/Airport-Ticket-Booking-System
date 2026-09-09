namespace AirportTicketBooking.Domain.BookingServices;

public class BookingService
{
    public readonly IRepository<Booking> _bookingRepository;
    public readonly IRepository<Flight> _flightRepository;
    public BookingService(IRepository<Booking> bookingRepository, IRepository<Flight> flightRepository)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
    }

    public IEnumerable<Booking> GetAllBookings() => _bookingRepository.GetAll();
public bool CancelBooking(Guid bookingId)   
    {
        var booking = _bookingRepository.GetById(bookingId);
        if (booking == null)
        {
            return false;
        }
        else
        {
            _bookingRepository.Delete(bookingId);
            _bookingRepository.Save();
            return true;
        }
    }

    public bool ModifyBooking(Guid bookingId, FlightClass flightClass)
    {
        var booking =_bookingRepository.GetById(bookingId);
        if (booking == null)
        {
            return false;
        }
        else
        {
            var flight = _flightRepository.GetById(booking.FlightId);
            if (flight == null|| !flight.Prices.ContainsKey(flightClass))
            {
                return false;
            }
            else
            {
                booking.FlightClass = flightClass;
                booking.Price = flight.Prices[flightClass];
                booking.Status = BookingStatus.Modified;
                _bookingRepository.Update(booking);
                _bookingRepository.Save();
                return true;
            }
        }
    }

    public bool CreateBooking(Guid flightId, FlightClass flightClass, string passengerName, string passengerPassportNumber)
    {
        var flight = _flightRepository.GetById(flightId);
        if (flight == null || !flight.Prices.ContainsKey(flightClass)||flight.DepartureDateTime < DateTime.UtcNow)
        {
            return false;
        }
        

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            FlightId = flightId,
            FlightClass = flightClass,
            Price = flight.Prices[flightClass],
            PassengerName = passengerName,
            PassengerPassportNumber = passengerPassportNumber,
            Status = BookingStatus.Confirmed
        };

        _bookingRepository.Add(booking);
        _bookingRepository.Save();
        return true;
    }

}
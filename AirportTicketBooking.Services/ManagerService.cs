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

        private bool TryParseFlight(string [] columns,out Flight? flight)
    {
       flight=null;
       
       if (columns.Length < 10)
       {
            return false;
       }

       if (!DateTime.TryParse(columns[5], out DateTime departureDateTime) || !DateTime.TryParse(columns[6], out DateTime arrivalDateTime))
       {
            return false;
       }
       if (!decimal.TryParse(columns[7], out decimal economyPrice) || !decimal.TryParse(columns[8], out decimal businessPrice) || !decimal.TryParse(columns[9], out decimal firstClassPrice))
       {
            return false;
       }
       if (economyPrice < 0 || businessPrice < 0 || firstClassPrice < 0)
       {
            return false;
       }
       if (departureDateTime >= arrivalDateTime)
       {
            return false;
       }
       if (string.IsNullOrWhiteSpace(columns[0]) || string.IsNullOrWhiteSpace(columns[1]) || string.IsNullOrWhiteSpace(columns[2]) || string.IsNullOrWhiteSpace(columns[3]) || string.IsNullOrWhiteSpace(columns[4]))
       {
            return false;
       } 
              flight = new Flight
       {
            Id = Guid.NewGuid(),
            FlightNumber = columns[0],
            DepartureAirport = columns[1],
            ArrivalAirport = columns[2],
            DepartureCountry = columns[3],
            ArrivalCountry = columns[4],
                   DepartureDateTime =departureDateTime,
                ArrivalDateTime = arrivalDateTime,
                Prices = new Dictionary<FlightClass, decimal>
                {
                    { FlightClass.Economy, economyPrice },
                    { FlightClass.Business, businessPrice },
                    { FlightClass.FirstClass, firstClassPrice }
                }
       };
       return true;
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
            var columns = line.Split(',');
            if (!TryParseFlight(columns, out Flight? flight))
            {
                continue;
            }
            _flightRepository.Add(flight);
        }
        _flightRepository.Save();
    }

}
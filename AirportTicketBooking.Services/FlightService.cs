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
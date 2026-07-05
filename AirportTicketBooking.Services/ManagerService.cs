using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Domain.Interfaces;
using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Services.Utilities;
using System.Reflection;
using AirportTicketBooking.Services.Enums;

namespace AirportTicketBooking.Services;

public record CsvRowError(int LineNumber, string ErrorMessage);
public record CsvImportResult(int SuccessfulCount, List<CsvRowError> Errors);

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

        if (flightClass.HasValue) bookings = bookings.Where(b => b.FlightClass == flightClass.Value);
        if (flightId.HasValue) bookings = bookings.Where(b => b.FlightId == flightId.Value);
        if (maxPrice.HasValue) bookings = bookings.Where(b => b.Price <= maxPrice.Value);
        
        if (!string.IsNullOrEmpty(passengerPassportNumber))
        {
            bookings = bookings.Where(b => b.PassengerPassportNumber.Equals(passengerPassportNumber, StringComparison.OrdinalIgnoreCase));
        }

        return bookings;
    }

    public CsvImportResult ImportFlightsFromCsv(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file {filePath} does not exist.");
        }

        var errors = new List<CsvRowError>();
        int successfulCount = 0;
        int lineNumber = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            if (lineNumber == 1) continue;

            if (string.IsNullOrWhiteSpace(line))
            {
                errors.Add(new CsvRowError(lineNumber, "السطر فارغ ولا يحتوي على بيانات."));
                continue;
            }

            var columns = line.Split(',');

            if (columns.Length < 10)
            {
                errors.Add(new CsvRowError(lineNumber, $"عدد الأعمدة غير مكتمل. متوقع 10 أعمدة ولكن وجد {columns.Length}."));
                continue;
            }

            try
            {
                Flight flight = CsvReflectionParser.ParseRow(columns);
                var rowValidationErrors = new List<string>();

                if (flight.DepartureDateTime >= flight.ArrivalDateTime)
                {
                    rowValidationErrors.Add("تاريخ المغادرة لا يمكن أن يكون بعد أو مساوياً لتاريخ الوصول.");
                }
                if (flight.DepartureDateTime < DateTime.Now)
                {
                    rowValidationErrors.Add("تاريخ المغادرة قديم! يجب أن يكون تاريخ الرحلة في المستقبل.");
                }
                if (flight.ArrivalDateTime < DateTime.Now)
                {
                    rowValidationErrors.Add("تاريخ الوصول قديم! يجب أن يكون تاريخ الوصول في المستقبل.");
                }
                

                if (flight.Prices.Values.Any(p => p < 0))
                {
                    rowValidationErrors.Add("سعر الرحلة لا يمكن أن يكون قيمة سالبة.");
                }

                if (string.IsNullOrWhiteSpace(flight.FlightNumber)) rowValidationErrors.Add("رقم الرحلة حقل مطلوب.");
                if (string.IsNullOrWhiteSpace(flight.DepartureAirport)) rowValidationErrors.Add("مطار المغادرة حقل مطلوب.");
                if (string.IsNullOrWhiteSpace(flight.ArrivalAirport)) rowValidationErrors.Add("مطار الوصول حقل مطلوب.");

                if (rowValidationErrors.Any())
                {
                    string combinedErrors = string.Join(" | ", rowValidationErrors);
                    errors.Add(new CsvRowError(lineNumber, combinedErrors));
                    continue;
                }

                flight.Id = Guid.NewGuid();
                _flightRepository.Add(flight);
                successfulCount++;
            }
            catch (Exception ex)
            {
                errors.Add(new CsvRowError(lineNumber, $"خطأ في صياغة البيانات ونوعها: {ex.Message}"));
            }
        }

        if (successfulCount > 0)
        {
            _flightRepository.Save();
        }

        return new CsvImportResult(successfulCount, errors);
    }

    public void DisplayFlightValidationConstraints()
    {
        var validationInfoList = CsvReflectionParser.GetValidationInfo();

        Console.WriteLine("\n=== Dynamic Model Validation Details ===");

        foreach (var info in validationInfoList)
        {
            Console.WriteLine($"\n* {info.Property.Name} *");

            string typeLabel = info.FieldType switch
            {
                DisplayFieldType.FreeText => "Free Text",
                DisplayFieldType.DateTime => "Date Time",
                DisplayFieldType.Integer => "Integer",
                DisplayFieldType.PriceDictionary => "Dictionary (FlightClass -> Decimal)",
                _ => "Unknown"
            };
            Console.WriteLine($"   Type: {typeLabel}");

            var constraints = new List<string>();
            constraints.Add(info.IsRequired ? "Required" : "Optional");

            if (info.HasFutureDateConstraint)
                constraints.Add("Allowed Range (Today -> Future)");

            if (info.IsNonNegative)
                constraints.Add("Must be non-negative (>= 0)");

            Console.WriteLine($"   Constraint: {string.Join(", ", constraints)}");
        }
        Console.WriteLine("\n=========================================");
    }
}
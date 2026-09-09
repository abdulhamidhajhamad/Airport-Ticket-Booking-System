using System;
using AirportTicketBooking.Data;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Domain.Interfaces;
using AirportTicketBooking.Services;

namespace AirportTicketBooking.ConsoleApp;

class Program
{
    private static ManagerService _managerService = null!;
    private static PassengerService _passengerService = null!;

    static void Main(string[] args)
    {
        InitializeServices();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=============================================");
            Console.WriteLine("    Welcome to Airport Ticket Booking System ");
            Console.WriteLine("=============================================");
            Console.WriteLine("1. Login as Passenger");
            Console.WriteLine("2. Login as Manager");
            Console.WriteLine("3. Exit");
            Console.Write("\nSelect an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    PassengerMenu();
                    break;
                case "2":
                    ManagerMenu();
                    break;
                case "3":
                    Console.WriteLine("\nThank you for using our system. Goodbye!");
                    return;
                default:
                    ShowMessage("Invalid choice. Try again.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private static void InitializeServices()
    {
        IRepository<Flight> flightRepo = new JsonRepository<Flight>(@"..\AirportTicketBooking.Data\Flights.json");
        IRepository<Booking> bookingRepo = new JsonRepository<Booking>(@"..\AirportTicketBooking.Data\Bookings.json");

        FlightService flightService = new FlightService(flightRepo);
        BookingService bookingService = new BookingService(bookingRepo, flightRepo);

        _managerService = new ManagerService(bookingRepo, flightRepo, flightService);
        _passengerService = new PassengerService(bookingService, flightService);
    }

    private static void PassengerMenu()
    {
        Console.Write("\nEnter your Passport Number to continue: ");
        string? passport = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(passport))
        {
            ShowMessage("Passport number cannot be empty.", ConsoleColor.Red);
            return;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Passenger Menu [Passport: {passport}] ===");
            Console.WriteLine("1. Search Available Flights");
            Console.WriteLine("2. Book a Flight");
            Console.WriteLine("3. View My Bookings");
            Console.WriteLine("4. Modify a Booking");
            Console.WriteLine("5. Cancel a Booking");
            Console.WriteLine("6. Logout");
            Console.Write("\nSelect an option: ");

            string? choice = Console.ReadLine();
            if (choice == "6") break;

            switch (choice)
            {
                case "1":
                    var flights = _passengerService.SearchFlights();
                    PrintFlights(flights);
                    break;

                case "2":
                    Console.Write("Enter Flight Guid Id: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid fId))
                    {
                        Console.Write("Choose Class (0 = Economy, 1 = Business, 2 = FirstClass): ");
                        if (Enum.TryParse(Console.ReadLine(), out FlightClass fClass))
                        {
                            Console.Write("Enter Passenger Name: ");
                            string name = Console.ReadLine() ?? "Passenger";
                            
                            bool success = _passengerService.CreateMyBooking(fId, fClass, name, passport);
                            if (success) ShowMessage("Booking created successfully!", ConsoleColor.Green);
                            else ShowMessage("Failed to create booking. Verify Flight Id or Date.", ConsoleColor.Red);
                        }
                    }
                    else ShowMessage("Invalid Format for Guid.", ConsoleColor.Red);
                    break;

                case "3":
                    var myBookings = _passengerService.GetMyBookings(passport);
                    PrintBookings(myBookings);
                    break;

                case "4":
                    Console.Write("Enter Booking Guid Id to Modify: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid bModId))
                    {
                        Console.Write("Choose New Class (0 = Economy, 1 = Business, 2 = FirstClass): ");
                        if (Enum.TryParse(Console.ReadLine(), out FlightClass newClass))
                        {
                            bool success = _passengerService.ModifyMyBooking(bModId, newClass);
                            if (success) ShowMessage("Booking modified successfully!", ConsoleColor.Green);
                            else ShowMessage("Failed to modify booking.", ConsoleColor.Red);
                        }
                    }
                    break;

                case "5":
                    Console.Write("Enter Booking Guid Id to Cancel: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid bCanId))
                    {
                        bool success = _passengerService.CancelMyBooking(bCanId);
                        if (success) ShowMessage("Booking cancelled successfully!", ConsoleColor.Green);
                        else ShowMessage("Failed to cancel booking.", ConsoleColor.Red);
                    }
                    break;

                default:
                    ShowMessage("Invalid option.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private static void ManagerMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Manager Menu ===");
            Console.WriteLine("1. View and Filter All Bookings");
            Console.WriteLine("2. Batch Flight Upload (CSV)");
            Console.WriteLine("3. Display Dynamic Model Validation Constraints");
            Console.WriteLine("4. Logout");
            Console.Write("\nSelect an option: ");

            string? choice = Console.ReadLine();
            if (choice == "4") break;

            switch (choice)
            {
                case "1":
                    var bookings = _managerService.FilterBookings();
                    PrintBookings(bookings);
                    break;

                case "2":
                    Console.Write("Enter full path to CSV file: ");
                    string? path = Console.ReadLine();
                    if (!string.IsNullOrEmpty(path))
                    {
                        try
                        {
                            var result = _managerService.ImportFlightsFromCsv(path);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n[+] Successfully imported {result.SuccessfulCount} flights.");
                            Console.ResetColor();

                            if (result.Errors.Any())
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[!] Encountered {result.Errors.Count} error(s) during import:");
                                foreach (var err in result.Errors)
                                {
                                    Console.WriteLine($"   - Line [{err.LineNumber}]: {err.ErrorMessage}");
                                }
                                Console.ResetColor();
                            }
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                        }
                        catch (Exception ex)
                        {
                            ShowMessage($"Error: {ex.Message}", ConsoleColor.Red);
                        }
                    }
                    break;

                case "3":
                    _managerService.DisplayFlightValidationConstraints();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                default:
                    ShowMessage("Invalid option.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private static void PrintFlights(IEnumerable<Flight> flights)
    {
        Console.WriteLine("\n--- Available Flights ---");
        foreach (var f in flights)
        {
            Console.WriteLine($"Id: {f.Id} | Number: {f.FlightNumber} | From: {f.DepartureAirport} ({f.DepartureCountry}) -> To: {f.ArrivalAirport} ({f.ArrivalCountry}) | Date: {f.DepartureDateTime}");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void PrintBookings(IEnumerable<Booking> bookings)
    {
        Console.WriteLine("\n--- Bookings ---");
        foreach (var b in bookings)
        {
            Console.WriteLine($"Id: {b.Id} | FlightId: {b.FlightId} | Class: {b.FlightClass} | Status: {b.Status} | Passenger: {b.PassengerName} | Passport: {b.PassengerPassportNumber} | Price: {b.Price:C}");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"\n{message}");
        Console.ResetColor();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
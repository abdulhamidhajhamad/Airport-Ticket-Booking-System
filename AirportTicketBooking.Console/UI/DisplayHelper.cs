using System;
using AirportTicketBooking.Domain.Entities;

namespace AirportTicketBooking.ConsoleApp.UI;

public static class DisplayHelper
{
    public static void PrintFlights(IEnumerable<Flight> flights)
    {
        Console.WriteLine("\n--- Available Flights ---");
        foreach (var f in flights)
        {
            Console.WriteLine($"Id: {f.Id} | Number: {f.FlightNumber} | From: {f.DepartureAirport} ({f.DepartureCountry}) -> To: {f.ArrivalAirport} ({f.ArrivalCountry}) | Date: {f.DepartureDateTime}");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public static void PrintBookings(IEnumerable<Booking> bookings)
    {
        Console.WriteLine("\n--- Bookings ---");
        foreach (var b in bookings)
        {
            Console.WriteLine($"Id: {b.Id} | FlightId: {b.FlightId} | Class: {b.FlightClass} | Status: {b.Status} | Passenger: {b.PassengerName} | Passport: {b.PassengerPassportNumber} | Price: {b.Price:C}");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public static void ShowMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"\n{message}");
        Console.ResetColor();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}

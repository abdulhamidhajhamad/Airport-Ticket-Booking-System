using System;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Services;

namespace AirportTicketBooking.ConsoleApp.UI;

public class PassengerMenuUI
{
    private readonly PassengerService _passengerService;

    public PassengerMenuUI(PassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    public void Show()
    {
        Console.Write("\nEnter your Passport Number to continue: ");
        string? passport = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(passport))
        {
            DisplayHelper.ShowMessage("Passport number cannot be empty.", ConsoleColor.Red);
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
                    DisplayHelper.PrintFlights(flights);
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
                            if (success) DisplayHelper.ShowMessage("Booking created successfully!", ConsoleColor.Green);
                            else DisplayHelper.ShowMessage("Failed to create booking. Verify Flight Id or Date.", ConsoleColor.Red);
                        }
                    }
                    else DisplayHelper.ShowMessage("Invalid Format for Guid.", ConsoleColor.Red);
                    break;

                case "3":
                    var myBookings = _passengerService.GetMyBookings(passport);
                    DisplayHelper.PrintBookings(myBookings);
                    break;

                case "4":
                    Console.Write("Enter Booking Guid Id to Modify: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid bModId))
                    {
                        Console.Write("Choose New Class (0 = Economy, 1 = Business, 2 = FirstClass): ");
                        if (Enum.TryParse(Console.ReadLine(), out FlightClass newClass))
                        {
                            bool success = _passengerService.ModifyMyBooking(bModId, newClass);
                            if (success) DisplayHelper.ShowMessage("Booking modified successfully!", ConsoleColor.Green);
                            else DisplayHelper.ShowMessage("Failed to modify booking.", ConsoleColor.Red);
                        }
                    }
                    break;

                case "5":
                    Console.Write("Enter Booking Guid Id to Cancel: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid bCanId))
                    {
                        bool success = _passengerService.CancelMyBooking(bCanId);
                        if (success) DisplayHelper.ShowMessage("Booking cancelled successfully!", ConsoleColor.Green);
                        else DisplayHelper.ShowMessage("Failed to cancel booking.", ConsoleColor.Red);
                    }
                    break;

                default:
                    DisplayHelper.ShowMessage("Invalid option.", ConsoleColor.Red);
                    break;
            }
        }
    }
}

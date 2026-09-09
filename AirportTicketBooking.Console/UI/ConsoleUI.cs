using System;
using AirportTicketBooking.Data;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Interfaces;
using AirportTicketBooking.Services;

namespace AirportTicketBooking.ConsoleApp.UI;

public class ConsoleUI
{
    private ManagerService _managerService = null!;
    private PassengerService _passengerService = null!;

    public ConsoleUI()
    {
        InitializeServices();
    }

    private void InitializeServices()
    {
        IRepository<Flight> flightRepo = new JsonRepository<Flight>(@"..\AirportTicketBooking.Data\Flights.json");
        IRepository<Booking> bookingRepo = new JsonRepository<Booking>(@"..\AirportTicketBooking.Data\Bookings.json");

        FlightService flightService = new FlightService(flightRepo);
        BookingService bookingService = new BookingService(bookingRepo, flightRepo);

        _managerService = new ManagerService(bookingRepo, flightRepo, flightService);
        _passengerService = new PassengerService(bookingService, flightService);
    }

    public void Run()
    {
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
                    new PassengerMenuUI(_passengerService).Show();
                    break;
                case "2":
                    new ManagerMenuUI(_managerService).Show();
                    break;
                case "3":
                    Console.WriteLine("\nThank you for using our system. Goodbye!");
                    return;
                default:
                    DisplayHelper.ShowMessage("Invalid choice. Try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
}

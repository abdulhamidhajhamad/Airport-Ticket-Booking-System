using System;
using System.Linq;
using AirportTicketBooking.Services;

namespace AirportTicketBooking.ConsoleApp.UI;

public class ManagerMenuUI
{
    private readonly ManagerService _managerService;

    public ManagerMenuUI(ManagerService managerService)
    {
        _managerService = managerService;
    }

    public void Show()
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
                    DisplayHelper.PrintBookings(bookings);
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
                            DisplayHelper.ShowMessage($"Error: {ex.Message}", ConsoleColor.Red);
                        }
                    }
                    break;

                case "3":
                    _managerService.DisplayFlightValidationConstraints();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                default:
                    DisplayHelper.ShowMessage("Invalid option.", ConsoleColor.Red);
                    break;
            }
        }
    }
}

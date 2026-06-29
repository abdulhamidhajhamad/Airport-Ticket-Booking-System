using AirportTicketBooking.Data;
using AirportTicketBooking.Domain.Entities;

string filePath = "passengers.json";
var repo = new JsonRepository<Passenger>(filePath);

Console.WriteLine("--- 1. فحص دالة الإضافة (Add) ---");
var passenger1 = new Passenger { Name = "Ahmad", PassportNumber = "A1234567" };
var passenger2 = new Passenger { Name = "Sara", PassportNumber = "B9876543" };

repo.Add(passenger1);
repo.Add(passenger2);

repo.Save();
Console.WriteLine("تمت إضافة المسافرين وحفظ الملف بنجاح!");

Console.WriteLine("\n--- 2. فحص دالة جلب الكل (GetAll) ---");
var allPassengers = repo.GetAll();
foreach (var p in allPassengers)
{
    Console.WriteLine($"ID: {p.Id} | Name: {p.Name}");
}

Console.WriteLine("\n--- 3. فحص دالة التعديل (Update) ---");
var updatedPassenger = new Passenger
{
    Id = passenger1.Id,
    Name = "Ahmad Al-Ali",
    PassportNumber = passenger1.PassportNumber
};
repo.Update(updatedPassenger);
repo.Save();
Console.WriteLine($"تم تعديل الاسم إلى: {repo.GetById(passenger1.Id)?.Name}");

Console.WriteLine("\n--- 4. فحص دالة الحذف (Delete) ---");
repo.Delete(passenger2.Id);
repo.Save();
Console.WriteLine($"عدد المسافرين بعد الحذف: {repo.GetAll().Count()}");
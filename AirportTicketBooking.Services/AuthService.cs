namespace AirportTicketBooking.Services;
using  AirportTicketBooking.Domain.Entities;
using  AirportTicketBooking.Domain.Interfaces;
public class AuthService
{
    private readonly IRepository<Manager> _managerRepository;
    private readonly IRepository<Passenger> _passengerRepository;
    
    public AuthService(IRepository<Manager> managerRepository, IRepository<Passenger> passengerRepository)
    {
        _managerRepository = managerRepository;
        _passengerRepository = passengerRepository;
    }
    public User? Authenticate(string email,string password)
    {
        var manager = _managerRepository.GetAll().FirstOrDefault(m => m.Email == email && PasswordHasher.VerifyPassword(password, m.Password));
        if (manager != null)
        {
            return manager;
        }

        var passenger = _passengerRepository.GetAll().FirstOrDefault(p => p.Email == email && PasswordHasher.VerifyPassword(password, p.Password));
        if (passenger != null)
        {
            return passenger;
        }
        return null;
    }
}
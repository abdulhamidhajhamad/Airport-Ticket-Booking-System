using AirportTicketBooking.Domain.Common;

namespace AirportTicketBooking.Domain.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    IEnumerable<T> GetAll();
    T? GetById(Guid id);
    void Add(T entity);
    void Update(T entity);
    void Delete(Guid id);
    void Save();
}
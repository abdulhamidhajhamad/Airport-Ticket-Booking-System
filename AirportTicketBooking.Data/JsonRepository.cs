using System.Text.Json;
using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Interfaces;

namespace AirportTicketBooking.Data;

public class JsonRepository<T> : IRepository<T> where T : EntityBase
{
    private readonly string _filePath;
    private bool _isDirty;
    private readonly List<T> _cache;

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
        _isDirty = false;
        _cache = LoadFromFile();
    }

    private List<T> LoadFromFile()
    {
        if (!File.Exists(_filePath))
        {
            return new List<T>();
        }

        string json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public IEnumerable<T> GetAll() => _cache;

    public T? GetById(Guid id) => _cache.FirstOrDefault(e => e.Id == id);

    public void Add(T entity)
    {
        _cache.Add(entity);
        _isDirty = true;
    }

    public void Update(T entity)
    {
        var existing = GetById(entity.Id);
        if (existing != null)
        {
            _cache.Remove(existing);
            _cache.Add(entity);
            _isDirty = true;
        }
    }

    public void Delete(Guid id)
    {
        var existing = GetById(id);
        if (existing != null)
        {
            _cache.Remove(existing);
            _isDirty = true;
        }
    }

    public void Save()
    {
        if (!_isDirty) return;

        var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
        _isDirty = false;
    }
}

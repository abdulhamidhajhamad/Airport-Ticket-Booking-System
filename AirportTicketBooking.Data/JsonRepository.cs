using System.Text.Json;
using System.Text.Json.Serialization;
using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Interfaces;

namespace AirportTicketBooking.Data;

public class JsonRepository<T> : IRepository<T> where T : EntityBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

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

        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
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
        if (_cache.RemoveAll(e => e.Id == entity.Id) > 0)
        {
            _cache.Add(entity);
            _isDirty = true;
        }
    }

    public void Delete(Guid id)
    {
        if (_cache.RemoveAll(e => e.Id == id) > 0)
        {
            _isDirty = true;
        }
    }

    public void Save()
    {
        if (!_isDirty) return;

        var json = JsonSerializer.Serialize(_cache, _jsonOptions);
        File.WriteAllText(_filePath, json);
        _isDirty = false;
    }
}

class JsonRepository<T> : IRepository<T> where T : EntityBase
{
    private readonly string _FilePath { get; init; }
    private  bool _IsChanged { get; set; }
    private readonly List<T> _cache { get; }


    public JsonRepository(string filePath)
    {
        _FilePath = filePath;
        _IsChanged = false;
        _cache =LoadFromFile();
    }

    private List<T> LoadFromFile()
    {
        if (!File.Exists(_FilePath))
        {
            return new List<T>();
        }
        String json = File.ReadAllText(_FilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();   
        }
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public IEnumerable<T> GetAll()=> _cache;
    public T? GetById(Guid id) => _cache.FirstOrDefault(e => e.Id == id);
    public void Add(T entity)
    {
        _cache.Add(entity);
        _IsChanged = true;
    }
    public void Update(T entity)
    {
        var existingEntity = GetById(entity.Id);
        if (existingEntity != null)
        {
            _cache.Remove(existingEntity);
            _cache.Add(entity);
            _IsChanged = true;
        }
    }
    public void Delete(Guid id)
    {
        var existingEntity = GetById(id);
        if (existingEntity != null)
        {
            _cache.Remove(existingEntity);
            _IsChanged = true;
        }
    }
    public void Save()
    {
        if (_IsChanged)
        {
            var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_FilePath, json);
            _IsChanged = false;
        }
    }
}
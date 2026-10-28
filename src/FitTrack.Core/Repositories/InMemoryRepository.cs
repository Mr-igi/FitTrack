using FitTrack.Core.Models;

namespace FitTrack.Core.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();

    private int _nextId = 1;
    public T? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public IEnumerable<T> GetAll() => _items;

    public IEnumerable<T> Find(Func<T, bool> predicate) => _items.Where(predicate);

    public bool Any(Func<T, bool> predicate) => _items.Any(predicate);

    public T Add(T entity)
    {
        entity.Id = _nextId++;
        _items.Add(entity);

        return entity;
    }

    public void Remove(T entity) => _items.Remove(entity);
}
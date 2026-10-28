using FitTrack.Core.Models;

namespace FitTrack.Core.Repositories;

public interface IRepository<T> where T : class, IEntity
{
    T? GetById(int id);
    IEnumerable<T> GetAll();

    IEnumerable<T> Find(Func<T, bool> predicate);

    bool Any(Func<T, bool> predicate);

    T Add(T entity);

    void Remove(T entity);
}
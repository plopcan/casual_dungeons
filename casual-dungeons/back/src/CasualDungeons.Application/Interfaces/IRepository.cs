namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Operaciones CRUD básicas comunes a todos los repositorios.
/// Las consultas específicas de cada entidad se definen en sus
/// interfaces dedicadas (ej. IPersonajeRepository).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

using CasualDungeons.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación base de IRepository&lt;T&gt; sobre EF Core. Los
/// repositorios concretos heredan de esta clase y añaden sus
/// consultas específicas (con Include, filtros, etc.).
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;

    public EfRepository(AppDbContext context)
    {
        Context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        Context.Set<T>().Update(entity);
    }

    public virtual void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }
}

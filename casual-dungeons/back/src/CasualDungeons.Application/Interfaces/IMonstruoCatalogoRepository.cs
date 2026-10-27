using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface IMonstruoCatalogoRepository : IRepository<MonstruoCatalogo>
{
    /// <summary>
    /// Ficha completa de un monstruo del catálogo, con sus rasgos y
    /// acciones (colección Detalles), para mostrar el stat block
    /// completo antes de añadirlo a la campaña.
    /// </summary>
    Task<MonstruoCatalogo?> GetConDetallesAsync(int id, CancellationToken cancellationToken = default);
}

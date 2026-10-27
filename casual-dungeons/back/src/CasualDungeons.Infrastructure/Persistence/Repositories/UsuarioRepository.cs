using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : EfRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }

    // Se sobrescriben los métodos base para incluir siempre el Rol:
    // los servicios de Application acceden a usuario.Rol.Nombre al
    // mapear a DTO, por lo que cargarlo es obligatorio aquí.
    public override async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public override async Task<List<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
    }

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Usuario?> GetByIdWithConfiguracionAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Configuracion)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Usuarios.Where(u => u.NombreUsuario == nombreUsuario);
        if (excluirId.HasValue)
            query = query.Where(u => u.Id != excluirId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(string email, int? excluirId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Usuarios.Where(u => u.Email == email);
        if (excluirId.HasValue)
            query = query.Where(u => u.Id != excluirId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<Usuario>> GetByRolNombreAsync(string nombreRol, CancellationToken cancellationToken = default)
    {
        return await Context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.Rol.Nombre == nombreRol && u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync(cancellationToken);
    }
}

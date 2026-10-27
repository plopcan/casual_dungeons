using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using CasualDungeons.Infrastructure.Persistence.Repositories;

namespace CasualDungeons.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Usuarios = new UsuarioRepository(_context);
        Campanias = new CampaniaRepository(_context);
        Personajes = new PersonajeRepository(_context);
        NPCs = new NPCRepository(_context);
        MonstruosCampania = new MonstruoCampaniaRepository(_context);
        Combates = new CombateRepository(_context);
        Roles = new RolRepository(_context);
        ConfiguracionesUsuario = new EfRepository<ConfiguracionUsuario>(_context);
        Hechizos = new EfRepository<Hechizo>(_context);
        MonstruosCatalogo = new MonstruoCatalogoRepository(_context);
    }

    public IUsuarioRepository Usuarios { get; }
    public ICampaniaRepository Campanias { get; }
    public IPersonajeRepository Personajes { get; }
    public INPCRepository NPCs { get; }
    public IMonstruoCampaniaRepository MonstruosCampania { get; }
    public ICombateRepository Combates { get; }
    public IRolRepository Roles { get; }
    public IRepository<ConfiguracionUsuario> ConfiguracionesUsuario { get; }
    public IRepository<Hechizo> Hechizos { get; }
    public IMonstruoCatalogoRepository MonstruosCatalogo { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

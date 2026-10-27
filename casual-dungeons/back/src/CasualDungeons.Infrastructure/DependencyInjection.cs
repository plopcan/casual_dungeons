using CasualDungeons.Application.Interfaces;
using CasualDungeons.Application.Services;
using CasualDungeons.Infrastructure.Identity;
using CasualDungeons.Infrastructure.Persistence;
using CasualDungeons.Infrastructure.Realtime;
using CasualDungeons.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CasualDungeons.Infrastructure;

/// <summary>
/// Punto único de registro de dependencias para Infrastructure y
/// Application. Mantiene Program.cs (capa API) limpio: solo necesita
/// llamar a builder.Services.AddInfrastructure(builder.Configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en la configuración.");

        // ServerVersion.AutoDetect abre una conexión durante el arranque
        // para detectar la versión real del servidor MySQL/MariaDB y
        // generar SQL óptimo para esa versión. Si prefieres evitar esa
        // conexión adicional en el arranque, sustituye por una versión
        // fija conocida, ej.: new MySqlServerVersion(new Version(8, 0, 36)).
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mysql =>
                mysql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Persistencia
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Identidad / seguridad
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Tiempo real
        services.AddScoped<ICampaignNotifier, CampaignNotifier>();
        services.AddSignalR();

        // Almacenamiento de archivos subidos (imágenes, etc.)
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Servicios de Application (se registran aquí para mantener
        // un único punto de configuración del contenedor de DI).
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ICampaniaService, CampaniaService>();
        services.AddScoped<IPersonajeService, PersonajeService>();
        services.AddScoped<INPCService, NPCService>();
        services.AddScoped<IMonstruoCampaniaService, MonstruoCampaniaService>();
        services.AddScoped<ICombateService, CombateService>();
        services.AddScoped<IHechizoService, HechizoService>();
        services.AddScoped<IMonstruoCatalogoService, MonstruoCatalogoService>();

        return services;
    }
}

using System.Text;
using CasualDungeons.API.Configuration;
using CasualDungeons.Infrastructure;
using CasualDungeons.Infrastructure.Identity;
using CasualDungeons.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Límite de tamaño de subida de archivos (Kestrel exige esto además
// del [RequestSizeLimit] del controlador; sin esto, Kestrel rechaza
// con un error genérico antes de que el controlador pueda dar un
// mensaje claro sobre por qué falló).
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = SubidaArchivosOpciones.TamanoMaximoBytes;
});

// ---------------------------------------------------------------
// Validación temprana de configuración crítica: evita arrancar la
// API con el secreto JWT de ejemplo del appsettings.json.
// ---------------------------------------------------------------
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.StartsWith("CAMBIAR_"))
{
    throw new InvalidOperationException(
        "La configuración 'Jwt:Secret' no ha sido establecida. " +
        "Defina un valor seguro (mínimo 32 caracteres) en appsettings.json, " +
        "variables de entorno o user-secrets antes de iniciar la API.");
}

// ---------------------------------------------------------------
// Servicios
// ---------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa los enums como string en las respuestas JSON
        // (ej. Estado: "Activa" en vez de 0), más legible para el
        // frontend Angular.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Casual Dungeons API",
        Version = "v1",
        Description = "API REST para gestión de campañas de Dungeons & Dragons 5e."
    });

    // Soporte de JWT en la UI de Swagger ("Authorize" con Bearer token).
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresar el token JWT con el prefijo 'Bearer '. Ej: Bearer eyJhbGciOi..."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS: el frontend Angular corre en un origen distinto al de la
// API. Se restringe a los orígenes configurados explícitamente y
// se permiten credenciales, requeridas por SignalR.
var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Autenticación JWT.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    // SignalR no puede enviar el token en una cabecera Authorization
    // estándar desde el navegador en todos los transportes; se
    // acepta también vía query string "access_token" exclusivamente
    // para las rutas del hub.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/campaign"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ---------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sirve wwwroot/uploads/... como archivos estáticos. Debe ir antes
// de CORS/autenticación: las imágenes se cargan directamente desde
// etiquetas <img> del navegador, que no envían el header
// Authorization ni participan en el preflight CORS de la SPA.
app.UseStaticFiles();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CampaignHub>("/hubs/campaign");

app.Run();

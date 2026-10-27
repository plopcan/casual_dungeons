# Casual Dungeons — Backend (ASP.NET Core)

Backend en Clean Architecture (Domain / Application / Infrastructure / API) para la gestión de campañas de Dungeons & Dragons 5e.

## Estructura

```
src/
  CasualDungeons.Domain/         Entidades y reglas de negocio puras (sin dependencias externas)
  CasualDungeons.Application/    Casos de uso, DTOs, interfaces de repositorio/servicio
  CasualDungeons.Infrastructure/ EF Core, JWT, BCrypt, SignalR, implementaciones de repositorio
  CasualDungeons.API/            Controladores REST, Program.cs, configuración
```

## Requisitos previos

- .NET SDK 8.0+
- SQL Server (local, contenedor Docker, o Azure SQL)

## Puesta en marcha

### 1. Base de datos

Este proyecto **no incluye migraciones de EF Core generadas**, ya que requieren el SDK de .NET para crearse de forma fiable (`dotnet ef migrations add` genera el snapshot del modelo automáticamente; escribirlo a mano es propenso a errores).

En su lugar, usa el script SQL completo entregado junto al backend:

```
database/01_CreateDatabase.sql
```

Este script crea la base de datos `CasualDungeons`, todas las tablas, claves, índices, restricciones, triggers de auditoría y los roles semilla.

Ejecuta el script contra tu instancia de SQL Server (por ejemplo con `sqlcmd`, Azure Data Studio o SQL Server Management Studio) **antes** de arrancar la API.

**Importante:** el script inserta un usuario `admin` con un hash de contraseña placeholder (`__REEMPLAZAR_CON_HASH_BCRYPT__`). Debes generar un hash BCrypt real y actualizarlo manualmente, o crear el primer usuario administrador a través de un script de inicialización aparte antes de exponer la API.

Alternativa con EF Core: si prefieres generar las migraciones desde el modelo (recomendado si vas a evolucionar el esquema con el tiempo), instala la herramienta y genera la migración inicial:

```bash
dotnet tool install --global dotnet-ef
cd src/CasualDungeons.API
dotnet ef migrations add InitialCreate --project ../CasualDungeons.Infrastructure --startup-project .
dotnet ef database update --project ../CasualDungeons.Infrastructure --startup-project .
```

Ten en cuenta que el modelo EF Core (`AppDbContext` + configuraciones Fluent API) está diseñado para producir un esquema equivalente al script SQL, pero los nombres exactos de constraints/índices generados automáticamente por EF Core pueden diferir ligeramente de los del script manual. Usa **una de las dos vías, no ambas**, para evitar conflictos.

### 2. Configuración

Edita `src/CasualDungeons.API/appsettings.json` (o, mejor, usa variables de entorno / `dotnet user-secrets` para no commitear secretos):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CasualDungeons;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "una-clave-secreta-aleatoria-de-al-menos-32-caracteres",
    "Issuer": "CasualDungeons.API",
    "Audience": "CasualDungeons.Client",
    "ExpiracionMinutos": 120
  },
  "Cors": {
    "OrigenesPermitidos": [ "http://localhost:4200" ]
  }
}
```

La API **falla al arrancar** si `Jwt:Secret` sigue siendo el valor de ejemplo, como medida de seguridad para evitar despliegues accidentales sin configurar.

Con user-secrets (recomendado en desarrollo):

```bash
cd src/CasualDungeons.API
dotnet user-secrets set "Jwt:Secret" "tu-clave-secreta-real"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "tu-cadena-de-conexion"
```

### 3. Ejecutar

```bash
cd src/CasualDungeons.API
dotnet restore
dotnet run
```

La API queda disponible en `https://localhost:5001` (o el puerto que asigne `dotnet run`), con Swagger UI en `/swagger` en entorno de desarrollo.

El hub de SignalR queda mapeado en `/hubs/campaign`.

## Autenticación

- `POST /api/auth/login` devuelve un JWT.
- Incluir el token en cada request como `Authorization: Bearer <token>`.
- Para SignalR desde el navegador, el token se pasa como query string `?access_token=<token>` al conectar (limitación de los transportes de SignalR en el navegador, gestionada ya en `Program.cs`).

## Roles y autorización

| Rol | Permisos |
|---|---|
| Administrador | CRUD de usuarios |
| DungeonMaster | CRUD de campañas propias, NPCs, combates |
| Jugador | Lectura/edición de su propia ficha de personaje |

## Reglas D&D 5e aplicadas en el dominio

- Modificador de atributo: `floor((valor - 10) / 2)`.
- Bonificador de competencia por nivel: `2 + floor((nivel - 1) / 4)`.
- Percepción pasiva: `10 + modificador de Sabiduría (+ bonificador de competencia si aplica)`.
- Rango válido de atributos: 1-30. Nivel de personaje: 1-20. Nivel de hechizo: 0 (trucos) a 9.

## Notas de diseño relevantes

- **Cascadas de borrado:** `InventarioItem` y `ParticipanteCombate` tienen referencias polimórficas controladas (Personaje XOR NPC). Para evitar el error de SQL Server por múltiples rutas de cascada, solo una de las dos relaciones usa `Cascade`; la otra usa `Restrict`, y el servicio de aplicación correspondiente (`NPCService.EliminarAsync`) limpia explícitamente los registros dependientes antes de eliminar.
- **Eliminación de usuarios:** es lógica (campo `Activo = false`), no física, porque un usuario puede tener campañas, personajes o participaciones asociadas con FK `Restrict`.

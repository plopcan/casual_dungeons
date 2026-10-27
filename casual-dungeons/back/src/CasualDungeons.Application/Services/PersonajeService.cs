using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Personajes;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Services;

/// <summary>
/// Lógica de la ficha de personaje. Aplica reglas D&D 5e en los
/// cálculos derivados (modificadores, bonificador de competencia,
/// percepción pasiva) y notifica al DM en tiempo real cuando un
/// jugador modifica su ficha.
/// </summary>
public class PersonajeService : IPersonajeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignNotifier _campaignNotifier;

    public PersonajeService(IUnitOfWork unitOfWork, ICampaignNotifier campaignNotifier)
    {
        _unitOfWork = unitOfWork;
        _campaignNotifier = campaignNotifier;
    }

    public async Task<Result<PersonajeFichaDto>> GetFichaAsync(int personajeId, int usuarioSolicitanteId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeFichaDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        var esPropietario = personaje.UsuarioId == usuarioSolicitanteId;
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(personaje.CampaniaId, usuarioSolicitanteId, cancellationToken);

        if (!esPropietario && !esDM)
            return Result<PersonajeFichaDto>.Fallo("No tienes acceso a esta ficha.", ResultErrorTipo.SinPermiso);

        // El DM consulta la ficha en modo solo lectura; el jugador
        // propietario puede editarla.
        return Result<PersonajeFichaDto>.Exito(MapearAFichaDto(personaje, soloLectura: !esPropietario));
    }

    public async Task<Result<List<PersonajeResumenDto>>> GetMisPersonajesEnCampaniaAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var personajes = await _unitOfWork.Personajes.GetTodosByCampaniaYUsuarioAsync(campaniaId, usuarioId, cancellationToken);

        // Nota: aquí no se considera un error tener 0 personajes (a
        // diferencia de la versión anterior, que fallaba con
        // NoEncontrado): el frontend distingue "sin personaje todavía"
        // de un error real a partir de una lista vacía.
        var resumen = personajes.Select(p => new PersonajeResumenDto(
            p.Id,
            p.Nombre,
            p.Usuario?.NombreUsuario ?? string.Empty,
            p.Clase,
            p.Nivel,
            p.PuntosVidaActuales,
            p.PuntosVidaMaximos,
            p.ClaseArmadura,
            p.PercepcionPasiva,
            p.Activo)).ToList();

        return Result<List<PersonajeResumenDto>>.Exito(resumen);
    }

    public async Task<Result<List<PersonajeResumenDto>>> GetResumenPorCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<List<PersonajeResumenDto>>.Fallo("Solo el Dungeon Master de la campaña puede ver este listado.", ResultErrorTipo.SinPermiso);

        // El DM solo ve los personajes activos: un jugador puede
        // desactivar (retirar) uno de los suyos sin que siga
        // apareciendo aquí ni como opción para añadir a combate.
        var personajes = await _unitOfWork.Personajes.GetByCampaniaAsync(campaniaId, soloActivos: true, cancellationToken);

        var resumen = personajes.Select(p => new PersonajeResumenDto(
            p.Id,
            p.Nombre,
            p.Usuario.NombreUsuario,
            p.Clase,
            p.Nivel,
            p.PuntosVidaActuales,
            p.PuntosVidaMaximos,
            p.ClaseArmadura,
            p.PercepcionPasiva,
            p.Activo)).ToList();

        return Result<List<PersonajeResumenDto>>.Exito(resumen);
    }

    public async Task<Result<PersonajeFichaDto>> CrearAsync(int campaniaId, int usuarioId, CrearPersonajeDto dto, CancellationToken cancellationToken = default)
    {
        var esParticipante = await _unitOfWork.Campanias.EsParticipanteAsync(campaniaId, usuarioId, cancellationToken);
        if (!esParticipante)
            return Result<PersonajeFichaDto>.Fallo("Debes ser jugador de esta campaña para crear un personaje en ella.", ResultErrorTipo.SinPermiso);

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<PersonajeFichaDto>.Fallo("El nombre del personaje es obligatorio.", ResultErrorTipo.Validacion);
        if (string.IsNullOrWhiteSpace(dto.Raza))
            return Result<PersonajeFichaDto>.Fallo("La raza es obligatoria.", ResultErrorTipo.Validacion);
        if (string.IsNullOrWhiteSpace(dto.Clase))
            return Result<PersonajeFichaDto>.Fallo("La clase es obligatoria.", ResultErrorTipo.Validacion);

        var fuerza = dto.Fuerza ?? 10;
        var destreza = dto.Destreza ?? 10;
        var constitucion = dto.Constitucion ?? 10;
        var inteligencia = dto.Inteligencia ?? 10;
        var sabiduria = dto.Sabiduria ?? 10;
        var carisma = dto.Carisma ?? 10;

        int[] atributos = { fuerza, destreza, constitucion, inteligencia, sabiduria, carisma };
        if (atributos.Any(a => a is < 1 or > 30))
            return Result<PersonajeFichaDto>.Fallo("Los atributos deben estar entre 1 y 30.", ResultErrorTipo.Validacion);

        // Reglas D&D 5e para un personaje recién creado (nivel 1):
        //  - Puntos de golpe = valor máximo del dado de golpe de su
        //    clase + modificador de Constitución (PHB: a nivel 1 no
        //    se tira el dado, se toma el máximo).
        //  - CA sin armadura = 10 + modificador de Destreza.
        //  - Bonificador de iniciativa = modificador de Destreza.
        //  - Percepción pasiva = 10 + modificador de Sabiduría
        //    (todavía sin competencias, que se añaden después desde
        //    la pestaña de habilidades).
        var dadoGolpe = ObtenerDadoGolpePorClase(dto.Clase);
        var valorDadoGolpe = int.Parse(dadoGolpe[1..]); // "d10" -> 10
        var modConstitucion = Personaje.CalcularModificador(constitucion);
        var modDestreza = Personaje.CalcularModificador(destreza);
        var modSabiduria = Personaje.CalcularModificador(sabiduria);

        var puntosVidaMaximos = Math.Max(1, valorDadoGolpe + modConstitucion);

        var personaje = new Personaje
        {
            CampaniaId = campaniaId,
            UsuarioId = usuarioId,
            Nombre = dto.Nombre.Trim(),
            Raza = dto.Raza.Trim(),
            Clase = dto.Clase.Trim(),
            Subclase = dto.Subclase,
            Trasfondo = dto.Trasfondo,
            Alineamiento = dto.Alineamiento,
            ImagenUrl = dto.ImagenUrl,
            Nivel = 1,
            Experiencia = 0,
            Fuerza = fuerza,
            Destreza = destreza,
            Constitucion = constitucion,
            Inteligencia = inteligencia,
            Sabiduria = sabiduria,
            Carisma = carisma,
            PuntosVidaActuales = puntosVidaMaximos,
            PuntosVidaMaximos = puntosVidaMaximos,
            PuntosVidaTemporales = 0,
            DadoGolpe = dadoGolpe,
            DadosGolpeRestantes = 1,
            ClaseArmadura = 10 + modDestreza,
            Velocidad = 30,
            IniciativaBonus = modDestreza,
            PercepcionPasiva = 10 + modSabiduria,
            Activo = true,
        };

        await _unitOfWork.Personajes.AddAsync(personaje, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarPersonajeActualizadoAsync(campaniaId, personaje.Id, cancellationToken);

        var fichaCompleta = await _unitOfWork.Personajes.GetFichaCompletaAsync(personaje.Id, cancellationToken);
        return Result<PersonajeFichaDto>.Exito(MapearAFichaDto(fichaCompleta!, soloLectura: false));
    }

    public async Task<Result<PersonajeActivoDto>> CambiarActivoAsync(int personajeId, int usuarioId, bool activo, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetByIdAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeActivoDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeActivoDto>.Fallo("Solo el propietario del personaje puede activarlo o desactivarlo.", ResultErrorTipo.SinPermiso);

        personaje.Activo = activo;
        _unitOfWork.Personajes.Update(personaje);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeActivoDto>.Exito(new PersonajeActivoDto(personaje.Id, personaje.Activo));
    }

    public async Task<Result<PersonajeFichaDto>> ActualizarAsync(int personajeId, int usuarioId, ActualizarPersonajeDto dto, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeFichaDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeFichaDto>.Fallo("Solo el propietario del personaje puede editar su ficha.", ResultErrorTipo.SinPermiso);

        var validacion = ValidarReglasBase(dto);
        if (!validacion.EsExitoso)
            return Result<PersonajeFichaDto>.Fallo(validacion.Error!, validacion.TipoError!.Value);

        personaje.Nombre = dto.Nombre;
        personaje.Raza = dto.Raza;
        personaje.Clase = dto.Clase;
        personaje.Subclase = dto.Subclase;
        personaje.Trasfondo = dto.Trasfondo;
        personaje.Alineamiento = dto.Alineamiento;
        personaje.ImagenUrl = dto.ImagenUrl;
        personaje.Nivel = dto.Nivel;
        personaje.Experiencia = dto.Experiencia;
        personaje.Fuerza = dto.Fuerza;
        personaje.Destreza = dto.Destreza;
        personaje.Constitucion = dto.Constitucion;
        personaje.Inteligencia = dto.Inteligencia;
        personaje.Sabiduria = dto.Sabiduria;
        personaje.Carisma = dto.Carisma;
        personaje.PuntosVidaActuales = dto.PuntosVidaActuales;
        personaje.PuntosVidaMaximos = dto.PuntosVidaMaximos;
        personaje.PuntosVidaTemporales = dto.PuntosVidaTemporales;
        personaje.DadoGolpe = dto.DadoGolpe;
        personaje.DadosGolpeRestantes = dto.DadosGolpeRestantes;
        personaje.ClaseArmadura = dto.ClaseArmadura;
        personaje.Velocidad = dto.Velocidad;
        personaje.IniciativaBonus = dto.IniciativaBonus;
        personaje.NotasGenerales = dto.NotasGenerales;

        // Percepción pasiva recalculada automáticamente según la
        // regla 5e: 10 + mod. Sabiduría (+ competencia si aplica).
        var competenciaPercepcion = personaje.Habilidades
            .FirstOrDefault(h => h.NombreHabilidad.Equals("Percepción", StringComparison.OrdinalIgnoreCase));
        personaje.PercepcionPasiva = personaje.CalcularPercepcionPasivaBase(
            competenciaPercepcion?.Competente ?? false,
            competenciaPercepcion?.Experticia ?? false);

        _unitOfWork.Personajes.Update(personaje);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeFichaDto>.Exito(MapearAFichaDto(personaje, soloLectura: false));
    }

    public async Task<Result<InventarioItemDto>> AgregarItemInventarioAsync(int personajeId, int usuarioId, GuardarInventarioItemDto dto, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<InventarioItemDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<InventarioItemDto>.Fallo("Solo el propietario del personaje puede editar el inventario.", ResultErrorTipo.SinPermiso);

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<InventarioItemDto>.Fallo("El nombre del ítem es obligatorio.", ResultErrorTipo.Validacion);

        var item = new InventarioItem
        {
            PersonajeId = personajeId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Cantidad = dto.Cantidad,
            Peso = dto.Peso,
            Equipado = dto.Equipado,
            Tipo = dto.Tipo
        };

        personaje.Inventario.Add(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<InventarioItemDto>.Exito(new InventarioItemDto(
            item.Id, item.Nombre, item.Descripcion, item.Cantidad, item.Peso, item.Equipado, item.Tipo));
    }

    public async Task<Result> EliminarItemInventarioAsync(int personajeId, int usuarioId, int itemId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result.Fallo("Solo el propietario del personaje puede editar el inventario.", ResultErrorTipo.SinPermiso);

        var item = personaje.Inventario.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return Result.Fallo("Ítem de inventario no encontrado.", ResultErrorTipo.NoEncontrado);

        personaje.Inventario.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result.Exito();
    }

    public async Task<Result<NotaDto>> AgregarNotaAsync(int personajeId, int usuarioId, GuardarNotaDto dto, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<NotaDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<NotaDto>.Fallo("Solo el propietario del personaje puede añadir notas.", ResultErrorTipo.SinPermiso);

        var ahora = DateTime.UtcNow;
        var nota = new Nota
        {
            PersonajeId = personajeId,
            Titulo = dto.Titulo,
            Contenido = dto.Contenido,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        personaje.Notas.Add(nota);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NotaDto>.Exito(new NotaDto(nota.Id, nota.Titulo, nota.Contenido, nota.FechaActualizacion));
    }

    public async Task<Result<NotaDto>> ActualizarNotaAsync(int personajeId, int usuarioId, int notaId, GuardarNotaDto dto, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<NotaDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<NotaDto>.Fallo("Solo el propietario del personaje puede editar notas.", ResultErrorTipo.SinPermiso);

        var nota = personaje.Notas.FirstOrDefault(n => n.Id == notaId);
        if (nota is null)
            return Result<NotaDto>.Fallo("Nota no encontrada.", ResultErrorTipo.NoEncontrado);

        nota.Titulo = dto.Titulo;
        nota.Contenido = dto.Contenido;
        // FechaActualizacion se refresca automáticamente por el
        // trigger TRG_Notas_FechaActualizacion en la base de datos.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NotaDto>.Exito(new NotaDto(nota.Id, nota.Titulo, nota.Contenido, DateTime.UtcNow));
    }

    public async Task<Result> EliminarNotaAsync(int personajeId, int usuarioId, int notaId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result.Fallo("Solo el propietario del personaje puede eliminar notas.", ResultErrorTipo.SinPermiso);

        var nota = personaje.Notas.FirstOrDefault(n => n.Id == notaId);
        if (nota is null)
            return Result.Fallo("Nota no encontrada.", ResultErrorTipo.NoEncontrado);

        personaje.Notas.Remove(nota);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    public async Task<Result<PersonajeHabilidadDto>> ActualizarHabilidadAsync(int personajeId, int usuarioId, int habilidadId, bool competente, bool experticia, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeHabilidadDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeHabilidadDto>.Fallo("Solo el propietario del personaje puede editar sus habilidades.", ResultErrorTipo.SinPermiso);

        var habilidad = personaje.Habilidades.FirstOrDefault(h => h.Id == habilidadId);
        if (habilidad is null)
            return Result<PersonajeHabilidadDto>.Fallo("Habilidad no encontrada.", ResultErrorTipo.NoEncontrado);

        // Regla 5e: la experticia exige competencia (duplica el
        // bonificador de competencia, no tiene sentido sin ella).
        habilidad.Competente = competente || experticia;
        habilidad.Experticia = experticia;

        // Si la habilidad editada es Percepción, recalcular la
        // percepción pasiva persistida del personaje.
        if (habilidad.NombreHabilidad.Equals("Percepción", StringComparison.OrdinalIgnoreCase))
        {
            personaje.PercepcionPasiva = personaje.CalcularPercepcionPasivaBase(habilidad.Competente, habilidad.Experticia);
            _unitOfWork.Personajes.Update(personaje);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeHabilidadDto>.Exito(new PersonajeHabilidadDto(
            habilidad.Id, habilidad.NombreHabilidad, habilidad.AtributoBase, habilidad.Competente, habilidad.Experticia));
    }

    public async Task<Result<PersonajeHechizoDto>> AgregarHechizoAsync(int personajeId, int usuarioId, int hechizoId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeHechizoDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeHechizoDto>.Fallo("Solo el propietario del personaje puede gestionar sus hechizos.", ResultErrorTipo.SinPermiso);

        var hechizo = await _unitOfWork.Hechizos.GetByIdAsync(hechizoId, cancellationToken);
        if (hechizo is null)
            return Result<PersonajeHechizoDto>.Fallo("Hechizo no encontrado en el catálogo.", ResultErrorTipo.NoEncontrado);

        if (personaje.Hechizos.Any(h => h.HechizoId == hechizoId))
            return Result<PersonajeHechizoDto>.Fallo("El personaje ya conoce ese hechizo.", ResultErrorTipo.Conflicto);

        var personajeHechizo = new PersonajeHechizo
        {
            PersonajeId = personajeId,
            HechizoId = hechizoId,
            Preparado = false
        };

        personaje.Hechizos.Add(personajeHechizo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonajeHechizoDto>.Exito(new PersonajeHechizoDto(
            personajeHechizo.Id, hechizo.Id, hechizo.Nombre, hechizo.Descripcion,hechizo.Nivel, hechizo.Escuela, personajeHechizo.Preparado));
    }

    public async Task<Result> QuitarHechizoAsync(int personajeId, int usuarioId, int personajeHechizoId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result.Fallo("Solo el propietario del personaje puede gestionar sus hechizos.", ResultErrorTipo.SinPermiso);

        var personajeHechizo = personaje.Hechizos.FirstOrDefault(h => h.Id == personajeHechizoId);
        if (personajeHechizo is null)
            return Result.Fallo("El personaje no tiene ese hechizo.", ResultErrorTipo.NoEncontrado);

        personaje.Hechizos.Remove(personajeHechizo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    public async Task<Result<PersonajeHechizoDto>> TogglePreparadoAsync(int personajeId, int usuarioId, int personajeHechizoId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeHechizoDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeHechizoDto>.Fallo("Solo el propietario del personaje puede gestionar sus hechizos.", ResultErrorTipo.SinPermiso);

        var personajeHechizo = personaje.Hechizos.FirstOrDefault(h => h.Id == personajeHechizoId);
        if (personajeHechizo is null)
            return Result<PersonajeHechizoDto>.Fallo("El personaje no tiene ese hechizo.", ResultErrorTipo.NoEncontrado);

        personajeHechizo.Preparado = !personajeHechizo.Preparado;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonajeHechizoDto>.Exito(new PersonajeHechizoDto(
            personajeHechizo.Id,
            personajeHechizo.HechizoId,
            personajeHechizo.Hechizo.Nombre,
            personajeHechizo.Hechizo.Descripcion,
            personajeHechizo.Hechizo.Nivel,
            personajeHechizo.Hechizo.Escuela,
            personajeHechizo.Preparado));
    }

    // ---------------- Espacios de conjuro ----------------
    //
    // Los totales por nivel los define libremente el jugador (no se
    // calculan a partir de Clase/Nivel): Clase es un campo de texto
    // libre pensado para admitir clases homebrew de la comunidad, y
    // cualquier tabla oficial de espacios por clase dejaría de tener
    // sentido en cuanto alguien meta una clase que esa tabla no
    // contempla. El jugador es quien mejor sabe cuántos espacios le
    // corresponden según su clase (oficial o no).

    public async Task<Result<PersonajeEspacioConjuroDto>> ConfigurarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, int espaciosTotales, CancellationToken cancellationToken = default)
    {
        if (nivelConjuro is < 1 or > 9)
            return Result<PersonajeEspacioConjuroDto>.Fallo("El nivel de conjuro debe estar entre 1 y 9.", ResultErrorTipo.Validacion);

        if (espaciosTotales < 0)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Los espacios totales no pueden ser negativos.", ResultErrorTipo.Validacion);

        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Solo el propietario del personaje puede configurar sus espacios de conjuro.", ResultErrorTipo.SinPermiso);

        var espacio = personaje.EspaciosConjuro.FirstOrDefault(e => e.NivelConjuro == nivelConjuro);
        if (espacio is null)
        {
            espacio = new PersonajeEspacioConjuro
            {
                PersonajeId = personajeId,
                NivelConjuro = nivelConjuro,
                EspaciosTotales = espaciosTotales,
                EspaciosUsados = 0
            };
            personaje.EspaciosConjuro.Add(espacio);
        }
        else
        {
            espacio.EspaciosTotales = espaciosTotales;

            // Si se reduce el total por debajo de los ya gastados,
            // recortamos los usados: la base de datos exige
            // EspaciosUsados <= EspaciosTotales (CK_EspaciosConjuro_Cantidades).
            if (espacio.EspaciosUsados > espacio.EspaciosTotales)
                espacio.EspaciosUsados = espacio.EspaciosTotales;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeEspacioConjuroDto>.Exito(new PersonajeEspacioConjuroDto(
            espacio.NivelConjuro, espacio.EspaciosTotales, espacio.EspaciosUsados, espacio.EspaciosDisponibles));
    }

    public async Task<Result<PersonajeEspacioConjuroDto>> GastarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Solo el propietario del personaje puede gastar sus espacios de conjuro.", ResultErrorTipo.SinPermiso);

        var espacio = personaje.EspaciosConjuro.FirstOrDefault(e => e.NivelConjuro == nivelConjuro);
        if (espacio is null)
            return Result<PersonajeEspacioConjuroDto>.Fallo("El personaje no tiene configurado ese nivel de espacio de conjuro.", ResultErrorTipo.NoEncontrado);

        if (espacio.EspaciosDisponibles <= 0)
            return Result<PersonajeEspacioConjuroDto>.Fallo("No quedan espacios de ese nivel disponibles.", ResultErrorTipo.Conflicto);

        espacio.EspaciosUsados++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeEspacioConjuroDto>.Exito(new PersonajeEspacioConjuroDto(
            espacio.NivelConjuro, espacio.EspaciosTotales, espacio.EspaciosUsados, espacio.EspaciosDisponibles));
    }

    public async Task<Result<PersonajeEspacioConjuroDto>> RecuperarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<PersonajeEspacioConjuroDto>.Fallo("Solo el propietario del personaje puede recuperar sus espacios de conjuro.", ResultErrorTipo.SinPermiso);

        var espacio = personaje.EspaciosConjuro.FirstOrDefault(e => e.NivelConjuro == nivelConjuro);
        if (espacio is null)
            return Result<PersonajeEspacioConjuroDto>.Fallo("El personaje no tiene configurado ese nivel de espacio de conjuro.", ResultErrorTipo.NoEncontrado);

        if (espacio.EspaciosUsados <= 0)
            return Result<PersonajeEspacioConjuroDto>.Fallo("No hay espacios gastados de ese nivel que recuperar.", ResultErrorTipo.Conflicto);

        espacio.EspaciosUsados--;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<PersonajeEspacioConjuroDto>.Exito(new PersonajeEspacioConjuroDto(
            espacio.NivelConjuro, espacio.EspaciosTotales, espacio.EspaciosUsados, espacio.EspaciosDisponibles));
    }

    public async Task<Result<List<PersonajeEspacioConjuroDto>>> ResetearEspaciosConjuroAsync(int personajeId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var personaje = await _unitOfWork.Personajes.GetFichaCompletaAsync(personajeId, cancellationToken);
        if (personaje is null)
            return Result<List<PersonajeEspacioConjuroDto>>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);

        if (personaje.UsuarioId != usuarioId)
            return Result<List<PersonajeEspacioConjuroDto>>.Fallo("Solo el propietario del personaje puede reiniciar sus espacios de conjuro.", ResultErrorTipo.SinPermiso);

        foreach (var espacio in personaje.EspaciosConjuro)
            espacio.EspaciosUsados = 0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _campaignNotifier.NotificarPersonajeActualizadoAsync(personaje.CampaniaId, personaje.Id, cancellationToken);

        return Result<List<PersonajeEspacioConjuroDto>>.Exito(personaje.EspaciosConjuro
            .OrderBy(e => e.NivelConjuro)
            .Select(e => new PersonajeEspacioConjuroDto(e.NivelConjuro, e.EspaciosTotales, e.EspaciosUsados, e.EspaciosDisponibles))
            .ToList());
    }

    /// <summary>
    /// Dado de golpe por clase según D&D 5e (PHB), usado al crear un
    /// personaje nuevo. Acepta nombres en español e inglés, sin
    /// distinguir mayúsculas/acentos. Si la clase no se reconoce
    /// (homebrew, variante, etc.) se usa d8 como valor razonable por
    /// defecto, similar al de la mayoría de clases del manual.
    /// </summary>
    private static readonly Dictionary<string, string> DadoGolpePorClase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["barbaro"] = "d12",
        ["barbarian"] = "d12",

        ["guerrero"] = "d10",
        ["fighter"] = "d10",
        ["paladin"] = "d10",
        ["explorador"] = "d10",
        ["guardabosques"] = "d10",
        ["ranger"] = "d10",

        ["bardo"] = "d8",
        ["bard"] = "d8",
        ["clerigo"] = "d8",
        ["cleric"] = "d8",
        ["druida"] = "d8",
        ["druid"] = "d8",
        ["monje"] = "d8",
        ["monk"] = "d8",
        ["picaro"] = "d8",
        ["ladron"] = "d8",
        ["rogue"] = "d8",
        ["brujo"] = "d8",
        ["warlock"] = "d8",

        ["hechicero"] = "d6",
        ["sorcerer"] = "d6",
        ["mago"] = "d6",
        ["wizard"] = "d6",
    };

    private static string ObtenerDadoGolpePorClase(string clase)
    {
        // Quita tildes para que "Clérigo"/"clerigo" o "Pícaro"/"picaro"
        // resuelvan igual sin mantener cada variante acentuada en el
        // diccionario.
        var claseNormalizada = new string(clase.Trim()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        return DadoGolpePorClase.TryGetValue(claseNormalizada, out var dado) ? dado : "d8";
    }

    private static Result ValidarReglasBase(ActualizarPersonajeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result.Fallo("El nombre del personaje es obligatorio.", ResultErrorTipo.Validacion);

        if (dto.Nivel is < 1 or > 20)
            return Result.Fallo("El nivel debe estar entre 1 y 20.", ResultErrorTipo.Validacion);

        int[] atributos = { dto.Fuerza, dto.Destreza, dto.Constitucion, dto.Inteligencia, dto.Sabiduria, dto.Carisma };
        if (atributos.Any(a => a is < 1 or > 30))
            return Result.Fallo("Los atributos deben estar entre 1 y 30.", ResultErrorTipo.Validacion);

        if (dto.PuntosVidaMaximos < 1)
            return Result.Fallo("Los puntos de vida máximos deben ser al menos 1.", ResultErrorTipo.Validacion);

        if (dto.PuntosVidaActuales < 0)
            return Result.Fallo("Los puntos de vida actuales no pueden ser negativos.", ResultErrorTipo.Validacion);

        if (dto.Experiencia < 0)
            return Result.Fallo("La experiencia no puede ser negativa.", ResultErrorTipo.Validacion);

        return Result.Exito();
    }

    private static PersonajeFichaDto MapearAFichaDto(Personaje p, bool soloLectura) => new(
        p.Id,
        p.CampaniaId,
        p.UsuarioId,
        p.Nombre,
        p.Raza,
        p.Clase,
        p.Subclase,
        p.Trasfondo,
        p.Alineamiento,
        p.ImagenUrl,
        p.Nivel,
        p.Experiencia,
        p.Fuerza,
        p.Destreza,
        p.Constitucion,
        p.Inteligencia,
        p.Sabiduria,
        p.Carisma,
        p.ModificadorFuerza,
        p.ModificadorDestreza,
        p.ModificadorConstitucion,
        p.ModificadorInteligencia,
        p.ModificadorSabiduria,
        p.ModificadorCarisma,
        p.BonificadorCompetencia,
        p.PuntosVidaActuales,
        p.PuntosVidaMaximos,
        p.PuntosVidaTemporales,
        p.DadoGolpe,
        p.DadosGolpeRestantes,
        p.ClaseArmadura,
        p.Velocidad,
        p.IniciativaBonus,
        p.PercepcionPasiva,
        p.Inventario.Select(i => new InventarioItemDto(i.Id, i.Nombre, i.Descripcion, i.Cantidad, i.Peso, i.Equipado, i.Tipo)).ToList(),
        p.Habilidades.Select(h => new PersonajeHabilidadDto(h.Id, h.NombreHabilidad, h.AtributoBase, h.Competente, h.Experticia)).ToList(),
        p.Hechizos.Select(ph => new PersonajeHechizoDto(ph.Id, ph.HechizoId, ph.Hechizo.Nombre, ph.Hechizo.Descripcion, ph.Hechizo.Nivel, ph.Hechizo.Escuela, ph.Preparado)).ToList(),
        p.EspaciosConjuro.Select(e => new PersonajeEspacioConjuroDto(e.NivelConjuro, e.EspaciosTotales, e.EspaciosUsados, e.EspaciosDisponibles)).ToList(),
        p.Notas.Select(n => new NotaDto(n.Id, n.Titulo, n.Contenido, n.FechaActualizacion)).ToList(),
        p.NotasGenerales,
        p.Activo,
        soloLectura);
}

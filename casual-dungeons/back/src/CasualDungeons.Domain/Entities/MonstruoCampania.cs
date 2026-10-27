using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Monstruo propio de una campaña, creado por el Dungeon Master a
/// mano o copiado desde MonstruoCatalogo. A diferencia de NPC (que
/// modela un personaje no jugable de rol, con Nivel/Notas), un
/// MonstruoCampania es un bloque de estadísticas de combate con los
/// mismos campos que el catálogo precreado. Tabla dnd.MonstruosCampania.
/// </summary>
public class MonstruoCampania : BaseEntity
{
    public int CampaniaId { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }

    /// <summary>Ej. "Humanoide (goblinoide)", "No-muerto", "Dragón".</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Diminuto, Pequeño, Mediano, Grande, Enorme, Gargantuesco.</summary>
    public string Tamano { get; set; } = string.Empty;

    public string? Alineamiento { get; set; }

    public int ClaseArmadura { get; set; } = 10;

    /// <summary>Ej. "armadura de cuero, escudo", "natural".</summary>
    public string? DescripcionArmadura { get; set; }

    public int PuntosVidaMaximos { get; set; } = 1;

    /// <summary>Snapshot editable de PV actuales (daño recibido en campaña/combate).</summary>
    public int PuntosVidaActuales { get; set; } = 1;

    /// <summary>Fórmula de dados de golpe, ej. "2d6+2".</summary>
    public string? DadoGolpe { get; set; }

    /// <summary>Texto libre para poder listar varios modos, ej. "9 m, vuelo 18 m".</summary>
    public string? Velocidad { get; set; }

    public int Fuerza { get; set; } = 10;
    public int Destreza { get; set; } = 10;
    public int Constitucion { get; set; } = 10;
    public int Inteligencia { get; set; } = 10;
    public int Sabiduria { get; set; } = 10;
    public int Carisma { get; set; } = 10;

    public string? Salvaciones { get; set; }
    public string? Habilidades { get; set; }
    public string? ResistenciasDano { get; set; }
    public string? InmunidadesDano { get; set; }
    public string? InmunidadesEstado { get; set; }
    public string? Sentidos { get; set; }
    public string? Idiomas { get; set; }

    /// <summary>Nivel de desafío como texto para admitir fracciones: "1/8", "1/4", "1/2", "5".</summary>
    public string? Desafio { get; set; }

    public int? PuntosExperiencia { get; set; }

    /// <summary>
    /// Rasgos, Acciones, Acciones legendarias y Reacciones como un
    /// único bloque de texto libre (a diferencia del catálogo
    /// precreado, que los modela como filas estructuradas). El DM
    /// lo rellena tal cual lo escribiría en una hoja de monstruo.
    /// </summary>
    public string? RasgosAcciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Campania Campania { get; set; } = null!;

    public int ModificadorFuerza => Personaje.CalcularModificador(Fuerza);
    public int ModificadorDestreza => Personaje.CalcularModificador(Destreza);
    public int ModificadorConstitucion => Personaje.CalcularModificador(Constitucion);
    public int ModificadorInteligencia => Personaje.CalcularModificador(Inteligencia);
    public int ModificadorSabiduria => Personaje.CalcularModificador(Sabiduria);
    public int ModificadorCarisma => Personaje.CalcularModificador(Carisma);
}

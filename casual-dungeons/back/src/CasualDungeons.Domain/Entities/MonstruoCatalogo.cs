using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Catálogo general de monstruos/NPC precreados de D&D 5e, para que
/// el DM pueda añadirlos a una campaña como NPC sin teclear su
/// estadística a mano. Tabla dnd.MonstruosCatalogo.
///
/// IMPORTANTE (licencia de contenido): solo se cargan aquí criaturas
/// publicadas como contenido abierto en el SRD 5.1 (System Reference
/// Document) de Wizards of the Coast. El Manual de Monstruos
/// completo no puede reproducirse (no es contenido abierto); por
/// eso el catálogo no cubre criaturas exclusivas de esa obra (ej.
/// mind flayer, beholder) aunque su nombre pueda sonar familiar.
/// </summary>
public class MonstruoCatalogo : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Ej. "Humanoide (goblinoide)", "No-muerto", "Dragón".</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Diminuto, Pequeño, Mediano, Grande, Enorme, Gargantuesco.</summary>
    public string Tamano { get; set; } = string.Empty;

    public string? Alineamiento { get; set; }

    public int ClaseArmadura { get; set; }

    /// <summary>Ej. "armadura de cuero, escudo", "natural".</summary>
    public string? DescripcionArmadura { get; set; }

    public int PuntosVidaPromedio { get; set; }

    /// <summary>Fórmula de dados de golpe, ej. "2d6+2".</summary>
    public string DadoGolpe { get; set; } = string.Empty;

    /// <summary>Texto libre para poder listar varios modos, ej. "9 m, vuelo 18 m".</summary>
    public string Velocidad { get; set; } = string.Empty;

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
    public string Desafio { get; set; } = "0";

    public int PuntosExperiencia { get; set; }

    /// <summary>Origen del contenido, ej. "SRD 5.1", para dejar claro qué es reproducible.</summary>
    public string Fuente { get; set; } = "SRD 5.1";

    public DateTime FechaCreacion { get; set; }

    public ICollection<MonstruoCatalogoDetalle> Detalles { get; set; } = new List<MonstruoCatalogoDetalle>();

    public int ModificadorFuerza => Personaje.CalcularModificador(Fuerza);
    public int ModificadorDestreza => Personaje.CalcularModificador(Destreza);
    public int ModificadorConstitucion => Personaje.CalcularModificador(Constitucion);
    public int ModificadorInteligencia => Personaje.CalcularModificador(Inteligencia);
    public int ModificadorSabiduria => Personaje.CalcularModificador(Sabiduria);
    public int ModificadorCarisma => Personaje.CalcularModificador(Carisma);
}

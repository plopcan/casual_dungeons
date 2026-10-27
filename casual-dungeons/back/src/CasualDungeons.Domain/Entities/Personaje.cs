using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Ficha de personaje jugador (PC). Tabla dnd.Personajes.
///
/// Reglas D&D 5e aplicadas:
///   - Modificador de atributo = floor((valor - 10) / 2).
///   - Percepción pasiva = 10 + modificador de Sabiduría
///     (+ bonificación de competencia si el personaje es
///     competente en Percepción, gestionado en
///     PersonajeHabilidades; aquí se expone el cálculo base).
///   - Nivel válido: 1-20.
///   - Atributos válidos: 1-30.
/// </summary>
public class Personaje : BaseEntity
{
    public int CampaniaId { get; set; }
    public int UsuarioId { get; set; }

    // Información general
    public string Nombre { get; set; } = string.Empty;
    public string Raza { get; set; } = string.Empty;
    public string Clase { get; set; } = string.Empty;
    public string? Subclase { get; set; }
    public string? Trasfondo { get; set; }
    public string? Alineamiento { get; set; }
    public string? ImagenUrl { get; set; }

    // Progresión
    public int Nivel { get; set; } = 1;
    public int Experiencia { get; set; } = 0;

    // Combate
    public int PuntosVidaActuales { get; set; }
    public int PuntosVidaMaximos { get; set; } = 1;
    public int PuntosVidaTemporales { get; set; } = 0;
    public string? DadoGolpe { get; set; }
    public int DadosGolpeRestantes { get; set; } = 1;
    public int ClaseArmadura { get; set; } = 10;
    public int Velocidad { get; set; } = 30;
    public int IniciativaBonus { get; set; } = 0;

    /// <summary>
    /// Percepción pasiva persistida (incluye ajustes manuales del
    /// DM/jugador, p.ej. por objetos mágicos). El valor base sin
    /// ajustes se puede recalcular con CalcularPercepcionPasivaBase().
    /// </summary>
    public int PercepcionPasiva { get; set; } = 10;

    // Atributos (1-30)
    public int Fuerza { get; set; } = 10;
    public int Destreza { get; set; } = 10;
    public int Constitucion { get; set; } = 10;
    public int Inteligencia { get; set; } = 10;
    public int Sabiduria { get; set; } = 10;
    public int Carisma { get; set; } = 10;

    public string? NotasGenerales { get; set; }

    /// <summary>
    /// Permite a un jugador "retirar" temporalmente uno de sus
    /// personajes sin borrarlo (ej. tras morir, o si deja de usarlo
    /// por llevar varios PCs a la vez). El DM solo ve en su pantalla
    /// los personajes activos: los inactivos no aparecen en la
    /// pestaña "Jugadores" ni como opción para añadir a combate.
    /// </summary>
    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public Campania Campania { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;

    public ICollection<PersonajeHabilidad> Habilidades { get; set; } = new List<PersonajeHabilidad>();
    public ICollection<PersonajeHechizo> Hechizos { get; set; } = new List<PersonajeHechizo>();
    public ICollection<PersonajeEspacioConjuro> EspaciosConjuro { get; set; } = new List<PersonajeEspacioConjuro>();
    public ICollection<InventarioItem> Inventario { get; set; } = new List<InventarioItem>();
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();

    // ---------------------------------------------------------------
    // Reglas D&D 5e: cálculos derivados
    // ---------------------------------------------------------------

    /// <summary>
    /// Modificador de cualquier atributo según 5e: floor((valor-10)/2).
    /// Se usa división entera con redondeo hacia -infinito para
    /// reproducir correctamente valores impares y negativos
    /// (ej. atributo 7 -> modificador -2, no -1).
    /// </summary>
    public static int CalcularModificador(int valorAtributo)
        => (int)Math.Floor((valorAtributo - 10) / 2.0);

    public int ModificadorFuerza => CalcularModificador(Fuerza);
    public int ModificadorDestreza => CalcularModificador(Destreza);
    public int ModificadorConstitucion => CalcularModificador(Constitucion);
    public int ModificadorInteligencia => CalcularModificador(Inteligencia);
    public int ModificadorSabiduria => CalcularModificador(Sabiduria);
    public int ModificadorCarisma => CalcularModificador(Carisma);

    /// <summary>
    /// Bonificador de competencia según nivel de personaje (5e):
    /// +2 en niveles 1-4, +3 en 5-8, +4 en 9-12, +5 en 13-16, +6 en 17-20.
    /// </summary>
    public int BonificadorCompetencia => 2 + (Nivel - 1) / 4;

    /// <summary>
    /// Percepción pasiva base sin objetos mágicos ni ventaja/desventaja:
    /// 10 + modificador de Sabiduría (+ bonif. de competencia si aplica).
    /// </summary>
    public int CalcularPercepcionPasivaBase(bool competenteEnPercepcion, bool experticiaEnPercepcion = false)
    {
        var bonifCompetencia = 0;
        if (experticiaEnPercepcion)
            bonifCompetencia = BonificadorCompetencia * 2;
        else if (competenteEnPercepcion)
            bonifCompetencia = BonificadorCompetencia;

        return 10 + ModificadorSabiduria + bonifCompetencia;
    }
}

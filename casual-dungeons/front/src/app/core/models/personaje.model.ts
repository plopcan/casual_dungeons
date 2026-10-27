export interface PersonajeResumen {
  id: number;
  nombrePersonaje: string;
  nombreJugador: string;
  clase: string;
  nivel: number;
  puntosVidaActuales: number;
  puntosVidaMaximos: number;
  claseArmadura: number;
  percepcionPasiva: number;
  activo: boolean;
}

export interface InventarioItem {
  id: number;
  nombre: string;
  descripcion: string | null;
  cantidad: number;
  peso: number | null;
  equipado: boolean;
  tipo: string | null;
}

export interface GuardarInventarioItemRequest {
  nombre: string;
  descripcion: string | null;
  cantidad: number;
  peso: number | null;
  equipado: boolean;
  tipo: string | null;
}

export interface PersonajeHabilidad {
  id: number;
  nombreHabilidad: string;
  atributoBase: string;
  competente: boolean;
  experticia: boolean;
}

export interface PersonajeHechizo {
  id: number;
  hechizoId: number;
  nombreHechizo: string;
  nivelHechizo: number;
  escuela: string | null;
  preparado: boolean;
  descripcionHechizo: string | null;
}

export interface PersonajeEspacioConjuro {
  nivelConjuro: number;
  espaciosTotales: number;
  espaciosUsados: number;
  espaciosDisponibles: number;
}

export interface Nota {
  id: number;
  titulo: string | null;
  contenido: string | null;
  fechaActualizacion: string;
}

export interface GuardarNotaRequest {
  titulo: string | null;
  contenido: string | null;
}

/** Hechizo del catálogo general (tabla Hechizos del backend). */
export interface HechizoCatalogo {
  id: number;
  nombre: string;
  nivel: number;
  escuela: string | null;
  tiempoLanzamiento: string | null;
  alcance: string | null;
  componentes: string | null;
  duracion: string | null;
  descripcion: string | null;
}

/**
 * Ficha completa del personaje. Coincide campo a campo con
 * PersonajeFichaDto del backend (41 propiedades).
 */
export interface PersonajeFicha {
  id: number;
  campaniaId: number;
  usuarioId: number;

  // Información general
  nombre: string;
  raza: string;
  clase: string;
  subclase: string | null;
  trasfondo: string | null;
  alineamiento: string | null;
  imagenUrl: string | null;
  nivel: number;
  experiencia: number;

  // Estadísticas
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  modificadorFuerza: number;
  modificadorDestreza: number;
  modificadorConstitucion: number;
  modificadorInteligencia: number;
  modificadorSabiduria: number;
  modificadorCarisma: number;
  bonificadorCompetencia: number;

  // Combate
  puntosVidaActuales: number;
  puntosVidaMaximos: number;
  puntosVidaTemporales: number;
  dadoGolpe: string | null;
  dadosGolpeRestantes: number;
  claseArmadura: number;
  velocidad: number;
  iniciativaBonus: number;
  percepcionPasiva: number;

  // Colecciones
  inventario: InventarioItem[];
  habilidades: PersonajeHabilidad[];
  hechizos: PersonajeHechizo[];
  espaciosConjuro: PersonajeEspacioConjuro[];
  notas: Nota[];

  notasGenerales: string | null;
  activo: boolean;
  soloLectura: boolean;
}

/**
 * Datos para crear un personaje nuevo (nivel 1). Coincide con
 * CrearPersonajeDto del backend. Los atributos son opcionales: si
 * se omiten, el backend usa 10 (modificador +0).
 */
export interface CrearPersonajeRequest {
  nombre: string;
  raza: string;
  clase: string;
  subclase: string | null;
  trasfondo: string | null;
  alineamiento: string | null;
  imagenUrl: string | null;
  fuerza: number | null;
  destreza: number | null;
  constitucion: number | null;
  inteligencia: number | null;
  sabiduria: number | null;
  carisma: number | null;
}

/**
 * Payload de actualización de la ficha. Coincide con
 * ActualizarPersonajeDto del backend.
 */
export interface ActualizarPersonajeRequest {
  nombre: string;
  raza: string;
  clase: string;
  subclase: string | null;
  trasfondo: string | null;
  alineamiento: string | null;
  imagenUrl: string | null;
  nivel: number;
  experiencia: number;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  puntosVidaActuales: number;
  puntosVidaMaximos: number;
  puntosVidaTemporales: number;
  dadoGolpe: string | null;
  dadosGolpeRestantes: number;
  claseArmadura: number;
  velocidad: number;
  iniciativaBonus: number;
  notasGenerales: string | null;
}

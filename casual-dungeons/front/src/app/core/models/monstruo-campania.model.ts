export interface MonstruoCampania {
  id: number;
  campaniaId: number;
  nombre: string;
  imagenUrl: string | null;
  tipo: string;
  tamano: string;
  alineamiento: string | null;
  claseArmadura: number;
  descripcionArmadura: string | null;
  puntosVidaMaximos: number;
  puntosVidaActuales: number;
  dadoGolpe: string | null;
  velocidad: string | null;
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
  salvaciones: string | null;
  habilidades: string | null;
  resistenciasDano: string | null;
  inmunidadesDano: string | null;
  inmunidadesEstado: string | null;
  sentidos: string | null;
  idiomas: string | null;
  desafio: string | null;
  puntosExperiencia: number | null;
  /** Rasgos, Acciones, Acciones legendarias y Reacciones como un único bloque de texto libre. */
  rasgosAcciones: string | null;
}

export interface GuardarMonstruoCampaniaRequest {
  nombre: string;
  imagenUrl: string | null;
  tipo: string;
  tamano: string;
  alineamiento: string | null;
  claseArmadura: number;
  descripcionArmadura: string | null;
  puntosVidaMaximos: number;
  puntosVidaActuales: number;
  dadoGolpe: string | null;
  velocidad: string | null;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  salvaciones: string | null;
  habilidades: string | null;
  resistenciasDano: string | null;
  inmunidadesDano: string | null;
  inmunidadesEstado: string | null;
  sentidos: string | null;
  idiomas: string | null;
  desafio: string | null;
  puntosExperiencia: number | null;
  rasgosAcciones: string | null;
}

export interface CrearMonstruoCampaniaDesdeCatalogoRequest {
  monstruoCatalogoId: number;
  nombrePersonalizado: string | null;
}

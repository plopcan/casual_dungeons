export type EstadoCombate = 'EnPreparacion' | 'EnCurso' | 'Finalizado';

export type OrigenParticipante = 'Personaje' | 'NPC' | 'MonstruoCampania';

export interface ParticipanteCombate {
  id: number;
  personajeId: number | null;
  npcId: number | null;
  monstruoCampaniaId: number | null;
  nombre: string;
  ordenIniciativa: number;
  valorIniciativa: number | null;
  puntosVidaActuales: number | null;
  origen: OrigenParticipante;
}

export interface Combate {
  id: number;
  campaniaId: number;
  estado: EstadoCombate;
  fecha: string;
  rondaActual: number;
  participantes: ParticipanteCombate[];
}

export interface CrearCombateRequest {
  campaniaId: number;
}

export interface AgregarParticipanteCombateRequest {
  personajeId: number | null;
  npcId: number | null;
  monstruoCampaniaId: number | null;
  valorIniciativa: number | null;
}

export interface ReordenarParticipantesRequest {
  participanteCombateIdsEnOrden: number[];
}

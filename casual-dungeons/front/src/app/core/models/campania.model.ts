export type EstadoCampania = 'Activa' | 'Finalizada';

export interface CampaniaResumen {
  id: number;
  nombre: string;
  descripcion: string | null;
  imagenUrl: string | null;
  fechaInicio: string;
  fechaFinalizacion: string | null;
  estado: EstadoCampania;
  esDungeonMaster: boolean;
}

export interface ParticipanteCampania {
  usuarioId: number;
  nombreUsuario: string;
  personajeId: number | null;
  personajeNombre: string | null;
}

export interface CampaniaDetalle {
  id: number;
  nombre: string;
  descripcion: string | null;
  imagenUrl: string | null;
  fechaInicio: string;
  fechaFinalizacion: string | null;
  estado: EstadoCampania;
  dungeonMasterId: number;
  dungeonMasterNombre: string;
  participantes: ParticipanteCampania[];
}

export interface CrearCampaniaRequest {
  nombre: string;
  descripcion: string | null;
  imagenUrl: string | null;
  fechaInicio: string;
  jugadoresIds: number[];
}

export interface EditarCampaniaRequest {
  nombre: string;
  descripcion: string | null;
  imagenUrl: string | null;
  fechaInicio: string;
  fechaFinalizacion: string | null;
  estado: EstadoCampania;
  jugadoresIds: number[];
}

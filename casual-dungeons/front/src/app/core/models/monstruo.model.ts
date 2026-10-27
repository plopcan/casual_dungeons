export interface MonstruoCatalogoResumen {
  id: number;
  nombre: string;
  tipo: string;
  tamano: string;
  desafio: string;
  puntosVidaPromedio: number;
  claseArmadura: number;
}

export interface MonstruoCatalogoDetalleItem {
  tipo: 'Rasgo' | 'Accion' | 'AccionLegendaria' | 'Reaccion';
  nombre: string;
  descripcion: string;
}

export interface MonstruoCatalogoFicha {
  id: number;
  nombre: string;
  tipo: string;
  tamano: string;
  alineamiento: string | null;
  claseArmadura: number;
  descripcionArmadura: string | null;
  puntosVidaPromedio: number;
  dadoGolpe: string;
  velocidad: string;
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
  desafio: string;
  puntosExperiencia: number;
  fuente: string;
  detalles: MonstruoCatalogoDetalleItem[];
}

export interface CrearNpcDesdeCatalogoRequest {
  monstruoId: number;
  nombrePersonalizado: string | null;
}

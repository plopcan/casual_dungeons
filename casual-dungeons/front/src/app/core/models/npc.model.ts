export interface NPC {
  id: number;
  campaniaId: number;
  nombre: string;
  imagenUrl: string | null;
  nivel: number;
  puntosVida: number;
  puntosVidaMaximos: number;
  claseArmadura: number;
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
  notas: string | null;
}

export interface GuardarNPCRequest {
  nombre: string;
  imagenUrl: string | null;
  nivel: number;
  puntosVida: number;
  puntosVidaMaximos: number;
  claseArmadura: number;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  notas: string | null;
}

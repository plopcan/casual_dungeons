export interface LoginRequest {
  nombreUsuario: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiraEn: string;
  usuarioId: number;
  nombreUsuario: string;
  rol: RolUsuario;
}

export type RolUsuario = 'Administrador' | 'DungeonMaster' | 'Jugador';

export interface CambiarPasswordRequest {
  passwordActual: string;
  passwordNueva: string;
}

export interface CambiarNombreUsuarioRequest {
  nuevoNombreUsuario: string;
}

export interface Usuario {
  id: number;
  nombreUsuario: string;
  email: string;
  rol: RolUsuario;
  fechaCreacion: string;
  activo: boolean;
  ultimoAcceso: string | null;
}

export interface CrearUsuarioRequest {
  nombreUsuario: string;
  password: string;
  email: string;
  rol: RolUsuario;
}

export interface EditarUsuarioRequest {
  nombreUsuario: string;
  email: string;
  rol: RolUsuario;
  activo: boolean;
  nuevaPassword?: string | null;
}

export interface ConfiguracionUsuario {
  temaOscuro: boolean;
}

/** Proyección mínima de usuario para selectores (ej. invitar jugadores). */
export interface UsuarioBasico {
  id: number;
  nombreUsuario: string;
}

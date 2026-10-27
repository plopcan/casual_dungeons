import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CambiarNombreUsuarioRequest,
  CambiarPasswordRequest,
  LoginRequest,
  LoginResponse,
  RolUsuario,
} from '../models';

interface SesionPersistida {
  token: string;
  expiraEn: string;
  usuarioId: number;
  nombreUsuario: string;
  rol: RolUsuario;
}

const CLAVE_STORAGE = 'dnd_sesion';

/**
 * Gestiona el estado de autenticación de toda la aplicación usando
 * Signals. La sesión se persiste en localStorage para sobrevivir
 * recargas de página; al iniciar el servicio se restaura desde ahí
 * si el token todavía no ha expirado.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  private readonly _sesion = signal<SesionPersistida | null>(this.restaurarSesion());

  readonly estaAutenticado = computed(() => this._sesion() !== null);
  readonly usuarioId = computed(() => this._sesion()?.usuarioId ?? null);
  readonly nombreUsuario = computed(() => this._sesion()?.nombreUsuario ?? null);
  readonly rol = computed<RolUsuario | null>(() => this._sesion()?.rol ?? null);
  readonly token = computed(() => this._sesion()?.token ?? null);

  readonly esAdministrador = computed(() => this.rol() === 'Administrador');
  readonly esDungeonMaster = computed(() => this.rol() === 'DungeonMaster');
  readonly esJugador = computed(() => this.rol() === 'Jugador');

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((respuesta) => this.guardarSesion(respuesta)),
    );
  }

  logout(): void {
    this._sesion.set(null);
    localStorage.removeItem(CLAVE_STORAGE);
  }

  cambiarPassword(request: CambiarPasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/password`, request);
  }

  cambiarNombreUsuario(request: CambiarNombreUsuarioRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/nombre-usuario`, request).pipe(
      tap(() => {
        const actual = this._sesion();
        if (actual) {
          this._sesion.set({ ...actual, nombreUsuario: request.nuevoNombreUsuario });
          this.persistir();
        }
      }),
    );
  }

  private guardarSesion(respuesta: LoginResponse): void {
    this._sesion.set({
      token: respuesta.token,
      expiraEn: respuesta.expiraEn,
      usuarioId: respuesta.usuarioId,
      nombreUsuario: respuesta.nombreUsuario,
      rol: respuesta.rol,
    });
    this.persistir();
  }

  private persistir(): void {
    const sesion = this._sesion();
    if (sesion) {
      localStorage.setItem(CLAVE_STORAGE, JSON.stringify(sesion));
    }
  }

  private restaurarSesion(): SesionPersistida | null {
    const guardado = localStorage.getItem(CLAVE_STORAGE);
    if (!guardado) return null;

    try {
      const sesion: SesionPersistida = JSON.parse(guardado);
      const expiracion = new Date(sesion.expiraEn).getTime();

      if (Number.isNaN(expiracion) || expiracion <= Date.now()) {
        localStorage.removeItem(CLAVE_STORAGE);
        return null;
      }

      return sesion;
    } catch {
      localStorage.removeItem(CLAVE_STORAGE);
      return null;
    }
  }
}

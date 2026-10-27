import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ConfiguracionUsuario } from '../models';

type Tema = 'dark' | 'light';

const CLAVE_STORAGE = 'dnd_tema';

/**
 * Controla el tema visual de la aplicación. El modo oscuro es el
 * predeterminado (requisito de producto). El tema se aplica como
 * atributo data-theme en <html>, lo que activa las variables CSS
 * correspondientes definidas en _tokens.scss.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/usuarios/me/configuracion`;

  private readonly _tema = signal<Tema>(this.leerTemaInicial());

  readonly tema = computed(() => this._tema());
  readonly esOscuro = computed(() => this._tema() === 'dark');

  constructor() {
    // Aplica el atributo data-theme al <html> cada vez que cambia
    // el signal, incluyendo la carga inicial.
    effect(() => {
      document.documentElement.setAttribute('data-theme', this._tema());
    });
  }

  alternar(): void {
    this.establecer(this._tema() === 'dark' ? 'light' : 'dark');
  }

  establecer(tema: Tema): void {
    this._tema.set(tema);
    localStorage.setItem(CLAVE_STORAGE, tema);
    this.http
      .put<void>(this.apiUrl, { temaOscuro: tema === 'dark' } satisfies ConfiguracionUsuario)
      .subscribe({
        // El fallo de sincronización con backend no debe romper la
        // experiencia local; el tema ya quedó aplicado y persistido
        // localmente.
        error: () => undefined,
      });
  }

  /**
   * Carga la configuración persistida en el backend (llamar tras
   * login, para reflejar la preferencia guardada en otro dispositivo).
   */
  cargarDesdeServidor(): void {
    this.http.get<ConfiguracionUsuario>(this.apiUrl).subscribe({
      next: (config) => {
        const tema: Tema = config.temaOscuro ? 'dark' : 'light';
        this._tema.set(tema);
        localStorage.setItem(CLAVE_STORAGE, tema);
      },
      error: () => undefined,
    });
  }

  private leerTemaInicial(): Tema {
    const guardado = localStorage.getItem(CLAVE_STORAGE);
    return guardado === 'light' ? 'light' : 'dark'; // oscuro por defecto
  }
}

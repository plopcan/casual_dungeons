import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ActualizarPersonajeRequest,
  CrearPersonajeRequest,
  GuardarInventarioItemRequest,
  GuardarNotaRequest,
  InventarioItem,
  Nota,
  PersonajeEspacioConjuro,
  PersonajeFicha,
  PersonajeHabilidad,
  PersonajeHechizo,
  PersonajeResumen,
} from '../models';

@Injectable({ providedIn: 'root' })
export class PersonajeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Personajes propios en una campaña. Puede haber más de uno: D&D 5e
   * no obliga a "1 personaje por jugador por campaña", así que el
   * componente que llama a esto debe manejar 0, 1 o varios resultados.
   */
  getMisPersonajes(campaniaId: number): Observable<PersonajeResumen[]> {
    return this.http.get<PersonajeResumen[]>(`${this.apiUrl}/campanias/${campaniaId}/mis-personajes`);
  }

  getFicha(personajeId: number): Observable<PersonajeFicha> {
    return this.http.get<PersonajeFicha>(`${this.apiUrl}/personajes/${personajeId}`);
  }

  getResumenPorCampania(campaniaId: number): Observable<PersonajeResumen[]> {
    return this.http.get<PersonajeResumen[]>(`${this.apiUrl}/campanias/${campaniaId}/personajes`);
  }

  /** Crea un personaje nuevo (nivel 1) en una campaña donde se participa como jugador. */
  crearPersonaje(campaniaId: number, request: CrearPersonajeRequest): Observable<PersonajeFicha> {
    return this.http.post<PersonajeFicha>(`${this.apiUrl}/campanias/${campaniaId}/personajes`, request);
  }

  /** Activa o desactiva uno de los propios personajes. */
  cambiarActivo(personajeId: number, activo: boolean): Observable<{ personajeId: number; activo: boolean }> {
    return this.http.patch<{ personajeId: number; activo: boolean }>(`${this.apiUrl}/personajes/${personajeId}/activo`, {
      activo,
    });
  }

  actualizar(personajeId: number, request: ActualizarPersonajeRequest): Observable<PersonajeFicha> {
    return this.http.put<PersonajeFicha>(`${this.apiUrl}/personajes/${personajeId}`, request);
  }

  // ---------------- Inventario ----------------

  agregarItemInventario(personajeId: number, request: GuardarInventarioItemRequest): Observable<InventarioItem> {
    return this.http.post<InventarioItem>(`${this.apiUrl}/personajes/${personajeId}/inventario`, request);
  }

  eliminarItemInventario(personajeId: number, itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/personajes/${personajeId}/inventario/${itemId}`);
  }

  // ---------------- Notas ----------------

  agregarNota(personajeId: number, request: GuardarNotaRequest): Observable<Nota> {
    return this.http.post<Nota>(`${this.apiUrl}/personajes/${personajeId}/notas`, request);
  }

  actualizarNota(personajeId: number, notaId: number, request: GuardarNotaRequest): Observable<Nota> {
    return this.http.put<Nota>(`${this.apiUrl}/personajes/${personajeId}/notas/${notaId}`, request);
  }

  eliminarNota(personajeId: number, notaId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/personajes/${personajeId}/notas/${notaId}`);
  }

  // ---------------- Habilidades ----------------

  actualizarHabilidad(
    personajeId: number,
    habilidadId: number,
    competente: boolean,
    experticia: boolean,
  ): Observable<PersonajeHabilidad> {
    return this.http.put<PersonajeHabilidad>(`${this.apiUrl}/personajes/${personajeId}/habilidades/${habilidadId}`, {
      competente,
      experticia,
    });
  }

  // ---------------- Hechizos ----------------

  agregarHechizo(personajeId: number, hechizoId: number): Observable<PersonajeHechizo> {
    return this.http.post<PersonajeHechizo>(`${this.apiUrl}/personajes/${personajeId}/hechizos/${hechizoId}`, {});
  }

  quitarHechizo(personajeId: number, personajeHechizoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/personajes/${personajeId}/hechizos/${personajeHechizoId}`);
  }

  togglePreparado(personajeId: number, personajeHechizoId: number): Observable<PersonajeHechizo> {
    return this.http.patch<PersonajeHechizo>(
      `${this.apiUrl}/personajes/${personajeId}/hechizos/${personajeHechizoId}/preparado`,
      {},
    );
  }

  // Espacios de conjuro: el jugador define libremente sus totales
  // por nivel (no se calculan a partir de la clase).

  configurarEspacioConjuro(
    personajeId: number,
    nivelConjuro: number,
    espaciosTotales: number,
  ): Observable<PersonajeEspacioConjuro> {
    return this.http.put<PersonajeEspacioConjuro>(
      `${this.apiUrl}/personajes/${personajeId}/espacios-conjuro/${nivelConjuro}`,
      { espaciosTotales },
    );
  }

  gastarEspacioConjuro(personajeId: number, nivelConjuro: number): Observable<PersonajeEspacioConjuro> {
    return this.http.patch<PersonajeEspacioConjuro>(
      `${this.apiUrl}/personajes/${personajeId}/espacios-conjuro/${nivelConjuro}/gastar`,
      {},
    );
  }

  recuperarEspacioConjuro(personajeId: number, nivelConjuro: number): Observable<PersonajeEspacioConjuro> {
    return this.http.patch<PersonajeEspacioConjuro>(
      `${this.apiUrl}/personajes/${personajeId}/espacios-conjuro/${nivelConjuro}/recuperar`,
      {},
    );
  }

  resetearEspaciosConjuro(personajeId: number): Observable<PersonajeEspacioConjuro[]> {
    return this.http.post<PersonajeEspacioConjuro[]>(
      `${this.apiUrl}/personajes/${personajeId}/espacios-conjuro/resetear`,
      {},
    );
  }
}

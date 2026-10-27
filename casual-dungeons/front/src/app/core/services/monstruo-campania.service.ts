import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CrearMonstruoCampaniaDesdeCatalogoRequest, GuardarMonstruoCampaniaRequest, MonstruoCampania } from '../models';

@Injectable({ providedIn: 'root' })
export class MonstruoCampaniaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getByCampania(campaniaId: number): Observable<MonstruoCampania[]> {
    return this.http.get<MonstruoCampania[]>(`${this.apiUrl}/campanias/${campaniaId}/monstruos-campania`);
  }

  getById(id: number): Observable<MonstruoCampania> {
    return this.http.get<MonstruoCampania>(`${this.apiUrl}/monstruos-campania/${id}`);
  }

  crear(campaniaId: number, request: GuardarMonstruoCampaniaRequest): Observable<MonstruoCampania> {
    return this.http.post<MonstruoCampania>(`${this.apiUrl}/campanias/${campaniaId}/monstruos-campania`, request);
  }

  /** Crea un monstruo de campaña copiando el stat block completo de un monstruo del catálogo precreado. */
  crearDesdeCatalogo(
    campaniaId: number,
    request: CrearMonstruoCampaniaDesdeCatalogoRequest,
  ): Observable<MonstruoCampania> {
    return this.http.post<MonstruoCampania>(
      `${this.apiUrl}/campanias/${campaniaId}/monstruos-campania/desde-catalogo`,
      request,
    );
  }

  editar(id: number, request: GuardarMonstruoCampaniaRequest): Observable<MonstruoCampania> {
    return this.http.put<MonstruoCampania>(`${this.apiUrl}/monstruos-campania/${id}`, request);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/monstruos-campania/${id}`);
  }
}

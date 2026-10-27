import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MonstruoCatalogoFicha, MonstruoCatalogoResumen } from '../models';

@Injectable({ providedIn: 'root' })
export class MonstruoCatalogoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/monstruos-catalogo`;

  /** Listado para el selector. buscar filtra por nombre (contiene, sin distinguir mayúsculas). */
  getTodos(buscar?: string): Observable<MonstruoCatalogoResumen[]> {
    const params = buscar ? { params: { buscar } } : {};
    return this.http.get<MonstruoCatalogoResumen[]>(this.apiUrl, params);
  }

  getFicha(id: number): Observable<MonstruoCatalogoFicha> {
    return this.http.get<MonstruoCatalogoFicha>(`${this.apiUrl}/${id}`);
  }
}

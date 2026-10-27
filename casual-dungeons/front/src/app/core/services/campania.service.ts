import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CampaniaDetalle, CampaniaResumen, CrearCampaniaRequest, EditarCampaniaRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class CampaniaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/campanias`;

  getMisCampanias(activas: boolean): Observable<CampaniaResumen[]> {
    return this.http.get<CampaniaResumen[]>(this.apiUrl, { params: { activas } });
  }

  getDetalle(id: number): Observable<CampaniaDetalle> {
    return this.http.get<CampaniaDetalle>(`${this.apiUrl}/${id}`);
  }

  crear(request: CrearCampaniaRequest): Observable<CampaniaDetalle> {
    return this.http.post<CampaniaDetalle>(this.apiUrl, request);
  }

  editar(id: number, request: EditarCampaniaRequest): Observable<CampaniaDetalle> {
    return this.http.put<CampaniaDetalle>(`${this.apiUrl}/${id}`, request);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

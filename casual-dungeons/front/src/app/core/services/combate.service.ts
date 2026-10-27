import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AgregarParticipanteCombateRequest,
  Combate,
  CrearCombateRequest,
  ReordenarParticipantesRequest,
} from '../models';

@Injectable({ providedIn: 'root' })
export class CombateService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getByCampania(campaniaId: number): Observable<Combate[]> {
    return this.http.get<Combate[]>(`${this.apiUrl}/campanias/${campaniaId}/combates`);
  }

  getById(id: number): Observable<Combate> {
    return this.http.get<Combate>(`${this.apiUrl}/combates/${id}`);
  }

  crear(request: CrearCombateRequest): Observable<Combate> {
    return this.http.post<Combate>(`${this.apiUrl}/combates`, request);
  }

  agregarParticipante(combateId: number, request: AgregarParticipanteCombateRequest): Observable<Combate> {
    return this.http.post<Combate>(`${this.apiUrl}/combates/${combateId}/participantes`, request);
  }

  eliminarParticipante(combateId: number, participanteId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/combates/${combateId}/participantes/${participanteId}`);
  }

  reordenar(combateId: number, request: ReordenarParticipantesRequest): Observable<Combate> {
    return this.http.put<Combate>(`${this.apiUrl}/combates/${combateId}/orden`, request);
  }

  finalizar(combateId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/combates/${combateId}/finalizar`, {});
  }

  eliminar(combateId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/combates/${combateId}`);
  }
}

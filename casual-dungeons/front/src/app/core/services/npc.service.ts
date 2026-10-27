import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GuardarNPCRequest, NPC, CrearNpcDesdeCatalogoRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class NPCService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getByCampania(campaniaId: number): Observable<NPC[]> {
    return this.http.get<NPC[]>(`${this.apiUrl}/campanias/${campaniaId}/npcs`);
  }

  getById(id: number): Observable<NPC> {
    return this.http.get<NPC>(`${this.apiUrl}/npcs/${id}`);
  }

  crear(campaniaId: number, request: GuardarNPCRequest): Observable<NPC> {
    return this.http.post<NPC>(`${this.apiUrl}/campanias/${campaniaId}/npcs`, request);
  }

  /** Crea un NPC copiando el stat block de un monstruo del catálogo precreado. */
  crearDesdeCatalogo(campaniaId: number, request: CrearNpcDesdeCatalogoRequest): Observable<NPC> {
    return this.http.post<NPC>(`${this.apiUrl}/campanias/${campaniaId}/npcs/desde-catalogo`, request);
  }

  editar(id: number, request: GuardarNPCRequest): Observable<NPC> {
    return this.http.put<NPC>(`${this.apiUrl}/npcs/${id}`, request);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/npcs/${id}`);
  }
}

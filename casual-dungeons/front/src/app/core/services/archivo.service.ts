import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArchivoSubido } from '../models';

@Injectable({ providedIn: 'root' })
export class ArchivoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/archivos`;

  // Origen del backend sin el sufijo "/api" (ej. "https://localhost:5001"),
  // usado para resolver las URLs relativas que devuelve la subida
  // de archivos (ej. "/uploads/imagenes/xxx.png").
  private readonly origenBackend = environment.apiUrl.replace(/\/api\/?$/, '');

  subirImagen(archivo: File): Observable<ArchivoSubido> {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return this.http.post<ArchivoSubido>(`${this.apiUrl}/imagenes`, formData);
  }

  /**
   * Resuelve una URL de imagen guardada contra el origen correcto:
   * - Si es relativa (empieza por "/"), la resuelve contra el origen
   *   del backend, ya que si no el navegador la buscaría contra el
   *   origen del frontend Angular y fallaría con 404.
   * - Si ya es una URL absoluta (http:// o https://), guardada antes
   *   de que existiera la subida de archivos, se devuelve tal cual
   *   para no romper campañas con imágenes externas ya configuradas.
   */
  urlAbsoluta(urlRelativa: string | null): string | null {
    if (!urlRelativa) return null;

    if (/^https?:\/\//i.test(urlRelativa)) {
      return urlRelativa;
    }

    return `${this.origenBackend}${urlRelativa}`;
  }
}

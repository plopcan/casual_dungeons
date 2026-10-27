import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { HechizoCatalogo } from '../models';

@Injectable({ providedIn: 'root' })
export class HechizoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/hechizos`;

  getAll(): Observable<HechizoCatalogo[]> {
    return this.http.get<HechizoCatalogo[]>(this.apiUrl);
  }
}

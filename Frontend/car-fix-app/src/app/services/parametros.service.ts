import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ParametroDto } from '../models/parametro.model';

export interface ParametroRequest {
  nombre: string;
  valor:  string;
}

@Injectable({ providedIn: 'root' })
export class ParametrosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/parametros`;

  obtener(): Observable<ParametroDto[]> {
    return this.http.get<ParametroDto[]>(this.base);
  }

  crear(req: ParametroRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ParametroRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

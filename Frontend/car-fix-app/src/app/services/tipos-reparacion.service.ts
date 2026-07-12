import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TipoReparacionDto } from '../models/tipo-reparacion.model';

export interface CrearTipoReparacionRequest {
  descripcionReparacion:   string;
  duracionAproximadaHoras: number;
  costoBase:               number;
}

export interface ActualizarTipoReparacionRequest {
  descripcionReparacion:   string;
  duracionAproximadaHoras: number;
  costoBase:               number;
}

@Injectable({ providedIn: 'root' })
export class TiposReparacionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tipos-reparacion`;

  obtener(filtro?: string): Observable<TipoReparacionDto[]> {
    const params: Record<string, string> = {};
    if (filtro) params['filtro'] = filtro;
    return this.http.get<TipoReparacionDto[]>(this.base, { params });
  }

  crear(req: CrearTipoReparacionRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ActualizarTipoReparacionRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

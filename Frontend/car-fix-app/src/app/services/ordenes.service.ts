import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OrdenServicioDto, CrearOrdenResponseDto } from '../models/orden-servicio.model';

export interface OrdenRequest {
  vehiculoId:      number;
  fechaIngreso:    string;
  fechaSalida:     string;
  problemaGeneral: string;
  esGarantia:      boolean;
  estadoOrdenId?:  number;
}

@Injectable({ providedIn: 'root' })
export class OrdenesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/ordenes`;

  obtener(filtro?: string): Observable<OrdenServicioDto[]> {
    const params: Record<string, string> = {};
    if (filtro) params['filtro'] = filtro;
    return this.http.get<OrdenServicioDto[]>(this.base, { params });
  }

  crear(req: OrdenRequest): Observable<CrearOrdenResponseDto> {
    return this.http.post<CrearOrdenResponseDto>(this.base, req);
  }

  actualizar(id: number, req: OrdenRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  cambiarEstado(id: number, nuevoEstadoId: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/estado`, { nuevoEstadoId });
  }
}

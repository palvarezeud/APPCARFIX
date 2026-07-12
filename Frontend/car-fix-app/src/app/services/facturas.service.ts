import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FacturaDto, CrearFacturaRequest, ActualizarFacturaRequest } from '../models/factura.model';

@Injectable({ providedIn: 'root' })
export class FacturasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/facturas`;

  obtener(filtro?: string): Observable<FacturaDto[]> {
    const params: Record<string, string> = {};
    if (filtro) params['filtro'] = filtro;
    return this.http.get<FacturaDto[]>(this.base, { params });
  }

  crear(req: CrearFacturaRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ActualizarFacturaRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  cambiarEstado(id: number, nuevoEstadoId: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/estado`, { nuevoEstadoId });
  }

  enviar(id: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/enviar`, {});
  }

  obtenerPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/pdf`, { responseType: 'blob' });
  }
}

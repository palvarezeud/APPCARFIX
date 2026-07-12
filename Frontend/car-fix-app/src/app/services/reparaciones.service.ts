import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReparacionDto, AgregarReparacionRequest, ActualizarReparacionRequest } from '../models/reparacion.model';

@Injectable({ providedIn: 'root' })
export class ReparacionesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reparaciones`;

  obtenerPorFactura(facturaId: number): Observable<ReparacionDto[]> {
    return this.http.get<ReparacionDto[]>(this.base, { params: { facturaId } });
  }

  agregar(req: AgregarReparacionRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ActualizarReparacionRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  marcarListo(id: number, listo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/listo`, { listo });
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

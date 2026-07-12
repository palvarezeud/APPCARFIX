import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RepuestoDto, AgregarRepuestoRequest, ActualizarRepuestoRequest } from '../models/repuesto.model';

@Injectable({ providedIn: 'root' })
export class RepuestosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/repuestos`;

  obtenerPorFactura(facturaId: number): Observable<RepuestoDto[]> {
    return this.http.get<RepuestoDto[]>(this.base, { params: { facturaId } });
  }

  agregar(req: AgregarRepuestoRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ActualizarRepuestoRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  marcarIncluido(id: number, incluido: boolean): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/incluido`, { incluido });
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

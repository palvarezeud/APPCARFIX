import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HistoricoRespuestoDto } from '../models/historico-repuesto.model';

export interface HistoricoRepuestoRequest {
  marca:               string;
  modelo:              string;
  annio:               number;
  repuestoDecripcion:  string;
  precio:              number;
  repuestera:          string;
  fechaCompra:         string;
}

@Injectable({ providedIn: 'root' })
export class HistoricoRepuestosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/historico-repuestos`;

  obtener(marca?: string, modelo?: string): Observable<HistoricoRespuestoDto[]> {
    const params: Record<string, string> = {};
    if (marca)  params['marca']  = marca;
    if (modelo) params['modelo'] = modelo;
    return this.http.get<HistoricoRespuestoDto[]>(this.base, { params });
  }

  crear(req: HistoricoRepuestoRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: HistoricoRepuestoRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

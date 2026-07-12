import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { VehiculoDto } from '../models/vehiculo.model';
import { DatosVehiculoExtraidosDto } from '../models/datos-vehiculo-extraidos.model';

export interface VehiculoRequest {
  clienteId:          number;
  placa:              string | null;
  marca:              string;
  modelo:             string;
  vin:                string | null;
  annio:              number;
  motor:              string | null;
  esAutomatico:       boolean;
  detallesCarroceria: string;
}

@Injectable({ providedIn: 'root' })
export class VehiculosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/vehiculos`;

  obtener(filtro?: string): Observable<VehiculoDto[]> {
    const params: Record<string, string> = {};
    if (filtro) params['filtro'] = filtro;
    return this.http.get<VehiculoDto[]>(this.base, { params });
  }

  crear(req: VehiculoRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: VehiculoRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  escanearTarjetaCirculacion(foto: File): Observable<DatosVehiculoExtraidosDto> {
    const formData = new FormData();
    formData.append('foto', foto);
    return this.http.post<DatosVehiculoExtraidosDto>(
      `${this.base}/escanear-tarjeta-circulacion`, formData);
  }
}

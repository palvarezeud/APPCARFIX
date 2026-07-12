import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ClienteDto, CrearClienteRequest } from '../models/cliente.model';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clientes`;

  obtener(filtro?: string): Observable<ClienteDto[]> {
    const params: Record<string, string> = {};
    if (filtro) params['filtro'] = filtro;
    return this.http.get<ClienteDto[]>(this.base, { params });
  }

  crear(req: CrearClienteRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: CrearClienteRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

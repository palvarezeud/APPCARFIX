import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  UsuarioDto,
  CrearUsuarioRequest,
  ActualizarUsuarioRequest,
  CambiarContrasennaRequest
} from '../models/usuario.model';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/usuarios`;

  obtener(): Observable<UsuarioDto[]> {
    return this.http.get<UsuarioDto[]>(this.base);
  }

  crear(req: CrearUsuarioRequest): Observable<number> {
    return this.http.post<number>(this.base, req);
  }

  actualizar(id: number, req: ActualizarUsuarioRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  cambiarContrasenna(id: number, req: CambiarContrasennaRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/contrasenna`, req);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

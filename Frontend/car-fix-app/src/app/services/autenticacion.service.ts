import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TokenResponse } from '../models/token-response.model';

@Injectable({ providedIn: 'root' })
export class AutenticacionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/autenticacion`;

  iniciarSesion(nombreUsuario: string, password: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.base}/iniciar-sesion`, { nombreUsuario, password });
  }

  refrescarSesion(tokenRefresco: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.base}/refrescar`, { tokenRefresco });
  }

  cerrarSesion(tokenRefresco: string): Observable<void> {
    return this.http.post<void>(`${this.base}/cerrar-sesion`, { tokenRefresco });
  }
}

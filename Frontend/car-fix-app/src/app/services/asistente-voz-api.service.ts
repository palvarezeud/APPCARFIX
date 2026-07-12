import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { InterpretacionVozDto } from '../models/interpretacion-voz.model';

@Injectable({ providedIn: 'root' })
export class AsistenteVozApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/asistente-voz`;

  interpretar(
    transcripcion: string,
    pantallaActual: string,
    intentEnProgreso: string | null = null
  ): Observable<InterpretacionVozDto> {
    return this.http.post<InterpretacionVozDto>(
      `${this.base}/interpretar`, { transcripcion, pantallaActual, intentEnProgreso });
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TallerDto } from '../models/taller.model';

export interface ActualizarTallerRequest {
  nombre:               string;
  ubicacionDescripcion: string;
  telefonos:            string;
  email:                string;
  latitud:              number | null;
  longitud:             number | null;
}

@Injectable({ providedIn: 'root' })
export class TallerService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/taller`;

  obtener(): Observable<TallerDto> {
    return this.http.get<TallerDto>(this.base);
  }

  actualizar(req: ActualizarTallerRequest): Observable<void> {
    return this.http.put<void>(this.base, req);
  }
}

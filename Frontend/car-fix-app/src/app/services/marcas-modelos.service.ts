import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MarcaModeloDto } from '../models/marca-modelo.model';

@Injectable({ providedIn: 'root' })
export class MarcasModelosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/marcas-modelos`;

  obtener(): Observable<MarcaModeloDto[]> {
    return this.http.get<MarcaModeloDto[]>(this.base);
  }
}

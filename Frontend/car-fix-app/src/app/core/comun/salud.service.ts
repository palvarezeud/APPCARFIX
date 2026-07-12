import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SaludService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/salud`;

  activar(): Observable<boolean> {
    return this.http.get<boolean>(this.base);
  }
}

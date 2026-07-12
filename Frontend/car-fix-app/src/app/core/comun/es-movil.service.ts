import { Injectable, signal } from '@angular/core';

const CONSULTA_MOVIL = '(max-width: 768px)';

@Injectable({ providedIn: 'root' })
export class EsMovilService {
  private readonly mediaMovil = window.matchMedia(CONSULTA_MOVIL);

  private readonly _esMovil = signal(this.mediaMovil.matches);
  readonly esMovil = this._esMovil.asReadonly();

  constructor() {
    this.mediaMovil.addEventListener('change', e => this._esMovil.set(e.matches));
  }
}

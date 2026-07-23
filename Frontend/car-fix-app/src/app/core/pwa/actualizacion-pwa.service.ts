import { Injectable, inject, signal } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';

@Injectable({ providedIn: 'root' })
export class ActualizacionPwaService {
  private readonly swUpdate = inject(SwUpdate);

  private readonly _actualizacionDisponible = signal(false);
  readonly actualizacionDisponible = this._actualizacionDisponible.asReadonly();

  constructor() {
    if (!this.swUpdate.isEnabled) return;

    this.swUpdate.versionUpdates.subscribe(evento => {
      if (evento.type === 'VERSION_READY') {
        this._actualizacionDisponible.set(true);
      }
    });
  }

  actualizar(): void {
    window.location.reload();
  }
}

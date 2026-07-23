import { Component, inject } from '@angular/core';
import { ActualizacionPwaService } from './actualizacion-pwa.service';

@Component({
  standalone: true,
  selector: 'app-actualizacion-pwa',
  template: `
    @if (svc.actualizacionDisponible()) {
      <div class="install-banner">
        <div class="install-banner-texto">
          <strong>CAR FIX</strong>
          <span>Hay una nueva version disponible</span>
        </div>
        <div class="install-banner-acciones">
          <button class="install-btn-instalar" (click)="svc.actualizar()">Actualizar</button>
        </div>
      </div>
    }
  `
})
export class ActualizacionPwaComponent {
  protected readonly svc = inject(ActualizacionPwaService);
}

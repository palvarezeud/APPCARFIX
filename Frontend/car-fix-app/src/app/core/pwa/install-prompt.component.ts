import { Component, effect, inject, signal } from '@angular/core';
import { InstalacionPwaService } from './instalacion-pwa.service';

const CLAVE_CERRADO = 'install-prompt-cerrado';

@Component({
  standalone: true,
  selector: 'app-install-prompt',
  template: `
    @if (mostrarBanner()) {
      <div class="install-banner">
        <div class="install-banner-texto">
          <strong>CAR FIX</strong>
          <span>Instalar como app en tu dispositivo</span>
        </div>
        <div class="install-banner-acciones">
          <button class="install-btn-instalar" (click)="instalar()">Instalar</button>
          <button class="install-btn-cerrar" (click)="cerrar()" aria-label="Cerrar">✕</button>
        </div>
      </div>
    }
    @if (pwaService.mostrarInstruccionesIos()) {
      <div class="install-banner">
        <div class="install-banner-texto">
          <strong>CAR FIX</strong>
          <span>Toca el icono Compartir en Safari y selecciona "Agregar a pantalla de inicio"</span>
        </div>
        <div class="install-banner-acciones">
          <button class="install-btn-cerrar" (click)="pwaService.cerrarInstruccionesIos()" aria-label="Cerrar">✕</button>
        </div>
      </div>
    }
  `
})
export class InstallPromptComponent {
  protected readonly pwaService = inject(InstalacionPwaService);

  private readonly cerradoEnSesion = signal(!!sessionStorage.getItem(CLAVE_CERRADO));

  mostrarBanner = signal(false);

  constructor() {
    effect(() => {
      this.mostrarBanner.set(this.pwaService.puedeInstalarAndroid() && !this.cerradoEnSesion());
    });
  }

  instalar() {
    this.pwaService.instalarAndroid();
    this.mostrarBanner.set(false);
  }

  cerrar() {
    sessionStorage.setItem(CLAVE_CERRADO, '1');
    this.cerradoEnSesion.set(true);
  }
}

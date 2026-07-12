import { Component, OnInit, signal } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-install-prompt',
  template: `
    @if (mostrar()) {
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
  `
})
export class InstallPromptComponent implements OnInit {
  mostrar = signal(false);
  private eventoInstalacion: any = null;

  ngOnInit() {
    window.addEventListener('beforeinstallprompt', (e: Event) => {
      e.preventDefault();
      this.eventoInstalacion = e;
      if (!sessionStorage.getItem('install-prompt-cerrado')) {
        this.mostrar.set(true);
      }
    });
  }

  instalar() {
    this.eventoInstalacion?.prompt();
    this.eventoInstalacion?.userChoice.then(() => {
      this.eventoInstalacion = null;
      this.mostrar.set(false);
    });
  }

  cerrar() {
    sessionStorage.setItem('install-prompt-cerrado', '1');
    this.mostrar.set(false);
  }
}

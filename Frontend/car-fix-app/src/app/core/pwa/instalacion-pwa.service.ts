import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class InstalacionPwaService {
  private eventoInstalacion: any = null;

  private readonly _puedeInstalarAndroid = signal(false);
  readonly puedeInstalarAndroid = this._puedeInstalarAndroid.asReadonly();

  private readonly _esIOS = signal(this.detectarIOS());
  readonly esIOS = this._esIOS.asReadonly();

  private readonly _yaInstalado = signal(this.detectarInstalado());
  readonly yaInstalado = this._yaInstalado.asReadonly();

  private readonly _mostrarInstruccionesIos = signal(false);
  readonly mostrarInstruccionesIos = this._mostrarInstruccionesIos.asReadonly();

  readonly mostrarOpcionInstalar = computed(() =>
    !this._yaInstalado() && (this._puedeInstalarAndroid() || this._esIOS())
  );

  constructor() {
    window.addEventListener('beforeinstallprompt', (e: Event) => {
      e.preventDefault();
      this.eventoInstalacion = e;
      this._puedeInstalarAndroid.set(true);
    });

    window.addEventListener('appinstalled', () => {
      this._yaInstalado.set(true);
      this._puedeInstalarAndroid.set(false);
    });
  }

  manejarClicMenu(): void {
    if (this._esIOS()) {
      this._mostrarInstruccionesIos.set(true);
    } else {
      this.instalarAndroid();
    }
  }

  instalarAndroid(): void {
    this.eventoInstalacion?.prompt();
    this.eventoInstalacion?.userChoice.then((resultado: { outcome: string }) => {
      if (resultado.outcome === 'accepted') {
        this._yaInstalado.set(true);
      }
      this.eventoInstalacion = null;
      this._puedeInstalarAndroid.set(false);
    });
  }

  cerrarInstruccionesIos(): void {
    this._mostrarInstruccionesIos.set(false);
  }

  private detectarIOS(): boolean {
    return /iPad|iPhone|iPod/.test(navigator.userAgent)
      || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
  }

  private detectarInstalado(): boolean {
    return window.matchMedia('(display-mode: standalone)').matches
      || (navigator as any).standalone === true;
  }
}

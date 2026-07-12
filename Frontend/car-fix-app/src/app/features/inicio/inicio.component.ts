import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { EsMovilService } from '../../core/comun/es-movil.service';

@Component({
  selector: 'app-inicio',
  standalone: true,
  template: ``
})
export class InicioComponent {
  constructor() {
    const esMovil = inject(EsMovilService).esMovil();
    inject(Router).navigateByUrl(esMovil ? '/chat' : '/ordenes', { replaceUrl: true });
  }
}

import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-sin-acceso',
  imports: [RouterLink],
  template: `
    <div class="sin-acceso">
      <h2>Acceso denegado</h2>
      <p>No tiene permiso para acceder a esta seccion.</p>
      <a routerLink="/ordenes" class="btn btn-primario">Volver al inicio</a>
    </div>
  `
})
export class SinAccesoComponent {}

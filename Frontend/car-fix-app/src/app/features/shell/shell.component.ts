import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from '../nav/nav.component';
import { InstallPromptComponent } from '../../core/pwa/install-prompt.component';
import { ActualizacionPwaComponent } from '../../core/pwa/actualizacion-pwa.component';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [RouterOutlet, NavComponent, InstallPromptComponent, ActualizacionPwaComponent],
  template: `
    <div class="layout">
      <app-nav />
      <main class="contenido-principal">
        <router-outlet />
      </main>
    </div>
    <app-install-prompt />
    <app-actualizacion-pwa />
  `
})
export class ShellComponent {}

import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from '../nav/nav.component';
import { InstallPromptComponent } from '../../core/pwa/install-prompt.component';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [RouterOutlet, NavComponent, InstallPromptComponent],
  template: `
    <div class="layout">
      <app-nav />
      <main class="contenido-principal">
        <router-outlet />
      </main>
    </div>
    <app-install-prompt />
  `
})
export class ShellComponent {}

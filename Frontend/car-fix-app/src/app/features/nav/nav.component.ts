import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { EsMovilService } from '../../core/comun/es-movil.service';

@Component({
  standalone: true,
  selector: 'app-nav',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <!-- Top bar (solo visible en mobile) -->
    <header class="topbar">
      <button class="hamburger" (click)="menuAbierto.set(!menuAbierto())" aria-label="Abrir menu">
        <span [class.activo]="menuAbierto()"></span>
        <span [class.activo]="menuAbierto()"></span>
        <span [class.activo]="menuAbierto()"></span>
      </button>
      <div class="topbar-logo">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="logo-icono">
          <path d="M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z"/>
        </svg>
        <span>CAR FIX</span>
      </div>
    </header>

    <!-- Overlay oscuro (mobile, cuando el menu esta abierto) -->
    @if (menuAbierto()) {
      <div class="sidebar-overlay" (click)="menuAbierto.set(false)"></div>
    }

    <!-- Sidebar -->
    <aside class="sidebar" [class.abierto]="menuAbierto()">
      <div class="sidebar-logo">
        <div class="sidebar-logo-fila">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="logo-icono">
            <path d="M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z"/>
          </svg>
          <h1>CAR FIX</h1>
        </div>
        <span>Sistema de Taller</span>
      </div>

      <ul class="nav-lista">
        @if (esMovilSvc.esMovil()) {
          <li><a routerLink="/chat" routerLinkActive="activo" (click)="cerrar()">🏠 Inicio</a></li>
        }
        <li><a routerLink="/ordenes"   routerLinkActive="activo" (click)="cerrar()">📋 Ordenes de servicio</a></li>
        <li><a routerLink="/facturas"  routerLinkActive="activo" (click)="cerrar()">🧾 Facturas</a></li>
        <li><a routerLink="/clientes"  routerLinkActive="activo" (click)="cerrar()">👤 Clientes</a></li>
        <li><a routerLink="/vehiculos" routerLinkActive="activo" (click)="cerrar()">🚗 Vehiculos</a></li>

        @if (auth.esJefe()) {
          <li><div class="nav-separador"></div></li>
          <li><a routerLink="/catalogo-reparaciones" routerLinkActive="activo" (click)="cerrar()">🔧 Catalogo reparaciones</a></li>
          <li><a routerLink="/catalogo-repuestos"    routerLinkActive="activo" (click)="cerrar()">⚙️ Catalogo repuestos</a></li>
          <li><a routerLink="/usuarios"              routerLinkActive="activo" (click)="cerrar()">👥 Usuarios y perfiles</a></li>
        }

        @if (auth.esAdmin()) {
          <li><div class="nav-separador"></div></li>
          <li><a routerLink="/configuracion" routerLinkActive="activo" (click)="cerrar()">⚙️ Configuracion</a></li>
        }
      </ul>

      <div class="sidebar-footer">
        <div class="sidebar-usuario">{{ auth.nombreUsuario() }} ({{ auth.rol() }})</div>
        <button class="btn btn-outline" style="width:100%;color:#dde1e7;border-color:rgba(255,255,255,.2)"
                (click)="auth.cerrarSesion()">Cerrar sesion</button>
      </div>
    </aside>
  `
})
export class NavComponent {
  protected readonly auth = inject(AuthService);
  protected readonly esMovilSvc = inject(EsMovilService);
  menuAbierto = signal(false);

  cerrar() { this.menuAbierto.set(false); }
}

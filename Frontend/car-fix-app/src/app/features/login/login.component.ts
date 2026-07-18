import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service';
import { EsMovilService } from '../../core/comun/es-movil.service';
import { environment } from '../../../environments/environment';

interface TokenResponse { token: string; expiracion: string; }

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <div class="login-pagina">
      <div class="login-tarjeta">
        <div class="login-logo">
          <h1>CAR FIX</h1>
          <span>Sistema de Administracion de Taller</span>
        </div>

        @if (error()) {
          <div class="alerta alerta-error">{{ error() }}</div>
        }

        <div class="form-grupo">
          <label for="usuario">Usuario</label>
          <input id="usuario" type="text" [(ngModel)]="nombreUsuario"
                 placeholder="Ingrese su usuario" autocomplete="username"
                 (keyup.enter)="iniciarSesion()">
        </div>

        <div class="form-grupo">
          <label for="clave">Contraseña</label>
          <input id="clave" type="password" [(ngModel)]="password"
                 placeholder="Ingrese su contraseña" autocomplete="current-password"
                 (keyup.enter)="iniciarSesion()">
        </div>

        <button class="btn-login" [disabled]="cargando()" (click)="iniciarSesion()">
          @if (cargando()) { <span class="spinner"></span> Verificando... }
          @else             { Ingresar }
        </button>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly http     = inject(HttpClient);
  private readonly auth     = inject(AuthService);
  private readonly router   = inject(Router);
  private readonly esMovil  = inject(EsMovilService);

  nombreUsuario = '';
  password      = '';
  cargando      = signal(false);
  error         = signal<string | null>(null);

  iniciarSesion(): void {
    if (!this.nombreUsuario || !this.password) {
      this.error.set('Ingrese usuario y contraseña.');
      return;
    }
    this.error.set(null);
    this.cargando.set(true);

    this.http.post<TokenResponse>(
      `${environment.apiUrl}/autenticacion/iniciar-sesion`,
      { nombreUsuario: this.nombreUsuario, password: this.password }
    ).subscribe({
      next: res => {
        this.auth.guardarSesion(res.token);
        this.router.navigateByUrl(this.esMovil.esMovil() ? '/chat' : '/ordenes');
      },
      error: err => {
        this.cargando.set(false);
        this.error.set(
          err.status === 401 ? 'Credenciales invalidas.' : 'Error al conectar con el servidor.'
        );
      }
    });
  }
}

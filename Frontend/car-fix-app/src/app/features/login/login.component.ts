import { Component, computed, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { BiometriaService } from '../../core/auth/biometria.service';
import { EsMovilService } from '../../core/comun/es-movil.service';
import { AutenticacionService } from '../../services/autenticacion.service';

const CLAVE_PROMPT_CERRADO = 'biometria-prompt-cerrado';

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

        @if (mostrandoPromptActivar()) {
          <div class="alerta alerta-exito">
            <p>¿Activar ingreso con huella/rostro en este dispositivo?</p>
            <div class="form-acciones">
              <button class="btn-secundario" (click)="declinarBiometria()">Ahora no</button>
              <button class="btn-accion" (click)="activarBiometria()">Activar</button>
            </div>
          </div>
        } @else {

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

          <div class="login-acciones">
            <button class="btn-login" [disabled]="cargando()" (click)="iniciarSesion()">
              @if (cargando()) { <span class="spinner"></span> Verificando... }
              @else             { Ingresar }
            </button>
            @if (mostrarBotonBiometrico()) {
              <button type="button" class="btn-biometria" [disabled]="cargando()"
                      (click)="ingresarConBiometria()"
                      title="Ingresar con huella/rostro"
                      aria-label="Ingresar con huella o reconocimiento facial">
                <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor"
                     stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M12 11c0 3.5-.5 6.5-2 9" />
                  <path d="M8.5 20c1-2 1.5-4.5 1.5-7 0-1.1.9-2 2-2s2 .9 2 2c0 .5 0 1-.1 1.5" />
                  <path d="M5 20c1.5-2.5 2.5-5.5 2.5-9a4.5 4.5 0 0 1 9 0v1" />
                  <path d="M4 15.5c.7-1.5 1-3.3 1-4.5a7 7 0 0 1 13.9-1.2" />
                  <path d="M17.5 14.5c.3-1 .5-2.2.5-3.5" />
                  <path d="M2.5 11a9.5 9.5 0 0 1 15.3-7.5" />
                </svg>
              </button>
            }
          </div>
        }
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly autenticacionSvc = inject(AutenticacionService);
  private readonly auth      = inject(AuthService);
  private readonly router    = inject(Router);
  private readonly esMovil   = inject(EsMovilService);
  protected readonly biometria = inject(BiometriaService);

  nombreUsuario = '';
  password      = '';
  cargando      = signal(false);
  error         = signal<string | null>(null);

  mostrandoPromptActivar  = signal(false);
  private tokenRefrescoPendiente: string | null = null;

  mostrarBotonBiometrico = computed(() => this.biometria.habilitado() && this.biometria.soportado());

  iniciarSesion(): void {
    if (!this.nombreUsuario || !this.password) {
      this.error.set('Ingrese usuario y contraseña.');
      return;
    }
    this.error.set(null);
    this.cargando.set(true);

    this.autenticacionSvc.iniciarSesion(this.nombreUsuario, this.password).subscribe({
      next: res => {
        this.auth.guardarSesion(res.token);
        this.cargando.set(false);

        const debeOfrecerBiometria =
          this.esMovil.esMovil() &&
          this.biometria.soportado() &&
          !this.biometria.habilitado() &&
          !localStorage.getItem(CLAVE_PROMPT_CERRADO);

        if (debeOfrecerBiometria) {
          this.tokenRefrescoPendiente = res.tokenRefresco;
          this.mostrandoPromptActivar.set(true);
          return;
        }

        this.navegarLuegoDeIngresar();
      },
      error: err => {
        this.cargando.set(false);
        this.error.set(
          err.status === 401 ? 'Credenciales invalidas.' : 'Error al conectar con el servidor.'
        );
      }
    });
  }

  ingresarConBiometria(): void {
    this.error.set(null);
    this.cargando.set(true);

    this.biometria.desbloquear().then(res => {
      this.cargando.set(false);
      if (!res) {
        this.error.set('No se pudo verificar la huella/rostro. Ingresa con tu contraseña.');
        return;
      }
      this.auth.guardarSesion(res.token);
      this.navegarLuegoDeIngresar();
    });
  }

  activarBiometria(): void {
    const token = this.tokenRefrescoPendiente;
    this.mostrandoPromptActivar.set(false);
    if (token) {
      this.biometria.registrar(this.nombreUsuario, token).finally(() => this.navegarLuegoDeIngresar());
    } else {
      this.navegarLuegoDeIngresar();
    }
  }

  declinarBiometria(): void {
    localStorage.setItem(CLAVE_PROMPT_CERRADO, '1');
    this.mostrandoPromptActivar.set(false);
    this.navegarLuegoDeIngresar();
  }

  private navegarLuegoDeIngresar(): void {
    this.router.navigateByUrl(this.esMovil.esMovil() ? '/chat' : '/ordenes');
  }
}

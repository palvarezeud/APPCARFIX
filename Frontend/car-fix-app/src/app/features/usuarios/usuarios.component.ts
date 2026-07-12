import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UsuariosService } from '../../services/usuarios.service';
import { UsuarioDto, ROLES } from '../../models/usuario.model';

type ModoFormulario = 'crear' | 'editar' | 'password' | null;

@Component({
  standalone: true,
  selector: 'app-usuarios',
  imports: [FormsModule],
  template: `
    <div class="pagina-header">
      <h2>Usuarios y Perfiles</h2>
      <div class="acciones">
        <button class="btn btn-primario"   (click)="abrirFormulario('crear')">+ Agregar</button>
        <button class="btn btn-primario" [disabled]="!seleccionado()" (click)="abrirFormulario('editar')">Modificar</button>
        <button class="btn btn-outline"    [disabled]="!seleccionado()" (click)="abrirFormulario('password')">Cambiar contraseña</button>
        <button class="btn btn-peligro"    [disabled]="!seleccionado()" (click)="confirmarEliminar()">Eliminar</button>
      </div>
    </div>

    @if (error())  { <div class="alerta alerta-error">{{ error() }}</div> }
    @if (exito())  { <div class="alerta alerta-exito">{{ exito() }}</div> }

    @if (cargando()) {
      <div class="cargando"><span class="spinner"></span> Cargando...</div>
    } @else {
      <div class="tabla-contenedor">
        <table>
          <thead>
            <tr><th>#</th><th>Usuario</th><th>Nombre completo</th><th>Email</th><th>Rol</th><th>Estado</th></tr>
          </thead>
          <tbody>
            @for (u of usuarios(); track u.usuarioId) {
              <tr [class.seleccionada]="seleccionado()?.usuarioId === u.usuarioId" (click)="seleccionar(u)">
                <td>{{ u.usuarioId }}</td>
                <td><strong>{{ u.nombreUsuario }}</strong></td>
                <td>{{ u.nombreCompleto }}</td>
                <td>{{ u.email ?? '—' }}</td>
                <td><span class="badge">{{ u.nombreRol }}</span></td>
                <td>
                  @if (u.activo) {
                    <span class="badge badge-2">Activo</span>
                  } @else {
                    <span class="badge badge-5">Inactivo</span>
                  }
                </td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="celda-vacia">No hay usuarios registrados.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    <!-- Formulario crear -->
    @if (modoFormulario() === 'crear') {
      <div class="formulario-panel">
        <h3>Nuevo usuario</h3>
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid">
          <div class="form-grupo">
            <label class="campo-requerido">Nombre de usuario</label>
            <input type="text" [(ngModel)]="formCrear.nombreUsuario" placeholder="Ej: jperez">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Contraseña</label>
            <input type="password" [(ngModel)]="formCrear.password" placeholder="Minimo 6 caracteres">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Nombre completo</label>
            <input type="text" [(ngModel)]="formCrear.nombreCompleto" placeholder="Ej: Juan Perez">
          </div>
          <div class="form-grupo">
            <label>Email</label>
            <input type="email" [(ngModel)]="formCrear.email" placeholder="correo@ejemplo.com">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Rol</label>
            <select [(ngModel)]="formCrear.rolId">
              @for (r of roles; track r.rolId) {
                <option [value]="r.rolId">{{ r.nombre }}</option>
              }
            </select>
          </div>
          <div class="checkbox-grupo">
            <input type="checkbox" id="activoCrear" [(ngModel)]="formCrear.activo">
            <label for="activoCrear">Usuario activo</label>
          </div>
        </div>

        <div class="form-acciones">
          <button class="btn btn-secundario" (click)="cancelar()">Cancelar</button>
          <button class="btn btn-primario" [disabled]="guardando()" (click)="guardarNuevo()">
            @if (guardando()) { Guardando... } @else { Aceptar }
          </button>
        </div>
      </div>
    }

    <!-- Formulario editar -->
    @if (modoFormulario() === 'editar') {
      <div class="formulario-panel">
        <h3>Modificar usuario — {{ seleccionado()!.nombreUsuario }}</h3>
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid">
          <div class="form-grupo">
            <label class="campo-requerido">Nombre completo</label>
            <input type="text" [(ngModel)]="formEditar.nombreCompleto">
          </div>
          <div class="form-grupo">
            <label>Email</label>
            <input type="email" [(ngModel)]="formEditar.email" placeholder="correo@ejemplo.com">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Rol</label>
            <select [(ngModel)]="formEditar.rolId">
              @for (r of roles; track r.rolId) {
                <option [value]="r.rolId">{{ r.nombre }}</option>
              }
            </select>
          </div>
          <div class="checkbox-grupo">
            <input type="checkbox" id="activoEditar" [(ngModel)]="formEditar.activo">
            <label for="activoEditar">Usuario activo</label>
          </div>
        </div>

        <div class="form-acciones">
          <button class="btn btn-secundario" (click)="cancelar()">Cancelar</button>
          <button class="btn btn-primario" [disabled]="guardando()" (click)="guardarEdicion()">
            @if (guardando()) { Guardando... } @else { Aceptar }
          </button>
        </div>
      </div>
    }

    <!-- Formulario cambiar contraseña -->
    @if (modoFormulario() === 'password') {
      <div class="formulario-panel">
        <h3>Cambiar contraseña — {{ seleccionado()!.nombreUsuario }}</h3>
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid">
          <div class="form-grupo">
            <label class="campo-requerido">Nueva contraseña</label>
            <input type="password" [(ngModel)]="nuevoPassword" placeholder="Minimo 6 caracteres">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Confirmar contraseña</label>
            <input type="password" [(ngModel)]="confirmarPassword" placeholder="Repetir contraseña">
          </div>
        </div>

        <div class="form-acciones">
          <button class="btn btn-secundario" (click)="cancelar()">Cancelar</button>
          <button class="btn btn-primario" [disabled]="guardando()" (click)="guardarPassword()">
            @if (guardando()) { Guardando... } @else { Aceptar }
          </button>
        </div>
      </div>
    }
  `
})
export class UsuariosComponent implements OnInit {
  private readonly svc = inject(UsuariosService);

  protected readonly roles = ROLES;

  cargando       = signal(false);
  guardando      = signal(false);
  error          = signal<string | null>(null);
  exito          = signal<string | null>(null);
  errorForm      = signal<string | null>(null);
  usuarios       = signal<UsuarioDto[]>([]);
  seleccionado   = signal<UsuarioDto | null>(null);
  modoFormulario = signal<ModoFormulario>(null);

  formCrear  = { nombreUsuario: '', password: '', nombreCompleto: '', email: '', rolId: 3, activo: true };
  formEditar = { nombreCompleto: '', email: '', rolId: 3, activo: true };
  nuevoPassword    = '';
  confirmarPassword = '';

  ngOnInit() { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.svc.obtener().subscribe({
      next:  data => { this.usuarios.set(data); this.cargando.set(false); },
      error: ()   => { this.error.set('Error al cargar usuarios.'); this.cargando.set(false); }
    });
  }

  seleccionar(u: UsuarioDto): void {
    this.seleccionado.set(this.seleccionado()?.usuarioId === u.usuarioId ? null : u);
    this.modoFormulario.set(null);
  }

  abrirFormulario(modo: ModoFormulario): void {
    this.errorForm.set(null);
    this.modoFormulario.set(modo);

    if (modo === 'crear') {
      this.formCrear = { nombreUsuario: '', password: '', nombreCompleto: '', email: '', rolId: 3, activo: true };
    } else if (modo === 'editar' && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.formEditar = { nombreCompleto: s.nombreCompleto, email: s.email ?? '', rolId: s.rolId, activo: s.activo };
    } else if (modo === 'password') {
      this.nuevoPassword     = '';
      this.confirmarPassword = '';
    }
  }

  cancelar(): void { this.modoFormulario.set(null); }

  guardarNuevo(): void {
    const f = this.formCrear;
    if (!f.nombreUsuario.trim())    { this.errorForm.set('El nombre de usuario es requerido.'); return; }
    if (f.password.length < 6)      { this.errorForm.set('La contrasenna debe tener al menos 6 caracteres.'); return; }
    if (!f.nombreCompleto.trim())   { this.errorForm.set('El nombre completo es requerido.'); return; }

    this.guardando.set(true);
    this.svc.crear({
      nombreUsuario:  f.nombreUsuario.trim(),
      password:       f.password,
      nombreCompleto: f.nombreCompleto.trim(),
      email:          f.email.trim() || null,
      rolId:          Number(f.rolId),
      activo:         f.activo
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.modoFormulario.set(null);
        this.mostrarExito('Usuario creado correctamente.');
        this.cargar();
      },
      error: err => { this.guardando.set(false); this.errorForm.set(err.error ?? 'Error al crear usuario.'); }
    });
  }

  guardarEdicion(): void {
    const f = this.formEditar;
    if (!f.nombreCompleto.trim()) { this.errorForm.set('El nombre completo es requerido.'); return; }

    this.guardando.set(true);
    this.svc.actualizar(this.seleccionado()!.usuarioId, {
      nombreCompleto: f.nombreCompleto.trim(),
      email:          f.email.trim() || null,
      rolId:          Number(f.rolId),
      activo:         f.activo
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.modoFormulario.set(null);
        this.mostrarExito('Usuario actualizado correctamente.');
        this.cargar();
      },
      error: err => { this.guardando.set(false); this.errorForm.set(err.error ?? 'Error al actualizar.'); }
    });
  }

  guardarPassword(): void {
    if (this.nuevoPassword.length < 6) {
      this.errorForm.set('La contrasenna debe tener al menos 6 caracteres.');
      return;
    }
    if (this.nuevoPassword !== this.confirmarPassword) {
      this.errorForm.set('Las contrasennas no coinciden.');
      return;
    }

    this.guardando.set(true);
    this.svc.cambiarContrasenna(this.seleccionado()!.usuarioId, { nuevoPassword: this.nuevoPassword }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.modoFormulario.set(null);
        this.mostrarExito('Contrasenna cambiada correctamente.');
      },
      error: err => { this.guardando.set(false); this.errorForm.set(err.error ?? 'Error al cambiar contrasenna.'); }
    });
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar el usuario "${s.nombreUsuario}"?`)) return;

    this.svc.eliminar(s.usuarioId).subscribe({
      next: () => {
        this.seleccionado.set(null);
        this.mostrarExito('Usuario eliminado.');
        this.cargar();
      },
      error: err => this.error.set(err.error ?? 'No se puede eliminar este usuario.')
    });
  }

  private mostrarExito(msg: string): void {
    this.exito.set(msg);
    setTimeout(() => this.exito.set(null), 3500);
  }
}

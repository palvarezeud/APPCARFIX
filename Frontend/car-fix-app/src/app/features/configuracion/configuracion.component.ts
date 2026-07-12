import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TallerService } from '../../services/taller.service';
import { ParametrosService } from '../../services/parametros.service';
import { ParametroDto } from '../../models/parametro.model';
import { MapaUbicacionComponent } from '../../shared/components/mapa-ubicacion/mapa-ubicacion.component';

type ModoParametro = 'crear' | 'editar';

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [DecimalPipe, MapaUbicacionComponent],
  template: `
<div class="pantalla-contenedor">

  <div class="pantalla-encabezado">
    <h2 class="pantalla-titulo">Configuracion del Sistema</h2>
  </div>

  <!-- ───── Datos del Taller ───── -->
  <div class="formulario-panel">
    <h3 class="formulario-titulo">Datos del Taller</h3>

    @if (cargandoTaller()) {
      <p class="texto-cargando">Cargando...</p>
    } @else {
      <div class="form-grid">
        <div class="form-grupo form-grupo-ancho">
          <label>Nombre del taller *</label>
          <input type="text" [value]="tallerNombre()"
                 (input)="tallerNombre.set($any($event.target).value)" />
        </div>
        <div class="form-grupo form-grupo-ancho">
          <label>Direccion / Descripcion de ubicacion *</label>
          <textarea rows="2" [value]="tallerUbicacionDescripcion()"
                    (input)="tallerUbicacionDescripcion.set($any($event.target).value)"></textarea>
        </div>
        <div class="form-grupo">
          <label>Telefonos *</label>
          <input type="text" [value]="tallerTelefonos()"
                 (input)="tallerTelefonos.set($any($event.target).value)" />
        </div>
        <div class="form-grupo">
          <label>Email *</label>
          <input type="email" [value]="tallerEmail()"
                 (input)="tallerEmail.set($any($event.target).value)" />
        </div>
        <div class="form-grupo form-grupo-ancho">
          <label>Geolocalizacion &mdash; haga clic en el mapa o arrastre el pin para ajustar</label>
          <app-mapa-ubicacion [latitud]="tallerLatitud()" [longitud]="tallerLongitud()"
                               (ubicacionCambiada)="onUbicacionTallerCambiada($event)" />
          <small class="texto-coordenadas">
            {{ tallerLatitud() != null && tallerLongitud() != null
                ? (tallerLatitud()! | number:'1.6-6') + ', ' + (tallerLongitud()! | number:'1.6-6')
                : 'Sin coordenadas definidas' }}
          </small>
        </div>
      </div>

      @if (errorTaller()) { <p class="error-form">{{ errorTaller() }}</p> }

      <div class="form-acciones">
        <button class="btn-accion" (click)="guardarTaller()" [disabled]="guardandoTaller()">
          {{ guardandoTaller() ? 'Guardando...' : 'Guardar' }}
        </button>
      </div>
    }
  </div>

  <!-- ───── Parametros del Sistema ───── -->
  <div class="pantalla-encabezado seccion-parametros">
    <h2 class="pantalla-titulo">Parametros del Sistema</h2>
    @if (!mostrarFormularioParam()) {
      <div class="acciones-barra">
        <button class="btn-accion" (click)="abrirFormularioParam('crear')">Agregar</button>
        <button class="btn-accion" (click)="abrirFormularioParam('editar')" [disabled]="!parametroSeleccionado()">Modificar</button>
        <button class="btn-accion btn-peligro" (click)="eliminarParametro()" [disabled]="!parametroSeleccionado()">Eliminar</button>
      </div>
    }
  </div>

  @if (!mostrarFormularioParam()) {
    <div class="tabla-contenedor">
      @if (cargandoParametros()) {
        <p class="texto-cargando">Cargando...</p>
      } @else if (parametros().length === 0) {
        <p class="texto-vacio">No hay parametros registrados.</p>
      } @else {
        <table class="tabla-datos">
          <thead><tr><th>Nombre</th><th>Valor</th></tr></thead>
          <tbody>
            @for (p of parametros(); track p.parametroId) {
              <tr [class.fila-seleccionada]="parametroSeleccionado()?.parametroId === p.parametroId"
                  (click)="seleccionarParametro(p)">
                <td>{{ p.nombre }}</td>
                <td>{{ p.valor }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  }

  @if (mostrarFormularioParam()) {
    <div class="formulario-panel">
      <h3 class="formulario-titulo">{{ modoParam() === 'crear' ? 'Nuevo parametro' : 'Modificar parametro' }}</h3>
      <div class="form-grid">
        <div class="form-grupo">
          <label>Nombre *</label>
          <input type="text" [value]="paramNombre()"
                 (input)="paramNombre.set($any($event.target).value)" />
        </div>
        <div class="form-grupo">
          <label>Valor *</label>
          <input type="text" [value]="paramValor()"
                 (input)="paramValor.set($any($event.target).value)" />
        </div>
      </div>
      @if (errorParam()) { <p class="error-form">{{ errorParam() }}</p> }
      <div class="form-acciones">
        <button class="btn-secundario" (click)="cancelarParam()" [disabled]="guardandoParam()">Cancelar</button>
        <button class="btn-accion" (click)="guardarParam()" [disabled]="guardandoParam()">
          {{ guardandoParam() ? 'Guardando...' : 'Aceptar' }}
        </button>
      </div>
    </div>
  }

</div>
  `,
  styles: [`
    .seccion-parametros { margin-top: 32px; }
    .texto-coordenadas { display: block; margin-top: 6px; color: var(--color-texto-suave); }
  `]
})
export class ConfiguracionComponent implements OnInit {
  private readonly tallerSvc     = inject(TallerService);
  private readonly parametrosSvc = inject(ParametrosService);

  // ── Taller ──────────────────────────────────────────────────────
  cargandoTaller  = signal(false);
  guardandoTaller = signal(false);
  errorTaller     = signal('');

  tallerNombre               = signal('');
  tallerUbicacionDescripcion = signal('');
  tallerTelefonos            = signal('');
  tallerEmail                = signal('');
  tallerLatitud              = signal<number | null>(null);
  tallerLongitud             = signal<number | null>(null);

  // ── Parametros ──────────────────────────────────────────────────
  parametros            = signal<ParametroDto[]>([]);
  cargandoParametros     = signal(false);
  parametroSeleccionado  = signal<ParametroDto | null>(null);

  modoParam              = signal<ModoParametro>('crear');
  mostrarFormularioParam  = signal(false);
  guardandoParam          = signal(false);
  errorParam              = signal('');

  paramNombre = signal('');
  paramValor  = signal('');

  ngOnInit() {
    this.cargarTaller();
    this.cargarParametros();
  }

  // ── Taller ──────────────────────────────────────────────────────
  cargarTaller() {
    this.cargandoTaller.set(true);
    this.errorTaller.set('');
    this.tallerSvc.obtener().subscribe({
      next: t => {
        this.tallerNombre.set(t.nombre);
        this.tallerUbicacionDescripcion.set(t.ubicacionDescripcion);
        this.tallerTelefonos.set(t.telefonos);
        this.tallerEmail.set(t.email);
        this.tallerLatitud.set(t.latitud);
        this.tallerLongitud.set(t.longitud);
        this.cargandoTaller.set(false);
      },
      error: err => {
        this.cargandoTaller.set(false);
        this.errorTaller.set(this.extraerError(err));
      }
    });
  }

  onUbicacionTallerCambiada(ev: { latitud: number; longitud: number }) {
    this.tallerLatitud.set(ev.latitud);
    this.tallerLongitud.set(ev.longitud);
  }

  guardarTaller() {
    if (!this.tallerNombre().trim() || !this.tallerUbicacionDescripcion().trim()
        || !this.tallerTelefonos().trim() || !this.tallerEmail().trim()) {
      this.errorTaller.set('Nombre, direccion, telefonos y email son obligatorios.');
      return;
    }
    this.guardandoTaller.set(true);
    this.errorTaller.set('');
    this.tallerSvc.actualizar({
      nombre: this.tallerNombre(),
      ubicacionDescripcion: this.tallerUbicacionDescripcion(),
      telefonos: this.tallerTelefonos(),
      email: this.tallerEmail(),
      latitud: this.tallerLatitud(),
      longitud: this.tallerLongitud()
    }).subscribe({
      next: () => this.guardandoTaller.set(false),
      error: err => {
        this.guardandoTaller.set(false);
        this.errorTaller.set(this.extraerError(err));
      }
    });
  }

  // ── Parametros: lista ───────────────────────────────────────────
  cargarParametros() {
    this.cargandoParametros.set(true);
    this.parametrosSvc.obtener().subscribe({
      next: lista => { this.parametros.set(lista); this.cargandoParametros.set(false); },
      error: () => this.cargandoParametros.set(false)
    });
  }

  seleccionarParametro(p: ParametroDto) {
    this.parametroSeleccionado.set(
      this.parametroSeleccionado()?.parametroId === p.parametroId ? null : p);
  }

  abrirFormularioParam(m: ModoParametro) {
    if (m === 'editar' && !this.parametroSeleccionado()) return;
    this.errorParam.set('');
    this.guardandoParam.set(false);

    if (m === 'crear') {
      this.paramNombre.set('');
      this.paramValor.set('');
    } else {
      const p = this.parametroSeleccionado()!;
      this.paramNombre.set(p.nombre);
      this.paramValor.set(p.valor);
    }

    this.modoParam.set(m);
    this.mostrarFormularioParam.set(true);
  }

  cancelarParam() {
    this.mostrarFormularioParam.set(false);
    this.errorParam.set('');
  }

  guardarParam() {
    if (!this.paramNombre().trim() || !this.paramValor().trim()) {
      this.errorParam.set('Nombre y valor son obligatorios.');
      return;
    }
    this.guardandoParam.set(true);
    this.errorParam.set('');

    const req = { nombre: this.paramNombre(), valor: this.paramValor() };
    const onExito = () => {
      this.guardandoParam.set(false);
      this.mostrarFormularioParam.set(false);
      this.parametroSeleccionado.set(null);
      this.cargarParametros();
    };
    const onError = (err: { error?: unknown }) => {
      this.guardandoParam.set(false);
      this.errorParam.set(this.extraerError(err));
    };

    if (this.modoParam() === 'crear') {
      this.parametrosSvc.crear(req).subscribe({ next: onExito, error: onError });
    } else {
      this.parametrosSvc.actualizar(this.parametroSeleccionado()!.parametroId, req)
        .subscribe({ next: onExito, error: onError });
    }
  }

  eliminarParametro() {
    const p = this.parametroSeleccionado();
    if (!p) return;
    if (!confirm(`Eliminar el parametro "${p.nombre}"?`)) return;
    this.parametrosSvc.eliminar(p.parametroId).subscribe({
      next: () => { this.parametroSeleccionado.set(null); this.cargarParametros(); },
      error: err => alert(this.extraerError(err))
    });
  }

  private extraerError(err: { error?: unknown }): string {
    const e = err.error;
    if (typeof e === 'string') return e;
    return (e as any)?.detail ?? (e as any)?.title ?? 'Error al procesar la solicitud.';
  }
}

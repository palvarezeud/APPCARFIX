import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { TiposReparacionService } from '../../services/tipos-reparacion.service';
import { TipoReparacionDto } from '../../models/tipo-reparacion.model';

const POR_PAGINA = 25;
type Modo = 'ver' | 'crear' | 'editar';

@Component({
  standalone: true,
  selector: 'app-catalogo-reparaciones',
  imports: [FormsModule, DecimalPipe],
  template: `
    <div class="pagina-header">
      <h2>Catálogo de Reparaciones</h2>
      <div class="acciones">
        <button class="btn btn-primario"   (click)="abrirFormulario('crear')">+ Agregar</button>
        <button class="btn btn-primario" [disabled]="!seleccionado()" (click)="abrirFormulario('editar')">Modificar</button>
        <button class="btn btn-peligro"    [disabled]="!seleccionado()" (click)="confirmarEliminar()">Eliminar</button>
      </div>
    </div>

    @if (!mostrarFormulario()) {
    <div class="filtro-bar">
      <input type="text" placeholder="Buscar por descripción..."
             [ngModel]="textoBusqueda()"
             (ngModelChange)="textoBusqueda.set($event); paginaActual.set(0)">
    </div>

    @if (error()) { <div class="alerta alerta-error">{{ error() }}</div> }

    @if (cargando()) {
      <div class="cargando"><span class="spinner"></span> Cargando...</div>
    } @else {
      <div class="tabla-contenedor">
        <table>
          <thead>
            <tr><th>#</th><th>Descripción</th><th>Duración (hrs)</th><th>Costo base</th></tr>
          </thead>
          <tbody>
            @for (t of itemsPagina(); track t.tipoReparacionId) {
              <tr [class.seleccionada]="seleccionado()?.tipoReparacionId === t.tipoReparacionId"
                  (click)="seleccionar(t)">
                <td>{{ t.tipoReparacionId }}</td>
                <td>{{ t.descripcionReparacion }}</td>
                <td>{{ t.duracionAproximadaHoras }}</td>
                <td>{{ t.costoBase | number:'1.2-2' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="celda-vacia">No hay tipos de reparación registrados.</td></tr>
            }
          </tbody>
        </table>

        @if (itemsFiltrados().length > POR_PAGINA) {
          <div class="paginacion">
            <button class="btn btn-outline" [disabled]="paginaActual() === 0"
                    (click)="paginaActual.update(p => p - 1)">‹</button>
            <span>{{ paginaActual() + 1 }} / {{ paginasTotales() }}</span>
            <button class="btn btn-outline" [disabled]="paginaActual() >= paginasTotales() - 1"
                    (click)="paginaActual.update(p => p + 1)">›</button>
          </div>
        }
      </div>
    }
    } <!-- fin @if (!mostrarFormulario()) -->

    @if (mostrarFormulario()) {
      <div class="formulario-panel">
        <h3>
          {{ modo() === 'ver' ? 'Detalle de tipo de reparacion' : modo() === 'editar' ? 'Modificar tipo de reparacion' : 'Nuevo tipo de reparacion' }}
        </h3>

        @if (modo() === 'ver') {
          <div class="banner-solo-lectura">&#128274; Vista de solo lectura — haga clic en Modificar para editar</div>
        }
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid" [class.solo-lectura]="modo() === 'ver'">
          <div class="form-grupo">
            <label class="campo-requerido">Descripción</label>
            <input type="text" [(ngModel)]="form.descripcionReparacion"
                   placeholder="Ej: Cambio de aceite y filtro">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Duración aproximada (horas)</label>
            <input type="number" [(ngModel)]="form.duracionAproximadaHoras" min="1"
                   placeholder="Minimo 1 hora">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Costo base</label>
            <input type="number" [(ngModel)]="form.costoBase" min="0" step="0.01"
                   placeholder="0.00">
          </div>
        </div>

        <div class="form-acciones">
          @if (modo() === 'ver') {
            <button class="btn btn-secundario" (click)="cancelar()">Cerrar</button>
          } @else {
            <button class="btn btn-secundario" (click)="cancelar()">Cancelar</button>
            <button class="btn btn-primario" [disabled]="guardando()" (click)="guardar()">
              @if (guardando()) { Guardando... } @else { Aceptar }
            </button>
          }
        </div>
      </div>
    }
  `
})
export class CatalogoReparacionesComponent implements OnInit {
  private readonly svc = inject(TiposReparacionService);
  protected readonly POR_PAGINA = POR_PAGINA;

  cargando          = signal(false);
  guardando         = signal(false);
  error             = signal<string | null>(null);
  errorForm         = signal<string | null>(null);
  private todos     = signal<TipoReparacionDto[]>([]);
  textoBusqueda     = signal('');
  paginaActual      = signal(0);
  seleccionado      = signal<TipoReparacionDto | null>(null);
  mostrarFormulario = signal(false);
  modo              = signal<Modo>('crear');

  form = { tipoReparacionId: 0, descripcionReparacion: '', duracionAproximadaHoras: 1, costoBase: 0 as number };

  itemsFiltrados = computed(() => {
    const f = this.textoBusqueda().toLowerCase().trim();
    return f ? this.todos().filter(t => t.descripcionReparacion.toLowerCase().includes(f)) : this.todos();
  });

  paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / POR_PAGINA));
  itemsPagina    = computed(() => {
    const ini = this.paginaActual() * POR_PAGINA;
    return this.itemsFiltrados().slice(ini, ini + POR_PAGINA);
  });

  ngOnInit() { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.svc.obtener().subscribe({
      next:  d => { this.todos.set(d); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar catalogo.'); this.cargando.set(false); }
    });
  }

  seleccionar(t: TipoReparacionDto): void {
    if (this.seleccionado()?.tipoReparacionId === t.tipoReparacionId && this.mostrarFormulario()) {
      this.seleccionado.set(null);
      this.mostrarFormulario.set(false);
    } else {
      this.seleccionado.set(t);
      this.abrirFormulario('ver');
    }
  }

  abrirFormulario(m: Modo): void {
    this.modo.set(m);
    this.errorForm.set(null);
    if ((m === 'ver' || m === 'editar') && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.form = {
        tipoReparacionId:        s.tipoReparacionId,
        descripcionReparacion:   s.descripcionReparacion,
        duracionAproximadaHoras: s.duracionAproximadaHoras,
        costoBase:               s.costoBase
      };
    } else {
      this.form = { tipoReparacionId: 0, descripcionReparacion: '', duracionAproximadaHoras: 1, costoBase: 0 };
    }
    this.mostrarFormulario.set(true);
  }

  cancelar(): void {
    this.mostrarFormulario.set(false);
    if (this.modo() === 'crear') this.seleccionado.set(null);
  }

  guardar(): void {
    if (!this.form.descripcionReparacion.trim()) {
      this.errorForm.set('La descripcion es requerida.'); return;
    }
    if (this.form.duracionAproximadaHoras < 1) {
      this.errorForm.set('La duracion minima es 1 hora.'); return;
    }

    this.guardando.set(true);
    this.errorForm.set(null);

    const onExito = () => {
      this.guardando.set(false);
      this.mostrarFormulario.set(false);
      this.seleccionado.set(null);
      this.cargar();
    };
    const onError = (err: { error?: string }) => {
      this.guardando.set(false);
      this.errorForm.set(err.error ?? 'Error al guardar.');
    };

    if (this.modo() === 'editar') {
      this.svc.actualizar(this.form.tipoReparacionId, {
        descripcionReparacion:   this.form.descripcionReparacion.trim(),
        duracionAproximadaHoras: this.form.duracionAproximadaHoras,
        costoBase:               this.form.costoBase
      }).subscribe({ next: onExito, error: onError });
    } else {
      this.svc.crear({
        descripcionReparacion:   this.form.descripcionReparacion.trim(),
        duracionAproximadaHoras: this.form.duracionAproximadaHoras,
        costoBase:               this.form.costoBase
      }).subscribe({ next: onExito, error: onError });
    }
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar "${s.descripcionReparacion}"?`)) return;

    this.svc.eliminar(s.tipoReparacionId).subscribe({
      next: () => { this.seleccionado.set(null); this.mostrarFormulario.set(false); this.cargar(); },
      error: err => this.error.set(err.error ?? 'No se puede eliminar este tipo de reparacion.')
    });
  }
}

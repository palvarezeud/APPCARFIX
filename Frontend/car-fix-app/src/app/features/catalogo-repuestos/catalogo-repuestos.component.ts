import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe, SlicePipe } from '@angular/common';
import { HistoricoRepuestosService } from '../../services/historico-repuestos.service';
import { HistoricoRespuestoDto } from '../../models/historico-repuesto.model';

const POR_PAGINA = 25;
type Modo = 'ver' | 'crear' | 'editar';

@Component({
  standalone: true,
  selector: 'app-catalogo-repuestos',
  imports: [FormsModule, DecimalPipe, SlicePipe],
  template: `
    <div class="pagina-header">
      <h2>Catalogo de Repuestos (Historico)</h2>
      <div class="acciones">
        <button class="btn btn-primario"   (click)="abrirFormulario('crear')">+ Agregar</button>
        <button class="btn btn-primario" [disabled]="!seleccionado()" (click)="abrirFormulario('editar')">Modificar</button>
        <button class="btn btn-peligro"    [disabled]="!seleccionado()" (click)="confirmarEliminar()">Eliminar</button>
      </div>
    </div>

    @if (!mostrarFormulario()) {
    <div class="filtro-bar">
      <input type="text" placeholder="Buscar por marca, modelo, repuesto..."
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
            <tr><th>#</th><th>Marca</th><th>Modelo</th><th>Año</th><th>Repuesto</th><th>Repuestera</th><th>Precio</th><th>Fecha</th></tr>
          </thead>
          <tbody>
            @for (h of itemsPagina(); track h.respuestoHistoricoId) {
              <tr [class.seleccionada]="seleccionado()?.respuestoHistoricoId === h.respuestoHistoricoId"
                  (click)="seleccionar(h)">
                <td>{{ h.respuestoHistoricoId }}</td>
                <td>{{ h.marca }}</td>
                <td>{{ h.modelo }}</td>
                <td>{{ h.annio }}</td>
                <td>{{ h.repuestoDecripcion }}</td>
                <td>{{ h.repuestera }}</td>
                <td>{{ h.precio | number:'1.2-2' }}</td>
                <td>{{ h.fechaCompra | slice:0:10 }}</td>
              </tr>
            } @empty {
              <tr><td colspan="8" class="celda-vacia">No hay registros en el historico.</td></tr>
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
          {{ modo() === 'ver' ? 'Detalle de repuesto' : modo() === 'editar' ? 'Modificar repuesto' : 'Nuevo repuesto en historico' }}
        </h3>

        @if (modo() === 'ver') {
          <div class="banner-solo-lectura">&#128274; Vista de solo lectura — haga clic en Modificar para editar</div>
        }
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid" [class.solo-lectura]="modo() === 'ver'">
          <div class="form-grupo">
            <label class="campo-requerido">Marca</label>
            <input type="text" [(ngModel)]="form.marca" placeholder="Ej: Toyota">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Modelo</label>
            <input type="text" [(ngModel)]="form.modelo" placeholder="Ej: Corolla">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Año</label>
            <input type="number" [(ngModel)]="form.annio" placeholder="Ej: 2020" min="1901">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Descripcion del repuesto</label>
            <input type="text" [(ngModel)]="form.repuestoDecripcion" placeholder="Ej: Filtro de aceite">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Precio</label>
            <input type="number" [(ngModel)]="form.precio" min="0" step="0.01" placeholder="0.00">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Repuestera</label>
            <input type="text" [(ngModel)]="form.repuestera" placeholder="Ej: AutoRepuestos CR">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Fecha de compra</label>
            <input type="date" [(ngModel)]="form.fechaCompra">
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
export class CatalogoRepuestosComponent implements OnInit {
  private readonly svc = inject(HistoricoRepuestosService);
  protected readonly POR_PAGINA = POR_PAGINA;

  cargando          = signal(false);
  guardando         = signal(false);
  error             = signal<string | null>(null);
  errorForm         = signal<string | null>(null);
  private todos     = signal<HistoricoRespuestoDto[]>([]);
  textoBusqueda     = signal('');
  paginaActual      = signal(0);
  seleccionado      = signal<HistoricoRespuestoDto | null>(null);
  mostrarFormulario = signal(false);
  modo              = signal<Modo>('crear');

  form = { marca: '', modelo: '', annio: new Date().getFullYear(), repuestoDecripcion: '', precio: 0 as number, repuestera: '', fechaCompra: '' };

  itemsFiltrados = computed(() => {
    const f = this.textoBusqueda().toLowerCase().trim();
    if (!f) return this.todos();
    return this.todos().filter(h =>
      h.marca.toLowerCase().includes(f) ||
      h.modelo.toLowerCase().includes(f) ||
      h.repuestoDecripcion.toLowerCase().includes(f) ||
      h.repuestera.toLowerCase().includes(f)
    );
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
      error: () => { this.error.set('Error al cargar historico.'); this.cargando.set(false); }
    });
  }

  seleccionar(h: HistoricoRespuestoDto): void {
    if (this.seleccionado()?.respuestoHistoricoId === h.respuestoHistoricoId && this.mostrarFormulario()) {
      this.seleccionado.set(null);
      this.mostrarFormulario.set(false);
    } else {
      this.seleccionado.set(h);
      this.abrirFormulario('ver');
    }
  }

  abrirFormulario(m: Modo): void {
    this.modo.set(m);
    this.errorForm.set(null);
    if ((m === 'ver' || m === 'editar') && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.form = {
        marca:              s.marca,
        modelo:             s.modelo,
        annio:              s.annio,
        repuestoDecripcion: s.repuestoDecripcion,
        precio:             s.precio,
        repuestera:         s.repuestera,
        fechaCompra:        s.fechaCompra.slice(0, 10)
      };
    } else {
      this.form = {
        marca: '', modelo: '', annio: new Date().getFullYear(), repuestoDecripcion: '', precio: 0, repuestera: '',
        fechaCompra: new Date().toISOString().substring(0, 10)
      };
    }
    this.mostrarFormulario.set(true);
  }

  cancelar(): void {
    this.mostrarFormulario.set(false);
    if (this.modo() === 'crear') this.seleccionado.set(null);
  }

  guardar(): void {
    if (!this.form.marca.trim())              { this.errorForm.set('La marca es requerida.'); return; }
    if (!this.form.modelo.trim())             { this.errorForm.set('El modelo es requerido.'); return; }
    if (!this.form.annio || this.form.annio < 1901) { this.errorForm.set('El año es invalido.'); return; }
    if (!this.form.repuestoDecripcion.trim()) { this.errorForm.set('La descripcion es requerida.'); return; }
    if (!this.form.repuestera.trim())         { this.errorForm.set('La repuestera es requerida.'); return; }
    if (!this.form.fechaCompra)               { this.errorForm.set('La fecha de compra es requerida.'); return; }

    this.guardando.set(true);
    this.errorForm.set(null);

    const body = {
      marca:              this.form.marca.trim(),
      modelo:             this.form.modelo.trim(),
      annio:              this.form.annio,
      repuestoDecripcion: this.form.repuestoDecripcion.trim(),
      precio:             this.form.precio,
      repuestera:         this.form.repuestera.trim(),
      fechaCompra:        this.form.fechaCompra
    };

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
      this.svc.actualizar(this.seleccionado()!.respuestoHistoricoId, body)
        .subscribe({ next: onExito, error: onError });
    } else {
      this.svc.crear(body).subscribe({ next: onExito, error: onError });
    }
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar "${s.repuestoDecripcion}" (${s.marca} ${s.modelo})?`)) return;

    this.svc.eliminar(s.respuestoHistoricoId).subscribe({
      next:  () => { this.seleccionado.set(null); this.mostrarFormulario.set(false); this.cargar(); },
      error: err => this.error.set(err.error ?? 'No se puede eliminar este registro.')
    });
  }
}

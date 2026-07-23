import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SlicePipe } from '@angular/common';
import { Router } from '@angular/router';
import { OrdenesService, OrdenRequest } from '../../services/ordenes.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { ReparacionesService } from '../../services/reparaciones.service';
import { RepuestosService } from '../../services/repuestos.service';
import { OrdenServicioDto, ESTADOS_ORDEN } from '../../models/orden-servicio.model';
import { VehiculoDto } from '../../models/vehiculo.model';
import { ReparacionDto } from '../../models/reparacion.model';
import { RepuestoDto } from '../../models/repuesto.model';
import { AsistenteVozService } from '../../core/voz/asistente-voz.service';

const POR_PAGINA = 15;

type ModoFormulario = 'ver' | 'crear' | 'editar';

@Component({
  standalone: true,
  selector: 'app-ordenes',
  imports: [FormsModule, SlicePipe],
  template: `
    <div class="pagina-header">
      <h2>Ordenes de Servicio</h2>
      @if (!mostrarFormulario()) {
        <div class="acciones">
          <button class="btn btn-primario"   (click)="abrirFormulario('crear')">+ Agregar</button>
          <button class="btn btn-primario" [disabled]="!seleccionado()" (click)="abrirFormulario('editar')">Modificar</button>
          <button class="btn btn-peligro"    [disabled]="!seleccionado()" (click)="confirmarEliminar()">Eliminar</button>
        </div>
      }
    </div>

    @if (!mostrarFormulario()) {
    <div class="filtro-bar">
      <input type="text" placeholder="Buscar por numero de orden, placa o cliente..."
             [ngModel]="textoBusqueda()"
             (ngModelChange)="textoBusqueda.set($event); paginaActual.set(0)">
      <div class="filtro-tipo">
        @for (e of estadosFiltro; track e.id) {
          <label>
            <input type="radio" name="filtroEstado"
                   [checked]="filtroEstado() === e.id"
                   (change)="filtroEstado.set(e.id); paginaActual.set(0)">
            {{ e.nombre }}
          </label>
        }
      </div>
    </div>

    @if (error()) { <div class="alerta alerta-error">{{ error() }}</div> }
    @if (mensajeExito()) { <div class="alerta alerta-exito">{{ mensajeExito() }}</div> }

    @if (cargando()) {
      <div class="cargando"><span class="spinner"></span> Cargando...</div>
    } @else {
      <div class="tabla-contenedor">
        <table class="tabla-ordenes">
          <thead>
            <tr>
              <th>#</th><th>Placa</th><th>Vehículo</th><th>Cliente</th>
              <th class="col-oculta-movil">Ingreso</th><th class="col-oculta-movil">Salida</th>
              <th class="col-oculta-movil">Estado</th><th class="col-oculta-movil">Garantia</th>
              <th class="col-oculta-movil">Factura</th>
            </tr>
          </thead>
          <tbody>
            @for (o of itemsPagina(); track o.ordenServicioId) {
              <tr [class.seleccionada]="seleccionado()?.ordenServicioId === o.ordenServicioId"
                  (click)="seleccionar(o)" (dblclick)="verDetalle(o)">
                <td>{{ o.ordenServicioId }}</td>
                <td>{{ o.placa }}</td>
                <td>{{ o.marca }} {{ o.modelo }}</td>
                <td>{{ o.nombreCliente }}</td>
                <td class="col-oculta-movil">{{ o.fechaIngreso | slice:0:10 }}</td>
                <td class="col-oculta-movil">{{ o.fechaSalida | slice:0:10 }}</td>
                <td class="col-oculta-movil"><span [class]="'badge badge-' + o.estadoOrdenId">{{ o.estadoOrdenDescripcion }}</span></td>
                <td class="col-oculta-movil">{{ o.esGarantia ? 'Si' : '—' }}</td>
                <td class="col-oculta-movil"><button class="btn-link-factura" (click)="verFactura(o.facturaId, $event)">#{{ o.facturaId }}</button></td>
              </tr>
            } @empty {
              <tr><td colspan="9" class="celda-vacia">No hay ordenes de servicio.</td></tr>
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
          {{ modoFormulario() === 'ver' ? 'Detalle de orden #' + seleccionado()!.ordenServicioId
           : modoFormulario() === 'editar' ? 'Modificar orden #' + seleccionado()!.ordenServicioId
           : 'Nueva orden de servicio' }}
        </h3>
        @if (esSoloLectura) {
          <div class="banner-solo-lectura">&#128274; Vista de solo lectura — haga clic en Modificar para editar</div>
        }
        @if (esFinalizado) {
          <div class="banner-solo-lectura">&#9888;&#65039; Orden finalizada — solo puede cambiar el estado</div>
        }
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid" [class.solo-lectura]="esSoloLectura">
          <div class="form-grupo vehiculo-autocompletar" style="grid-column: 1 / -1">
            <label class="campo-requerido">Vehículo</label>
            <input type="text" placeholder="Buscar por placa, marca, modelo o cliente..."
                   [ngModel]="vehiculoTexto()"
                   (ngModelChange)="onVehiculoTextoInput($event)"
                   (focus)="mostrarSugerenciasVehiculo.set(true)"
                   (blur)="ocultarSugerenciasVehiculo()"
                   [disabled]="esFinalizado"
                   autocomplete="off" />
            @if (mostrarSugerenciasVehiculo() && vehiculoTexto().trim()) {
              <ul class="lista-sugerencias-vehiculo">
                @for (v of vehiculoSugerencias(); track v.vehiculoId) {
                  <li (mousedown)="$event.preventDefault(); seleccionarVehiculoSugerido(v)">
                    {{ v.placa ? v.placa + ' — ' : '' }}{{ v.marca }} {{ v.modelo }} ({{ v.nombreCliente }})
                  </li>
                } @empty {
                  <li class="sin-resultados">Sin resultados</li>
                }
              </ul>
            }
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Fecha de ingreso</label>
            <input type="datetime-local" [(ngModel)]="form.fechaIngreso" [disabled]="esFinalizado">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Fecha de salida estimada</label>
            <input type="datetime-local" [(ngModel)]="form.fechaSalida" [disabled]="esFinalizado">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Tipo de Servicio</label>
            <div class="radio-grupo">
              <label><input type="radio" name="garantia" [value]="false" [(ngModel)]="form.esGarantia" [disabled]="esFinalizado"> Normal</label>
              <label><input type="radio" name="garantia" [value]="true"  [(ngModel)]="form.esGarantia" [disabled]="esFinalizado"> Garantia</label>
            </div>
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Estado</label>
            <select [(ngModel)]="form.estadoOrdenId">
              @for (e of estadosDisponibles; track e.id) {
                <option [ngValue]="e.id">{{ e.nombre }}</option>
              }
            </select>
          </div>
          <div class="form-grupo" style="grid-column: 1 / -1">
            <label class="campo-requerido">Problema reportado por el cliente</label>
            <textarea [(ngModel)]="form.problemaGeneral" rows="3"
                      placeholder="Describe el problema o revision que se debe realizar..."
                      [disabled]="esFinalizado"></textarea>
          </div>
        </div>

        @if (modoFormulario() !== 'crear' && seleccionado()) {
          <div class="seccion-factura">
            @if (cargandoDetalle()) {
              <p class="texto-cargando" style="margin-top:12px">Cargando detalle...</p>
            } @else {
              <div class="detalle-listas">
                <div class="detalle-lista-bloque">
                  <h5 class="detalle-lista-titulo">Reparaciones ({{ reparaciones().length }})</h5>
                  @if (reparaciones().length === 0) {
                    <p class="texto-vacio">Sin reparaciones registradas.</p>
                  } @else {
                    <table class="tabla-datos tabla-detalle-orden">
                      <thead>
                        <tr><th>Descripcion</th><th class="col-check" style="text-align:center">Listo</th></tr>
                      </thead>
                      <tbody>
                        @for (r of reparaciones(); track r.reparacionId) {
                          <tr>
                            <td>{{ r.descripcionReparacion }}</td>
                            <td class="col-check" style="text-align:center">
                              <input type="checkbox" [checked]="r.listo"
                                     [disabled]="esSoloLectura || esFinalizado"
                                     (change)="marcarListoReparacion(r, $any($event.target).checked)"
                                     style="width:16px;height:16px;cursor:pointer">
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  }
                </div>
                <div class="detalle-lista-bloque">
                  <h5 class="detalle-lista-titulo">Repuestos ({{ repuestos().length }})</h5>
                  @if (repuestos().length === 0) {
                    <p class="texto-vacio">Sin repuestos registrados.</p>
                  } @else {
                    <table class="tabla-datos tabla-detalle-orden">
                      <thead>
                        <tr><th>Repuesto</th><th class="col-check" style="text-align:center">Incluido</th></tr>
                      </thead>
                      <tbody>
                        @for (r of repuestos(); track r.repuestoId) {
                          <tr>
                            <td>{{ r.nombreRepuesto }}</td>
                            <td class="col-check" style="text-align:center">
                              <input type="checkbox" [checked]="r.incluido"
                                     [disabled]="esSoloLectura || esFinalizado"
                                     (change)="marcarIncluidoRepuesto(r, $any($event.target).checked)"
                                     style="width:16px;height:16px;cursor:pointer">
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  }
                </div>
              </div>
            }
          </div>
        }

        <div class="form-acciones" style="margin-top:24px;padding-top:24px;border-top:2px solid var(--color-borde)">
          @if (esSoloLectura) {
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

  `,
  styles: [`
    .vehiculo-autocompletar { position: relative; }
    .lista-sugerencias-vehiculo {
      position: absolute; top: 100%; left: 0; right: 0; z-index: 20;
      list-style: none; margin: 2px 0 0; padding: 4px 0; max-height: 240px; overflow-y: auto;
      background: var(--color-tarjeta); border: 1px solid var(--color-borde);
      border-radius: var(--radio-borde); box-shadow: 0 4px 12px rgba(0,0,0,.12);
    }
    .lista-sugerencias-vehiculo li {
      padding: 8px 14px; font-size: 13px; cursor: pointer;
    }
    .lista-sugerencias-vehiculo li:hover { background: #f8f9fa; }
    .lista-sugerencias-vehiculo li.sin-resultados {
      color: var(--color-texto-suave); cursor: default;
    }
    .lista-sugerencias-vehiculo li.sin-resultados:hover { background: transparent; }
  `]
})
export class OrdenesComponent implements OnInit {
  private readonly svc            = inject(OrdenesService);
  private readonly asistenteVoz   = inject(AsistenteVozService);
  private readonly vehiculosSvc   = inject(VehiculosService);
  private readonly reparacionesSvc = inject(ReparacionesService);
  private readonly repuestosSvc   = inject(RepuestosService);
  private readonly router         = inject(Router);
  protected readonly POR_PAGINA = POR_PAGINA;

  cargando          = signal(false);
  guardando         = signal(false);
  error             = signal<string | null>(null);
  errorForm         = signal<string | null>(null);
  mensajeExito      = signal<string | null>(null);
  private todos     = signal<OrdenServicioDto[]>([]);
  vehiculos         = signal<VehiculoDto[]>([]);
  textoBusqueda     = signal('');
  filtroEstado      = signal<number>(0);
  paginaActual      = signal(0);
  seleccionado      = signal<OrdenServicioDto | null>(null);
  mostrarFormulario = signal(false);
  modoFormulario    = signal<ModoFormulario>('crear');

  reparaciones      = signal<ReparacionDto[]>([]);
  repuestos         = signal<RepuestoDto[]>([]);
  cargandoDetalle   = signal(false);

  vehiculoTexto              = signal('');
  mostrarSugerenciasVehiculo = signal(false);

  vehiculoSugerencias = computed(() => {
    const t = this.vehiculoTexto().toLowerCase().trim();
    if (!t) return [];
    return this.vehiculos().filter(v =>
      (v.placa ?? '').toLowerCase().includes(t) ||
      v.marca.toLowerCase().includes(t) ||
      (v.modelo ?? '').toLowerCase().includes(t) ||
      v.nombreCliente.toLowerCase().includes(t)
    ).slice(0, 20);
  });

  estadosDisponibles = [1, 2, 3, 4, 5].map(id => ({ id, nombre: ESTADOS_ORDEN[id] }));
  estadosFiltro      = [{ id: 0, nombre: 'Todos' }, ...this.estadosDisponibles];

  form: { vehiculoId: number; fechaIngreso: string; fechaSalida: string;
          problemaGeneral: string; esGarantia: boolean; estadoOrdenId: number } = {
    vehiculoId: 0, fechaIngreso: '', fechaSalida: '', problemaGeneral: '', esGarantia: false, estadoOrdenId: 1
  };

  itemsFiltrados = computed(() => {
    const f  = this.textoBusqueda().toLowerCase().trim();
    const st = this.filtroEstado();
    return this.todos().filter(o => {
      const coincideTexto = !f ||
        o.ordenServicioId.toString().includes(f) ||
        o.placa.toLowerCase().includes(f) ||
        o.nombreCliente.toLowerCase().includes(f) ||
        o.marca.toLowerCase().includes(f);
      const coincideEstado = st === 0 || o.estadoOrdenId === st;
      return coincideTexto && coincideEstado;
    });
  });

  paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / POR_PAGINA));
  itemsPagina    = computed(() => {
    const ini = this.paginaActual() * POR_PAGINA;
    return this.itemsFiltrados().slice(ini, ini + POR_PAGINA);
  });

  ngOnInit() {
    this.cargar();
    this.aplicarAccionVoz();
  }

  private aplicarAccionVoz(): void {
    const accion = this.asistenteVoz.tomarAccionPendiente('ordenes');
    if (!accion) return;

    if (accion.tipo === 'abrir-crear') {
      this.abrirFormulario('crear');
    } else if (accion.tipo === 'buscar') {
      this.textoBusqueda.set(accion.termino);
    }
  }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.svc.obtener().subscribe({
      next:  data => { this.todos.set(data); this.cargando.set(false); },
      error: ()   => { this.error.set('Error al cargar ordenes.'); this.cargando.set(false); }
    });
  }

  seleccionar(o: OrdenServicioDto): void {
    this.error.set(null);
    this.mensajeExito.set(null);
    if (this.seleccionado()?.ordenServicioId === o.ordenServicioId) {
      this.seleccionado.set(null);
    } else {
      this.seleccionado.set(o);
    }
  }

  verDetalle(o: OrdenServicioDto): void {
    this.seleccionado.set(o);
    this.abrirFormulario('ver');
  }

  abrirFormulario(modo: ModoFormulario): void {
    this.modoFormulario.set(modo);
    this.errorForm.set(null);
    this.mensajeExito.set(null);

    this.mostrarSugerenciasVehiculo.set(false);

    if ((modo === 'ver' || modo === 'editar') && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.form = {
        vehiculoId:      s.vehiculoId,
        fechaIngreso:    s.fechaIngreso.slice(0, 16),
        fechaSalida:     s.fechaSalida.slice(0, 16),
        problemaGeneral: s.problemaGeneral,
        esGarantia:      s.esGarantia,
        estadoOrdenId:   s.estadoOrdenId
      };
      this.vehiculoTexto.set(`${s.placa ? s.placa + ' — ' : ''}${s.marca} ${s.modelo} (${s.nombreCliente})`);
      this.cargarVehiculos();
    } else if (modo === 'crear') {
      const ahora = new Date();
      const salida = new Date(ahora);
      salida.setDate(salida.getDate() + 3);
      this.form = {
        vehiculoId:      0,
        fechaIngreso:    this.toDatetimeLocal(ahora),
        fechaSalida:     this.toDatetimeLocal(salida),
        problemaGeneral: '',
        esGarantia:      false,
        estadoOrdenId:   1
      };
      this.vehiculoTexto.set('');
      this.cargarVehiculos();
    }

    this.mostrarFormulario.set(true);

    if (modo !== 'crear') {
      this.cargarDetalle(this.seleccionado()!.facturaId);
    } else {
      this.reparaciones.set([]);
      this.repuestos.set([]);
    }
  }

  private cargarDetalle(facturaId: number): void {
    this.cargandoDetalle.set(true);
    this.reparacionesSvc.obtenerPorFactura(facturaId).subscribe({
      next: data => this.reparaciones.set(data),
      error: () => this.reparaciones.set([])
    });
    this.repuestosSvc.obtenerPorFactura(facturaId).subscribe({
      next: data => { this.repuestos.set(data); this.cargandoDetalle.set(false); },
      error: () => { this.repuestos.set([]); this.cargandoDetalle.set(false); }
    });
  }

  cancelar(): void {
    this.mostrarFormulario.set(false);
    if (this.modoFormulario() === 'crear') this.seleccionado.set(null);
  }

  cargarVehiculos(): void {
    if (this.vehiculos().length > 0) return;
    this.vehiculosSvc.obtener().subscribe({ next: data => this.vehiculos.set(data) });
  }

  onVehiculoTextoInput(valor: string): void {
    this.vehiculoTexto.set(valor);
    this.mostrarSugerenciasVehiculo.set(true);
    this.form.vehiculoId = 0;
  }

  seleccionarVehiculoSugerido(v: VehiculoDto): void {
    this.form.vehiculoId = v.vehiculoId;
    this.vehiculoTexto.set(`${v.placa ? v.placa + ' — ' : ''}${v.marca} ${v.modelo} (${v.nombreCliente})`);
    this.mostrarSugerenciasVehiculo.set(false);
  }

  ocultarSugerenciasVehiculo(): void {
    setTimeout(() => this.mostrarSugerenciasVehiculo.set(false), 150);
  }

  private toDatetimeLocal(d: Date): string {
    return d.toISOString().slice(0, 16);
  }

  guardar(): void {
    const onExito = () => {
      this.guardando.set(false);
      this.mostrarFormulario.set(false);
      this.seleccionado.set(null);
      this.cargar();
    };
    const onError = (err: { error?: unknown }) => {
      this.guardando.set(false);
      const e = err.error;
      const msg = typeof e === 'string' ? e
        : (e as any)?.detail ?? (e as any)?.title ?? 'Error al guardar.';
      this.errorForm.set(msg);
    };

    // Orden en Finalizado: solo cambia el estado via endpoint dedicado
    if (this.esFinalizado) {
      this.guardando.set(true);
      this.errorForm.set(null);
      this.svc.cambiarEstado(this.seleccionado()!.ordenServicioId, this.form.estadoOrdenId)
        .subscribe({ next: onExito, error: onError });
      return;
    }

    if (!this.form.vehiculoId)             { this.errorForm.set('Seleccione un vehiculo.'); return; }
    if (!this.form.fechaIngreso)           { this.errorForm.set('La fecha de ingreso es requerida.'); return; }
    if (!this.form.fechaSalida)            { this.errorForm.set('La fecha de salida es requerida.'); return; }
    if (!this.form.problemaGeneral.trim()) { this.errorForm.set('El problema general es requerido.'); return; }

    this.guardando.set(true);
    this.errorForm.set(null);

    const body: OrdenRequest = {
      vehiculoId:      this.form.vehiculoId,
      fechaIngreso:    this.form.fechaIngreso,
      fechaSalida:     this.form.fechaSalida,
      problemaGeneral: this.form.problemaGeneral.trim(),
      esGarantia:      this.form.esGarantia,
      ...(this.modoFormulario() === 'editar' && { estadoOrdenId: this.form.estadoOrdenId })
    };

    if (this.modoFormulario() === 'editar') {
      this.svc.actualizar(this.seleccionado()!.ordenServicioId, body)
        .subscribe({ next: onExito, error: onError });
    } else {
      this.svc.crear(body).subscribe({ next: onExito, error: onError });
    }
  }

  get esSoloLectura(): boolean {
    return this.modoFormulario() === 'ver' || (this.seleccionado()?.estadoOrdenId ?? 0) >= 5;
  }

  get esFinalizado(): boolean {
    return this.modoFormulario() !== 'ver' && (this.seleccionado()?.estadoOrdenId ?? 0) === 4;
  }

  marcarListoReparacion(r: ReparacionDto, listo: boolean): void {
    this.reparacionesSvc.marcarListo(r.reparacionId, listo).subscribe({
      next: () => {
        const actualizada = this.reparaciones().map(x =>
          x.reparacionId === r.reparacionId ? { ...x, listo } : x
        );
        this.reparaciones.set(actualizada);

        const orden = this.seleccionado();
        const todasListas = actualizada.length > 0 && actualizada.every(x => x.listo);

        if (todasListas && orden && orden.estadoOrdenId < 4) {
          if (confirm('Todas las reparaciones están listas. ¿Desea pasar la orden a estado Finalizado?')) {
            this.svc.cambiarEstado(orden.ordenServicioId, 4).subscribe({
              next: () => {
                this.cargar();
                this.seleccionado.update(o => o ? { ...o, estadoOrdenId: 4, estadoOrdenDescripcion: 'Finalizado' } : o);
              }
            });
          }
        }
      }
    });
  }

  marcarIncluidoRepuesto(r: RepuestoDto, incluido: boolean): void {
    this.repuestosSvc.marcarIncluido(r.repuestoId, incluido).subscribe({
      next: () => {
        const actualizada = this.repuestos().map(x =>
          x.repuestoId === r.repuestoId ? { ...x, incluido } : x
        );
        this.repuestos.set(actualizada);
      }
    });
  }

  verFactura(facturaId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/facturas'], { queryParams: { id: facturaId } });
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar orden #${s.ordenServicioId} (${s.placa} — ${s.marca} ${s.modelo})?`)) return;

    this.svc.eliminar(s.ordenServicioId).subscribe({
      next:  () => {
        this.seleccionado.set(null);
        this.mensajeExito.set('Orden eliminada correctamente.');
        this.cargar();
      },
      error: (err: { error?: string }) => this.error.set(err.error ?? 'No se puede eliminar esta orden.')
    });
  }
}

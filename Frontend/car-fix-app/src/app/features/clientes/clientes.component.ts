import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClientesService } from '../../services/clientes.service';
import { ClienteDto } from '../../models/cliente.model';
import { AsistenteVozService } from '../../core/voz/asistente-voz.service';

const POR_PAGINA = 25;
const TAMANO_PAGINA_MOVIL = 15;
const CONSULTA_MOVIL = '(max-width: 768px)';
type Modo = 'ver' | 'crear' | 'editar';

@Component({
  standalone: true,
  selector: 'app-clientes',
  imports: [FormsModule],
  template: `
    <div class="pagina-header">
      <h2>Clientes</h2>
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
      <input type="text" placeholder="Buscar por nombre..."
             [ngModel]="textoBusqueda()"
             (ngModelChange)="textoBusqueda.set($event); reiniciarPaginacion()">
      <div class="filtro-tipo">
        <label>
          <input type="radio" name="filtroTipo" value="todos"
                 [checked]="filtroTipo() === 'todos'"
                 (change)="filtroTipo.set('todos'); reiniciarPaginacion()">
          Todos
        </label>
        <label>
          <input type="radio" name="filtroTipo" value="persona"
                 [checked]="filtroTipo() === 'persona'"
                 (change)="filtroTipo.set('persona'); reiniciarPaginacion()">
          Personas
        </label>
        <label>
          <input type="radio" name="filtroTipo" value="empresa"
                 [checked]="filtroTipo() === 'empresa'"
                 (change)="filtroTipo.set('empresa'); reiniciarPaginacion()">
          Empresas
        </label>
      </div>
    </div>

    @if (error()) {
      <div class="alerta alerta-error">{{ error() }}</div>
    }
    @if (mensajeExito()) {
      <div class="alerta alerta-exito">{{ mensajeExito() }}</div>
    }

    @if (cargando()) {
      <div class="cargando"><span class="spinner"></span> Cargando...</div>
    } @else {
      <div class="tabla-contenedor">
        <table class="tabla-clientes">
          <thead>
            <tr>
              <th>#</th><th>Nombre</th><th>Teléfono</th>
              <th class="col-oculta-movil">Email</th><th class="col-oculta-movil">Tipo</th>
            </tr>
          </thead>
          <tbody>
            @for (c of itemsMostrados(); track c.clienteId) {
              <tr [class.seleccionada]="seleccionado()?.clienteId === c.clienteId"
                  (click)="seleccionar(c)" (dblclick)="verDetalle(c)">
                <td>{{ c.clienteId }}</td>
                <td>{{ c.nombreCliente }}</td>
                <td>{{ c.telefono1 }}</td>
                <td class="col-oculta-movil">{{ c.email ?? '—' }}</td>
                <td class="col-oculta-movil">{{ c.esEmpresa ? 'Empresa' : 'Persona' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="celda-vacia">No hay clientes registrados.</td></tr>
            }
          </tbody>
        </table>

        @if (itemsFiltrados().length > tamanoPagina()) {
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
          {{ modo() === 'ver' ? 'Detalle de cliente' : modo() === 'editar' ? 'Modificar cliente' : 'Nuevo cliente' }}
        </h3>

        @if (modo() === 'ver') {
          <div class="banner-solo-lectura">&#128274; Vista de solo lectura — haga clic en Modificar para editar</div>
        }
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        <div class="form-grid" [class.solo-lectura]="modo() === 'ver'">
          <div class="form-grupo">
            <label class="campo-requerido">Nombre</label>
            <input type="text" [(ngModel)]="form.nombreCliente" placeholder="Nombre completo">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Teléfono principal</label>
            <input type="text" [(ngModel)]="form.telefono1" placeholder="Ej: 8888-1234">
          </div>
          <div class="form-grupo">
            <label>Teléfono secundario</label>
            <input type="text" [(ngModel)]="form.telefono2" placeholder="Opcional">
          </div>
          <div class="form-grupo">
            <label>Email</label>
            <input type="email" [(ngModel)]="form.email" placeholder="correo@ejemplo.com">
          </div>
          <div class="checkbox-grupo">
            <input type="checkbox" id="esEmpresa" [(ngModel)]="form.esEmpresa">
            <label for="esEmpresa">Es empresa</label>
          </div>
        </div>

        <div class="form-acciones">
          @if (modo() === 'ver') {
            <button class="btn btn-secundario" (click)="cancelar()">Cerrar</button>
          } @else {
            <button class="btn btn-secundario" (click)="cancelar()">Cancelar</button>
            <button class="btn btn-primario"   [disabled]="guardando()" (click)="guardar()">
              @if (guardando()) { Guardando... } @else { Aceptar }
            </button>
          }
        </div>
      </div>
    }
  `
})
export class ClientesComponent implements OnInit, OnDestroy {
  private readonly svc         = inject(ClientesService);
  private readonly asistenteVoz = inject(AsistenteVozService);

  cargando     = signal(false);
  guardando    = signal(false);
  error        = signal<string | null>(null);
  errorForm    = signal<string | null>(null);
  mensajeExito = signal<string | null>(null);

  private todos = signal<ClienteDto[]>([]);
  textoBusqueda = signal('');
  filtroTipo    = signal<'todos' | 'persona' | 'empresa'>('todos');
  paginaActual  = signal(0);
  seleccionado  = signal<ClienteDto | null>(null);
  mostrarFormulario = signal(false);
  modo          = signal<Modo>('crear');

  private readonly mediaMovil = window.matchMedia(CONSULTA_MOVIL);
  esMovil       = signal(this.mediaMovil.matches);
  private readonly onCambioMedia = (e: MediaQueryListEvent) => {
    this.esMovil.set(e.matches);
    this.paginaActual.set(0);
  };

  form = { nombreCliente: '', telefono1: '', telefono2: '', email: '', esEmpresa: false };

  itemsFiltrados = computed(() => {
    const f    = this.textoBusqueda().toLowerCase().trim();
    const tipo = this.filtroTipo();
    return this.todos().filter(c => {
      const coincideNombre = !f || c.nombreCliente.toLowerCase().includes(f);
      const coincideTipo   = tipo === 'todos'
        || (tipo === 'empresa' && c.esEmpresa)
        || (tipo === 'persona' && !c.esEmpresa);
      return coincideNombre && coincideTipo;
    });
  });

  tamanoPagina = computed(() => this.esMovil() ? TAMANO_PAGINA_MOVIL : POR_PAGINA);

  paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / this.tamanoPagina()));

  itemsMostrados = computed(() => {
    const inicio = this.paginaActual() * this.tamanoPagina();
    return this.itemsFiltrados().slice(inicio, inicio + this.tamanoPagina());
  });

  ngOnInit() {
    this.cargar();
    this.mediaMovil.addEventListener('change', this.onCambioMedia);
    this.aplicarAccionVoz();
  }

  private aplicarAccionVoz(): void {
    const accion = this.asistenteVoz.tomarAccionPendiente('clientes');
    if (!accion) return;

    if (accion.tipo === 'abrir-crear') {
      this.abrirFormulario('crear');
    } else if (accion.tipo === 'prellenar-cliente') {
      this.abrirFormulario('crear');
      const datos = accion.datos;
      if (datos.nombreCliente) this.form.nombreCliente = datos.nombreCliente;
      if (datos.telefono1)     this.form.telefono1     = datos.telefono1;
      if (datos.telefono2)     this.form.telefono2     = datos.telefono2;
      if (datos.email)         this.form.email         = datos.email;
      if (datos.esEmpresa !== null) this.form.esEmpresa = datos.esEmpresa;
    } else if (accion.tipo === 'buscar') {
      this.textoBusqueda.set(accion.termino);
    }
  }

  ngOnDestroy() {
    this.mediaMovil.removeEventListener('change', this.onCambioMedia);
  }

  reiniciarPaginacion(): void {
    this.paginaActual.set(0);
  }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.svc.obtener().subscribe({
      next:  data => { this.todos.set(data); this.cargando.set(false); },
      error: ()   => { this.error.set('Error al cargar clientes.'); this.cargando.set(false); }
    });
  }

  seleccionar(c: ClienteDto): void {
    this.error.set(null);
    this.mensajeExito.set(null);
    this.seleccionado.set(this.seleccionado()?.clienteId === c.clienteId ? null : c);
  }

  verDetalle(c: ClienteDto): void {
    this.seleccionado.set(c);
    this.abrirFormulario('ver');
  }

  abrirFormulario(m: Modo): void {
    this.modo.set(m);
    this.errorForm.set(null);
    this.mensajeExito.set(null);
    if ((m === 'ver' || m === 'editar') && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.form = { nombreCliente: s.nombreCliente, telefono1: s.telefono1,
                    telefono2: s.telefono2 ?? '', email: s.email ?? '', esEmpresa: s.esEmpresa };
    } else {
      this.form = { nombreCliente: '', telefono1: '', telefono2: '', email: '', esEmpresa: false };
    }
    this.mostrarFormulario.set(true);
  }

  cancelar(): void {
    this.mostrarFormulario.set(false);
    if (this.modo() === 'crear') this.seleccionado.set(null);
  }

  guardar(): void {
    if (!this.form.nombreCliente.trim()) { this.errorForm.set('El nombre es requerido.'); return; }
    if (!this.form.telefono1.trim())     { this.errorForm.set('El teléfono principal es requerido.'); return; }

    this.guardando.set(true);
    this.errorForm.set(null);

    const body = {
      nombreCliente: this.form.nombreCliente.trim(),
      telefono1:     this.form.telefono1.trim(),
      telefono2:     this.form.telefono2.trim() || null,
      email:         this.form.email.trim() || null,
      esEmpresa:     this.form.esEmpresa
    };

    const onExito = () => { this.guardando.set(false); this.mostrarFormulario.set(false); this.cargar(); };
    const onError = (err: { error?: string }) => {
      this.guardando.set(false);
      this.errorForm.set(err.error ?? 'Error al guardar.');
    };

    if (this.modo() === 'editar') {
      this.svc.actualizar(this.seleccionado()!.clienteId, body).subscribe({ next: onExito, error: onError });
    } else {
      this.svc.crear(body).subscribe({ next: onExito, error: onError });
    }
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar cliente "${s.nombreCliente}"?`)) return;

    this.svc.eliminar(s.clienteId).subscribe({
      next: () => {
        this.seleccionado.set(null);
        this.mostrarFormulario.set(false);
        this.mensajeExito.set('Cliente eliminado correctamente.');
        this.cargar();
      },
      error: err => this.error.set(err.error ?? 'No se puede eliminar este cliente.')
    });
  }
}

import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VehiculosService, VehiculoRequest } from '../../services/vehiculos.service';
import { ClientesService } from '../../services/clientes.service';
import { MarcasModelosService } from '../../services/marcas-modelos.service';
import { VehiculoDto } from '../../models/vehiculo.model';
import { ClienteDto } from '../../models/cliente.model';
import { MarcaModeloDto } from '../../models/marca-modelo.model';
import { DatosVehiculoExtraidosDto } from '../../models/datos-vehiculo-extraidos.model';
import { comprimirImagen } from '../../utils/imagen.util';
import { AsistenteVozService } from '../../core/voz/asistente-voz.service';
import { VehiculoVozDto } from '../../models/interpretacion-voz.model';

const POR_PAGINA = 25;
const TAMANO_PAGINA_MOVIL = 15;
const CONSULTA_MOVIL = '(max-width: 768px)';
type Modo = 'ver' | 'crear' | 'editar';

@Component({
  standalone: true,
  selector: 'app-vehiculos',
  imports: [FormsModule],
  template: `
    <div class="pagina-header">
      <h2>Vehículos</h2>
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
      <input type="text" placeholder="Buscar por placa o nombre del cliente..."
             [ngModel]="textoBusqueda()"
             (ngModelChange)="textoBusqueda.set($event); reiniciarPaginacion()">
      <div class="filtro-tipo">
        <label>
          <input type="radio" name="filtroTransmision" value="todos"
                 [checked]="filtroTransmision() === 'todos'"
                 (change)="filtroTransmision.set('todos'); reiniciarPaginacion()">
          Todos
        </label>
        <label>
          <input type="radio" name="filtroTransmision" value="manual"
                 [checked]="filtroTransmision() === 'manual'"
                 (change)="filtroTransmision.set('manual'); reiniciarPaginacion()">
          Manual
        </label>
        <label>
          <input type="radio" name="filtroTransmision" value="automatico"
                 [checked]="filtroTransmision() === 'automatico'"
                 (change)="filtroTransmision.set('automatico'); reiniciarPaginacion()">
          Automático
        </label>
      </div>
    </div>

    @if (error()) { <div class="alerta alerta-error">{{ error() }}</div> }

    @if (cargando()) {
      <div class="cargando"><span class="spinner"></span> Cargando...</div>
    } @else {
      <div class="tabla-contenedor">
        <table class="tabla-vehiculos">
          <thead>
            <tr>
              <th>#</th><th>Placa</th><th>Marca</th><th class="col-oculta-movil">Modelo</th>
              <th class="col-oculta-movil">Año</th><th class="col-oculta-movil">Transmisión</th><th>Cliente</th>
            </tr>
          </thead>
          <tbody>
            @for (v of itemsMostrados(); track v.vehiculoId) {
              <tr [class.seleccionada]="seleccionado()?.vehiculoId === v.vehiculoId"
                  (click)="seleccionar(v)" (dblclick)="verDetalle(v)">
                <td>{{ v.vehiculoId }}</td>
                <td>{{ v.placa ?? '—' }}</td>
                <td>{{ v.marca }}</td>
                <td class="col-oculta-movil">{{ v.modelo ?? '—' }}</td>
                <td class="col-oculta-movil">{{ v.annio ?? '—' }}</td>
                <td class="col-oculta-movil">{{ v.esAutomatico ? 'Automático' : 'Manual' }}</td>
                <td>{{ v.nombreCliente }}</td>
              </tr>
            } @empty {
              <tr><td colspan="7" class="celda-vacia">No hay vehículos registrados.</td></tr>
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
          {{ modo() === 'ver' ? 'Detalle de vehículo' : modo() === 'editar' ? 'Modificar vehículo' : 'Nuevo vehículo' }}
        </h3>

        @if (modo() === 'ver') {
          <div class="banner-solo-lectura">&#128274; Vista de solo lectura — haga clic en Modificar para editar</div>
        }
        @if (errorForm()) { <div class="alerta alerta-error">{{ errorForm() }}</div> }

        @if (modo() === 'crear') {
          <div class="escaneo-ia">
            <input #inputFotoTarjeta type="file" accept="image/*" capture="environment" hidden
                   (change)="onFotoSeleccionada($event)">
            <button type="button" class="btn btn-secundario" [disabled]="escaneando()"
                    (click)="inputFotoTarjeta.click()">
              @if (escaneando()) { &#128269;&#128196; Detectando información del vehículo... } @else { &#128269;&#128196; Escanear tarjeta de circulación }
            </button>
            @if (mensajeEscaneo()) { <div class="alerta alerta-info">{{ mensajeEscaneo() }}</div> }
            @if (errorEscaneo())   { <div class="alerta alerta-error">{{ errorEscaneo() }}</div> }
          </div>
        }

        <div class="form-grid" [class.solo-lectura]="modo() === 'ver'">
          <div class="form-grupo">
            <label class="campo-requerido">Cliente</label>
            <input type="text" list="lista-clientes-vehiculo" [value]="clienteTexto()"
                   (input)="onClienteInput($any($event.target).value)" placeholder="Escriba el nombre del cliente...">
            <datalist id="lista-clientes-vehiculo">
              @for (c of clientesSugeridos(); track c) { <option [value]="c"></option> }
            </datalist>
          </div>
          <div class="form-grupo">
            <label>Placa</label>
            <input type="text" [(ngModel)]="form.placa" placeholder="Ej: ABC-123">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Marca</label>
            <input type="text" list="lista-marcas-vehiculo" [value]="marca()"
                   (input)="onMarcaInput($any($event.target).value)" placeholder="Ej: Toyota">
            <datalist id="lista-marcas-vehiculo">
              @for (m of marcasSugeridas(); track m) { <option [value]="m"></option> }
            </datalist>
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Modelo</label>
            <input type="text" list="lista-modelos-vehiculo" [value]="modelo()"
                   (input)="onModeloInput($any($event.target).value)" placeholder="Ej: Corolla">
            <datalist id="lista-modelos-vehiculo">
              @for (m of modelosSugeridos(); track m) { <option [value]="m"></option> }
            </datalist>
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Año</label>
            <input type="text" list="lista-annios-vehiculo" [value]="annio()"
                   (input)="annio.set($any($event.target).value)" placeholder="Ej: 2020">
            <datalist id="lista-annios-vehiculo">
              @for (a of anniosSugeridos(); track a) { <option [value]="a"></option> }
            </datalist>
          </div>
          <div class="form-grupo">
            <label>VIN</label>
            <input type="text" [(ngModel)]="form.vin" placeholder="Numero de chasis">
          </div>
          <div class="form-grupo">
            <label>Motor</label>
            <input type="text" [(ngModel)]="form.motor" placeholder="Ej: 1.8L 4 cilindros">
          </div>
          <div class="form-grupo">
            <label class="campo-requerido">Transmisión</label>
            <div class="radio-grupo">
              <label>
                <input type="radio" name="transmision" [value]="false" [(ngModel)]="form.esAutomatico">
                Manual
              </label>
              <label>
                <input type="radio" name="transmision" [value]="true" [(ngModel)]="form.esAutomatico">
                Automática
              </label>
            </div>
          </div>
          <div class="form-grupo" style="grid-column: 1 / -1">
            <label class="campo-requerido">Detalles de carroceria</label>
            <textarea [(ngModel)]="form.detallesCarroceria" rows="3"
                      placeholder="Rayones, golpes u otros desperfectos observados al ingreso..."></textarea>
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
export class VehiculosComponent implements OnInit, OnDestroy {
  private readonly svc          = inject(VehiculosService);
  private readonly clientesSvc  = inject(ClientesService);
  private readonly marcasSvc    = inject(MarcasModelosService);
  private readonly asistenteVoz = inject(AsistenteVozService);

  cargando          = signal(false);
  guardando         = signal(false);
  error             = signal<string | null>(null);
  errorForm         = signal<string | null>(null);
  private todos     = signal<VehiculoDto[]>([]);
  clientes          = signal<ClienteDto[]>([]);
  textoBusqueda     = signal('');
  filtroTransmision = signal<'todos' | 'manual' | 'automatico'>('todos');
  paginaActual      = signal(0);
  seleccionado      = signal<VehiculoDto | null>(null);
  mostrarFormulario = signal(false);
  modo              = signal<Modo>('crear');

  private readonly mediaMovil = window.matchMedia(CONSULTA_MOVIL);
  esMovil       = signal(this.mediaMovil.matches);
  private readonly onCambioMedia = (e: MediaQueryListEvent) => {
    this.esMovil.set(e.matches);
    this.paginaActual.set(0);
  };

  private catalogoMarcaModelo = signal<MarcaModeloDto[]>([]);
  marca  = signal('');
  modelo = signal('');
  annio  = signal('');
  clienteTexto = signal('');

  escaneando     = signal(false);
  mensajeEscaneo = signal<string | null>(null);
  errorEscaneo   = signal<string | null>(null);

  marcasSugeridas = computed(() => {
    const marcas = this.catalogoMarcaModelo()
      .map(mm => mm.marca)
      .filter((m): m is string => !!m);
    return [...new Set(marcas)].sort();
  });

  modelosSugeridos = computed(() => {
    const marcaActual = this.marca().trim().toLowerCase();
    if (!marcaActual) return [];
    const modelos = this.catalogoMarcaModelo()
      .filter(mm => (mm.marca ?? '').toLowerCase() === marcaActual)
      .map(mm => mm.modelo)
      .filter((m): m is string => !!m);
    return [...new Set(modelos)].sort();
  });

  clientesSugeridos = computed(() => {
    return [...new Set(this.clientes().map(c => c.nombreCliente))].sort();
  });

  anniosSugeridos = computed(() => {
    const marcaActual  = this.marca().trim().toLowerCase();
    const modeloActual = this.modelo().trim().toLowerCase();
    if (!marcaActual || !modeloActual) return [];
    const annios = this.catalogoMarcaModelo()
      .filter(mm => (mm.marca ?? '').toLowerCase() === marcaActual
                 && (mm.modelo ?? '').toLowerCase() === modeloActual)
      .map(mm => mm.annio)
      .filter((a): a is number => a != null);
    return [...new Set(annios)].sort((a, b) => a - b).map(a => String(a));
  });

  form: { clienteId: number; placa: string; vin: string;
          motor: string; esAutomatico: boolean; detallesCarroceria: string } = {
    clienteId: 0, placa: '', vin: '',
    motor: '', esAutomatico: false, detallesCarroceria: ''
  };

  onMarcaInput(valor: string): void {
    this.marca.set(valor);
  }

  onModeloInput(valor: string): void {
    this.modelo.set(valor);
  }

  onClienteInput(valor: string): void {
    this.clienteTexto.set(valor);
    const encontrado = this.clientes()
      .find(c => c.nombreCliente.toLowerCase() === valor.toLowerCase().trim());
    this.form.clienteId = encontrado ? encontrado.clienteId : 0;
  }

  itemsFiltrados = computed(() => {
    const f  = this.textoBusqueda().toLowerCase().trim();
    const tr = this.filtroTransmision();
    return this.todos().filter(v => {
      const coincideTexto = !f ||
        (v.placa ?? '').toLowerCase().includes(f) ||
        v.nombreCliente.toLowerCase().includes(f) ||
        v.marca.toLowerCase().includes(f) ||
        (v.modelo ?? '').toLowerCase().includes(f);
      const coincideTransmision = tr === 'todos'
        || (tr === 'automatico' && v.esAutomatico)
        || (tr === 'manual' && !v.esAutomatico);
      return coincideTexto && coincideTransmision;
    });
  });

  tamanoPagina   = computed(() => this.esMovil() ? TAMANO_PAGINA_MOVIL : POR_PAGINA);
  paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / this.tamanoPagina()));

  itemsMostrados = computed(() => {
    const ini = this.paginaActual() * this.tamanoPagina();
    return this.itemsFiltrados().slice(ini, ini + this.tamanoPagina());
  });

  ngOnInit() {
    this.cargar();
    this.mediaMovil.addEventListener('change', this.onCambioMedia);
    this.aplicarAccionVoz();
  }

  private aplicarAccionVoz(): void {
    const accion = this.asistenteVoz.tomarAccionPendiente('vehiculos');
    if (!accion) return;

    if (accion.tipo === 'abrir-crear') {
      this.abrirFormulario('crear');
    } else if (accion.tipo === 'prellenar-vehiculo') {
      this.abrirFormulario('crear');
      this.aplicarDatosVoz(accion.datos);
    } else if (accion.tipo === 'buscar') {
      this.textoBusqueda.set(accion.termino);
    }
  }

  private aplicarDatosVoz(datos: VehiculoVozDto): void {
    if (datos.marca)  this.marca.set(datos.marca);
    if (datos.modelo) this.modelo.set(datos.modelo);
    if (datos.annio)  this.annio.set(String(datos.annio));
    if (datos.vin)    this.form.vin   = datos.vin;
    if (datos.placa)  this.form.placa = datos.placa;
    if (datos.motor)  this.form.motor = datos.motor;
    if (datos.esAutomatico !== null) this.form.esAutomatico = datos.esAutomatico;

    if (datos.nombreClienteBuscado) {
      const nombre = datos.nombreClienteBuscado;
      this.clientesSvc.obtener().subscribe({
        next: data => {
          this.clientes.set(data);
          this.onClienteInput(nombre);
        }
      });
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
      error: ()   => { this.error.set('Error al cargar vehículos.'); this.cargando.set(false); }
    });
  }

  seleccionar(v: VehiculoDto): void {
    this.error.set(null);
    this.seleccionado.set(this.seleccionado()?.vehiculoId === v.vehiculoId ? null : v);
  }

  verDetalle(v: VehiculoDto): void {
    this.seleccionado.set(v);
    this.abrirFormulario('ver');
  }

  abrirFormulario(m: Modo): void {
    this.modo.set(m);
    this.errorForm.set(null);
    this.escaneando.set(false);
    this.mensajeEscaneo.set(null);
    this.errorEscaneo.set(null);
    if ((m === 'ver' || m === 'editar') && this.seleccionado()) {
      const s = this.seleccionado()!;
      this.form = {
        clienteId: s.clienteId, placa: s.placa ?? '',
        vin: s.vin ?? '', motor: s.motor ?? '',
        esAutomatico: s.esAutomatico, detallesCarroceria: s.detallesCarroceria
      };
      this.marca.set(s.marca);
      this.modelo.set(s.modelo ?? '');
      this.annio.set(s.annio != null ? String(s.annio) : '');
      this.clienteTexto.set(s.nombreCliente);
    } else {
      this.form = {
        clienteId: 0, placa: '', vin: '',
        motor: '', esAutomatico: false, detallesCarroceria: ''
      };
      this.marca.set('');
      this.modelo.set('');
      this.annio.set(String(new Date().getFullYear()));
      this.clienteTexto.set('');
    }
    this.cargarClientes();
    this.cargarCatalogoMarcaModelo();
    this.mostrarFormulario.set(true);
  }

  cancelar(): void {
    this.mostrarFormulario.set(false);
    if (this.modo() === 'crear') this.seleccionado.set(null);
  }

  cargarClientes(): void {
    if (this.clientes().length > 0) return;
    this.clientesSvc.obtener().subscribe({ next: data => this.clientes.set(data) });
  }

  async onFotoSeleccionada(event: Event): Promise<void> {
    const input  = event.target as HTMLInputElement;
    const activo = input.files?.[0];
    input.value = ''; // permite volver a seleccionar el mismo archivo

    if (!activo) return;

    this.escaneando.set(true);
    this.mensajeEscaneo.set(null);
    this.errorEscaneo.set(null);

    try {
      const comprimida = await comprimirImagen(activo);
      this.svc.escanearTarjetaCirculacion(comprimida).subscribe({
        next: datos => {
          this.escaneando.set(false);
          this.aplicarDatosEscaneados(datos);
          this.mensajeEscaneo.set('Datos extraídos de la foto. Revise y corrija antes de guardar.');
        },
        error: (err: { error?: string }) => {
          this.escaneando.set(false);
          this.errorEscaneo.set(
            err.error ?? 'No se pudo leer la foto. Complete los datos manualmente.');
        }
      });
    } catch {
      this.escaneando.set(false);
      this.errorEscaneo.set('No se pudo procesar la foto. Intente de nuevo o complete manualmente.');
    }
  }

  private aplicarDatosEscaneados(datos: DatosVehiculoExtraidosDto): void {
    // Solo se sobreescriben los campos que la IA pudo leer (no-null); nunca se
    // pisa con vacio lo que el mecanico ya haya escrito a mano.
    if (datos.marca)  this.marca.set(datos.marca);
    if (datos.modelo) this.modelo.set(datos.modelo);
    if (datos.annio)  this.annio.set(String(datos.annio));
    if (datos.vin)    this.form.vin   = datos.vin;
    if (datos.placa)  this.form.placa = datos.placa;
    if (datos.motor)  this.form.motor = datos.motor;
  }

  cargarCatalogoMarcaModelo(): void {
    if (this.catalogoMarcaModelo().length > 0) return;
    this.marcasSvc.obtener().subscribe({ next: data => this.catalogoMarcaModelo.set(data) });
  }

  guardar(): void {
    const annioNumero = Number(this.annio());
    if (!this.form.clienteId)                { this.errorForm.set('Escriba y seleccione un cliente valido de la lista.'); return; }
    if (!this.marca().trim())                { this.errorForm.set('La marca es requerida.'); return; }
    if (!this.modelo().trim())               { this.errorForm.set('El modelo es requerido.'); return; }
    if (!annioNumero || annioNumero < 1900 || annioNumero > 2100)
      { this.errorForm.set('El anno debe estar entre 1900 y 2100.'); return; }
    if (!this.form.detallesCarroceria.trim()) { this.errorForm.set('Los detalles de carroceria son requeridos.'); return; }

    this.guardando.set(true);
    this.errorForm.set(null);

    const body: VehiculoRequest = {
      clienteId:          this.form.clienteId,
      placa:              this.form.placa.trim() || null,
      marca:              this.marca().trim(),
      modelo:             this.modelo().trim(),
      vin:                this.form.vin.trim() || null,
      annio:              annioNumero,
      motor:              this.form.motor.trim() || null,
      esAutomatico:       this.form.esAutomatico,
      detallesCarroceria: this.form.detallesCarroceria.trim()
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
      this.svc.actualizar(this.seleccionado()!.vehiculoId, body)
        .subscribe({ next: onExito, error: onError });
    } else {
      this.svc.crear(body).subscribe({ next: onExito, error: onError });
    }
  }

  confirmarEliminar(): void {
    const s = this.seleccionado();
    if (!s) return;
    if (!confirm(`¿Eliminar el vehículo ${s.marca} ${s.modelo ?? ''} (${s.placa ?? 'sin placa'})?`)) return;

    this.svc.eliminar(s.vehiculoId).subscribe({
      next:  () => { this.seleccionado.set(null); this.mostrarFormulario.set(false); this.cargar(); },
      error: (err: { error?: string }) => this.error.set(err.error ?? 'No se puede eliminar este vehículo.')
    });
  }
}

import {
  Component, inject, signal, computed, OnInit
} from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { FacturaDto } from '../../models/factura.model';
import { ReparacionDto } from '../../models/reparacion.model';
import { RepuestoDto } from '../../models/repuesto.model';
import { TipoReparacionDto } from '../../models/tipo-reparacion.model';
import { HistoricoRespuestoDto } from '../../models/historico-repuesto.model';
import { FacturasService } from '../../services/facturas.service';
import { ReparacionesService } from '../../services/reparaciones.service';
import { RepuestosService } from '../../services/repuestos.service';
import { TiposReparacionService } from '../../services/tipos-reparacion.service';
import { HistoricoRepuestosService } from '../../services/historico-repuestos.service';

type Modo = 'ver' | 'editar';

@Component({
  selector: 'app-facturas',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, DecimalPipe],
  template: `
<div class="pantalla-contenedor">

  <div class="pantalla-encabezado">
    <h2 class="pantalla-titulo">Facturas</h2>
    @if (!mostrarFormulario()) {
      <div class="acciones-barra">
        <button class="btn-accion" (click)="abrirFormulario('editar')" [disabled]="!seleccionada() || guardando()">Modificar</button>
        <button class="btn-accion btn-peligro" (click)="confirmarEliminar()" [disabled]="!seleccionada() || guardando()">Eliminar</button>
      </div>
    }
  </div>

  @if (!mostrarFormulario()) {
    <div class="filtros-barra">
      <input class="filtro-input" type="text" placeholder="Buscar por placa, cliente o N° factura..."
             [value]="textoBusqueda()"
             (input)="textoBusqueda.set($any($event.target).value)" />
      <div class="filtro-estado-grupo">
        <button [class]="'btn-filtro-estado' + (filtroEstado() === 0 ? ' activo' : '')" (click)="filtroEstado.set(0)">Todos</button>
        <button [class]="'btn-filtro-estado' + (filtroEstado() === 1 ? ' activo' : '')" (click)="filtroEstado.set(1)">Cotizacion</button>
        <button [class]="'btn-filtro-estado' + (filtroEstado() === 2 ? ' activo' : '')" (click)="filtroEstado.set(2)">Pendiente</button>
        <button [class]="'btn-filtro-estado' + (filtroEstado() === 3 ? ' activo' : '')" (click)="filtroEstado.set(3)">Pagada</button>
      </div>
    </div>

    <div class="tabla-contenedor">
      @if (cargando()) {
        <p class="texto-cargando">Cargando facturas...</p>
      } @else if (facturasFiltradas().length === 0) {
        <p class="texto-vacio">No hay facturas que coincidan con el filtro.</p>
      } @else {
        <table class="tabla-datos tabla-facturas">
          <thead>
            <tr>
              <th>#</th><th class="col-oculta-movil">Fecha</th><th>Placa</th><th>Vehículo</th>
              <th>Cliente</th><th class="col-monto col-oculta-movil">Repuestos</th>
              <th class="col-monto col-oculta-movil">Reparaciones</th><th class="col-monto col-oculta-movil">Total</th><th class="col-oculta-movil">Estado</th>
            </tr>
          </thead>
          <tbody>
            @for (f of facturasFiltradas(); track f.facturaId) {
              <tr [class.fila-seleccionada]="seleccionada()?.facturaId === f.facturaId"
                  (click)="seleccionar(f)" (dblclick)="verDetalle(f)">
                <td>{{ f.facturaId }}</td>
                <td class="col-oculta-movil">{{ f.fecha | date:'dd/MM/yyyy' }}</td>
                <td>{{ f.placa || '—' }}</td>
                <td>{{ f.marca }} {{ f.modelo }}</td>
                <td>{{ f.nombreCliente }}</td>
                <td class="col-monto col-oculta-movil">&#x20A1;{{ f.totalRepuestos | number:'1.2-2' }}</td>
                <td class="col-monto col-oculta-movil">&#x20A1;{{ f.totalReparaciones | number:'1.2-2' }}</td>
                <td class="col-monto col-oculta-movil"><strong>&#x20A1;{{ f.total | number:'1.2-2' }}</strong></td>
                <td class="col-oculta-movil"><span [class]="'badge badge-f-' + f.estadoFacturaId">{{ f.estadoFacturaDescripcion }}</span></td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  }

  @if (mostrarFormulario()) {
    <div class="formulario-panel">

      <!-- ───── MODO VER ───── -->
      @if (modo() === 'ver') {
        <div class="formulario-titulo-fila">
          <h3 class="formulario-titulo">Detalle de Factura #{{ seleccionada()?.facturaId }}</h3>
          <div class="acciones-factura-pdf">
            <button class="btn-accion" (click)="abrirPdf()" [disabled]="abriendoPdf()">
              {{ abriendoPdf() ? 'Abriendo...' : 'Abrir PDF' }}
            </button>
            <button class="btn-accion" (click)="enviarFactura()" [disabled]="enviandoFactura()">
              {{ enviandoFactura() ? 'Enviando...' : 'Enviar factura' }}
            </button>
          </div>
        </div>

        <div class="banner-solo-lectura">&#x1F512; Solo lectura &mdash; presione Modificar para editar</div>

        <div class="form-grid solo-lectura">
          <div class="form-grupo">
            <label>Factura #</label>
            <input type="text" [value]="seleccionada()?.facturaId" readonly />
          </div>
          <div class="form-grupo">
            <label>Fecha</label>
            <input type="text" [value]="seleccionada()?.fecha | date:'dd/MM/yyyy'" readonly />
          </div>
          <div class="form-grupo"><label>Estado</label>
            <input type="text" [value]="seleccionada()?.estadoFacturaDescripcion" readonly />
          </div>
          <div class="form-grupo">
            <label>Placa</label>
            <input type="text" [value]="seleccionada()?.placa || '—'" readonly />
          </div>
          <div class="form-grupo">
            <label>Vehículo</label>
            <input type="text" [value]="(seleccionada()?.marca ?? '') + ' ' + (seleccionada()?.modelo ?? '')" readonly />
          </div>
          <div class="form-grupo form-grupo-ancho">
            <label>Cliente</label>
            <input type="text" [value]="seleccionada()?.nombreCliente" readonly />
          </div>
          <div class="form-grupo form-grupo-ancho">
            <label>Descripcion General</label>
            <textarea [value]="seleccionada()?.descripcionGeneral || ''" readonly rows="2"></textarea>
          </div>
        </div>

      }

      <!-- ───── MODO EDITAR: encabezado + campos editables ───── -->
      @if (modo() === 'editar') {
        <div class="formulario-titulo-fila">
          <h3 class="formulario-titulo">Modificar Factura #{{ seleccionada()?.facturaId }}</h3>
          <div class="acciones-factura-pdf">
            <button class="btn-accion" (click)="abrirPdf()" [disabled]="abriendoPdf()">
              {{ abriendoPdf() ? 'Abriendo...' : 'Abrir PDF' }}
            </button>
            <button class="btn-accion" (click)="enviarFactura()" [disabled]="enviandoFactura()">
              {{ enviandoFactura() ? 'Enviando...' : 'Enviar factura' }}
            </button>
          </div>
        </div>

        <div class="form-grid">
          <div class="form-grupo"><label>Factura #</label>
            <input type="text" [value]="seleccionada()?.facturaId" readonly class="campo-readonly" />
          </div>
          <div class="form-grupo">
            <label>Fecha *</label>
            <input type="date" [value]="fecha()" (input)="fecha.set($any($event.target).value)" />
          </div>
          <div class="form-grupo">
            <label>Estado *</label>
            <select [value]="estadoFacturaId()" (change)="estadoFacturaId.set(+$any($event.target).value)">
              <option value="1">Cotizacion</option>
              <option value="2">Pendiente</option>
              <option value="3">Pagada</option>
            </select>
          </div>
          <div class="form-grupo"><label>Placa</label>
            <input type="text" [value]="seleccionada()?.placa || '—'" readonly class="campo-readonly" />
          </div>
          <div class="form-grupo"><label>Vehículo</label>
            <input type="text" [value]="(seleccionada()?.marca ?? '') + ' ' + (seleccionada()?.modelo ?? '')" readonly class="campo-readonly" />
          </div>
          <div class="form-grupo form-grupo-ancho"><label>Cliente</label>
            <input type="text" [value]="seleccionada()?.nombreCliente" readonly class="campo-readonly" />
          </div>
          <div class="form-grupo form-grupo-ancho">
            <label>Descripcion General</label>
            <textarea [value]="descripcionGeneral()"
                      (input)="descripcionGeneral.set($any($event.target).value)"
                      rows="2" placeholder="Informacion adicional de la factura"></textarea>
          </div>
        </div>
      }

      <!-- ── Reparaciones y Repuestos: columnas simultaneas, visibles en modo ver Y editar ── -->
      @if (seleccionada() && (modo() === 'ver' || modo() === 'editar')) {
        <div class="detalle-listas seccion-detalle-factura">

          <!-- ── Columna Reparaciones ── -->
          <div class="detalle-lista-bloque">
            <h5 class="detalle-lista-titulo">Reparaciones ({{ reparaciones().length }})</h5>

            @if (cargandoReparaciones()) {
              <p class="texto-cargando">Cargando...</p>
            } @else {
              @if (modo() === 'editar' && seleccionada()?.estadoFacturaId !== 3) {
                @if (!mostrarFormReparacion() && !repEditando()) {
                  <div class="tab-accion-barra">
                    <button class="btn-accion btn-sm" (click)="toggleAgregarReparacion()">+ Agregar</button>
                    <button class="btn-accion btn-sm" [disabled]="!reparacionSeleccionada()"
                            (click)="editarReparacion(reparacionSeleccionada()!)">Modificar</button>
                    <button class="btn-accion btn-sm btn-peligro" [disabled]="!reparacionSeleccionada()"
                            (click)="eliminarReparacion(reparacionSeleccionada()!.reparacionId)">Eliminar</button>
                  </div>
                }

                <!-- Formulario agregar reparacion -->
                @if (mostrarFormReparacion()) {
                  <!-- Panel busqueda catalogo -->
                  <div class="panel-ref">
                    <button class="btn-ref-toggle" (click)="mostrarCatalogo.set(!mostrarCatalogo())">
                      &#x1F50D; {{ mostrarCatalogo() ? 'Ocultar catalogo' : 'Buscar en catalogo de reparaciones' }}
                    </button>
                    @if (mostrarCatalogo()) {
                      <div class="ref-busqueda">
                        <div class="ref-busqueda-barra">
                          <input type="text" class="filtro-input" placeholder="Escriba para buscar..."
                                 [value]="catFiltro()"
                                 (input)="onCatFiltroInput($any($event.target).value)" />
                          @if (catCargando()) { <span class="texto-cargando">Buscando...</span> }
                        </div>
                        @if (catResultados().length > 0) {
                          <table class="tabla-datos tabla-sm tabla-detalle-orden">
                            <thead><tr><th>Tipo de Reparacion</th><th class="col-monto">Costo Base</th></tr></thead>
                            <tbody>
                              @for (t of catResultadosPaginados(); track t.tipoReparacionId) {
                                <tr [class.fila-seleccionada]="catSeleccionado()?.tipoReparacionId === t.tipoReparacionId"
                                    (click)="marcarCatalogo(t)" (dblclick)="seleccionarCatalogo(t)">
                                  <td>{{ t.descripcionReparacion }}</td>
                                  <td class="col-monto">&#x20A1;{{ t.costoBase | number:'1.2-2' }}</td>
                                </tr>
                              }
                            </tbody>
                          </table>
                          @if (catResultados().length > 10) {
                            <div class="paginacion">
                              <button class="btn-secundario btn-sm" [disabled]="catPaginaActual() === 0"
                                      (click)="catPaginaActual.update(p => p - 1)">&#8249;</button>
                              <span>{{ catPaginaActual() + 1 }} / {{ catPaginasTotales() }}</span>
                              <button class="btn-secundario btn-sm" [disabled]="catPaginaActual() >= catPaginasTotales() - 1"
                                      (click)="catPaginaActual.update(p => p + 1)">&#8250;</button>
                            </div>
                          }
                          <div class="ref-busqueda-acciones">
                            <button class="btn-accion btn-sm" [disabled]="!catSeleccionado()" (click)="usarCatalogoSeleccionado()">Usar seleccionado</button>
                          </div>
                        } @else if (!catCargando() && catFiltro()) {
                          <p class="texto-vacio">Sin resultados para "{{ catFiltro() }}".</p>
                        }
                      </div>
                    }
                  </div>

                  <div class="sub-form">
                    <div class="form-grupo form-grupo-ancho">
                      <label>Descripcion *</label>
                      <input type="text" placeholder="Descripcion del trabajo"
                             [value]="subDescReparacion()"
                             (input)="subDescReparacion.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Costo (&#x20A1;)</label>
                      <input type="number" min="0" step="0.01"
                             [value]="subCostoReparacion()"
                             (input)="subCostoReparacion.set(+$any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Duracion (horas)</label>
                      <input type="number" min="1" step="1" placeholder="Opcional"
                             [value]="subDuracionReparacion() ?? ''"
                             (input)="subDuracionReparacion.set($any($event.target).value ? +$any($event.target).value : null)" />
                    </div>
                    <div class="sub-form-acciones">
                      <button class="btn-secundario btn-sm" (click)="toggleAgregarReparacion()">Cancelar</button>
                      <button class="btn-accion btn-sm" (click)="guardarReparacion()" [disabled]="guardando()">
                        {{ guardando() ? 'Guardando...' : 'Guardar' }}
                      </button>
                    </div>
                  </div>
                  @if (errorSubForm()) { <p class="error-form">{{ errorSubForm() }}</p> }
                }

                <!-- Formulario editar reparacion -->
                @if (repEditando()) {
                  <div class="sub-form sub-form-editar">
                    <div class="form-grupo form-grupo-ancho">
                      <label>Descripcion *</label>
                      <input type="text" [value]="repEditDesc()"
                             (input)="repEditDesc.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Costo (&#x20A1;)</label>
                      <input type="number" min="0" step="0.01"
                             [value]="repEditCosto()"
                             (input)="repEditCosto.set(+$any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Duracion (horas)</label>
                      <input type="number" min="1" step="1" placeholder="Opcional"
                             [value]="repEditDuracion() ?? ''"
                             (input)="repEditDuracion.set($any($event.target).value ? +$any($event.target).value : null)" />
                    </div>
                    <div class="sub-form-acciones">
                      <button class="btn-secundario btn-sm" (click)="repEditando.set(null)">Cancelar</button>
                      <button class="btn-accion btn-sm" (click)="guardarEdicionReparacion()" [disabled]="guardando()">
                        {{ guardando() ? 'Guardando...' : 'Guardar cambios' }}
                      </button>
                    </div>
                  </div>
                  @if (errorSubForm()) { <p class="error-form">{{ errorSubForm() }}</p> }
                }
              }

              @if (reparaciones().length === 0) {
                <p class="texto-vacio">Sin reparaciones registradas.</p>
              } @else {
                <table class="tabla-datos tabla-sm tabla-detalle-orden">
                  <thead>
                    <tr>
                      <th>Descripcion</th><th class="col-monto">Costo</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (r of reparaciones(); track r.reparacionId) {
                      <tr [class.fila-editando]="repEditando()?.reparacionId === r.reparacionId"
                          [class.fila-seleccionada]="reparacionSeleccionada()?.reparacionId === r.reparacionId"
                          (click)="seleccionarReparacion(r)">
                        <td>{{ r.descripcionReparacion }}</td>
                        <td class="col-monto">&#x20A1;{{ r.costo | number:'1.2-2' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              }
            }
          </div>

          <!-- ── Columna Repuestos ── -->
          <div class="detalle-lista-bloque">
            <h5 class="detalle-lista-titulo">Repuestos ({{ repuestos().length }})</h5>

            @if (cargandoRepuestos()) {
              <p class="texto-cargando">Cargando...</p>
            } @else {
              @if (modo() === 'editar' && seleccionada()?.estadoFacturaId !== 3) {
                @if (!mostrarFormRepuesto() && !repuEditando()) {
                  <div class="tab-accion-barra">
                    <button class="btn-accion btn-sm" (click)="toggleAgregarRepuesto()">+ Agregar</button>
                    <button class="btn-accion btn-sm" [disabled]="!repuestoSeleccionado()"
                            (click)="editarRepuesto(repuestoSeleccionado()!)">Modificar</button>
                    <button class="btn-accion btn-sm btn-peligro" [disabled]="!repuestoSeleccionado()"
                            (click)="eliminarRepuesto(repuestoSeleccionado()!.repuestoId)">Eliminar</button>
                  </div>
                }

                <!-- Formulario agregar repuesto -->
                @if (mostrarFormRepuesto()) {
                  <!-- Panel busqueda catalogo de repuestos (historico) -->
                  <div class="panel-ref">
                    <button class="btn-ref-toggle" (click)="mostrarHistorico.set(!mostrarHistorico())">
                      &#x1F50D; {{ mostrarHistorico() ? 'Ocultar catalogo' : 'Buscar en catalogo de repuestos' }}
                    </button>
                    @if (mostrarHistorico()) {
                      <div class="ref-busqueda">
                        <div class="ref-busqueda-barra">
                          <input type="text" class="filtro-input" placeholder="Escriba para buscar..."
                                 [value]="histFiltro()"
                                 (input)="onHistFiltroInput($any($event.target).value)" />
                          @if (histCargando()) { <span class="texto-cargando">Buscando...</span> }
                        </div>
                        @if (histResultados().length > 0) {
                          <table class="tabla-datos tabla-sm tabla-detalle-orden">
                            <thead>
                              <tr>
                                <th>Repuesto</th><th class="col-oculta-movil">Marca/Modelo</th><th class="col-oculta-movil">Proveedor</th>
                                <th class="col-monto">Precio Ref.</th>
                              </tr>
                            </thead>
                            <tbody>
                              @for (h of histResultadosPaginados(); track h.respuestoHistoricoId) {
                                <tr [class.fila-seleccionada]="histSeleccionado()?.respuestoHistoricoId === h.respuestoHistoricoId"
                                    (click)="marcarHistorico(h)" (dblclick)="seleccionarHistorico(h)">
                                  <td>{{ h.repuestoDecripcion }}</td>
                                  <td class="col-oculta-movil">{{ h.marca }} {{ h.modelo }}</td>
                                  <td class="col-oculta-movil">{{ h.repuestera }}</td>
                                  <td class="col-monto">&#x20A1;{{ h.precio | number:'1.2-2' }}</td>
                                </tr>
                              }
                            </tbody>
                          </table>
                          @if (histResultados().length > 10) {
                            <div class="paginacion">
                              <button class="btn-secundario btn-sm" [disabled]="histPaginaActual() === 0"
                                      (click)="histPaginaActual.update(p => p - 1)">&#8249;</button>
                              <span>{{ histPaginaActual() + 1 }} / {{ histPaginasTotales() }}</span>
                              <button class="btn-secundario btn-sm" [disabled]="histPaginaActual() >= histPaginasTotales() - 1"
                                      (click)="histPaginaActual.update(p => p + 1)">&#8250;</button>
                            </div>
                          }
                          <div class="ref-busqueda-acciones">
                            <button class="btn-accion btn-sm" [disabled]="!histSeleccionado()" (click)="usarHistoricoSeleccionado()">Usar seleccionado</button>
                          </div>
                        } @else if (!histCargando() && histFiltro()) {
                          <p class="texto-vacio">Sin resultados para "{{ histFiltro() }}".</p>
                        }
                      </div>
                    }
                  </div>

                  <div class="sub-form">
                    <div class="form-grupo form-grupo-ancho">
                      <label>Nombre del Repuesto *</label>
                      <input type="text" placeholder="Descripcion del repuesto"
                             [value]="subNombreRepuesto()"
                             (input)="subNombreRepuesto.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Costo (&#x20A1;) *</label>
                      <input type="number" min="0" step="0.01"
                             [value]="subCostoRepuesto()"
                             (input)="subCostoRepuesto.set(+$any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Fecha Factura Proveedor *</label>
                      <input type="date" [value]="subFechaRepuesto()"
                             (input)="subFechaRepuesto.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Repuestera (Proveedor) *</label>
                      <input type="text" placeholder="Nombre de la tienda"
                             [value]="subRepuestera()"
                             (input)="subRepuestera.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>N&#xb0; Factura Proveedor</label>
                      <input type="text" placeholder="Opcional"
                             [value]="subNumeroFactura()"
                             (input)="subNumeroFactura.set($any($event.target).value)" />
                    </div>
                    <div class="sub-form-acciones">
                      <button class="btn-secundario btn-sm" (click)="toggleAgregarRepuesto()">Cancelar</button>
                      <button class="btn-accion btn-sm" (click)="guardarRepuesto()" [disabled]="guardando()">
                        {{ guardando() ? 'Guardando...' : 'Guardar' }}
                      </button>
                    </div>
                  </div>
                  @if (errorSubForm()) { <p class="error-form">{{ errorSubForm() }}</p> }
                }

                <!-- Formulario editar repuesto -->
                @if (repuEditando()) {
                  <div class="sub-form sub-form-editar">
                    <div class="form-grupo form-grupo-ancho">
                      <label>Nombre del Repuesto *</label>
                      <input type="text" [value]="repuEditNombre()"
                             (input)="repuEditNombre.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Costo (&#x20A1;) *</label>
                      <input type="number" min="0" step="0.01"
                             [value]="repuEditCosto()"
                             (input)="repuEditCosto.set(+$any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Fecha Factura Proveedor *</label>
                      <input type="date" [value]="repuEditFecha()"
                             (input)="repuEditFecha.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>Repuestera *</label>
                      <input type="text" [value]="repuEditRepuestera()"
                             (input)="repuEditRepuestera.set($any($event.target).value)" />
                    </div>
                    <div class="form-grupo">
                      <label>N&#xb0; Factura Proveedor</label>
                      <input type="text" [value]="repuEditNumFactura()"
                             (input)="repuEditNumFactura.set($any($event.target).value)" />
                    </div>
                    <div class="sub-form-acciones">
                      <button class="btn-secundario btn-sm" (click)="repuEditando.set(null)">Cancelar</button>
                      <button class="btn-accion btn-sm" (click)="guardarEdicionRepuesto()" [disabled]="guardando()">
                        {{ guardando() ? 'Guardando...' : 'Guardar cambios' }}
                      </button>
                    </div>
                  </div>
                  @if (errorSubForm()) { <p class="error-form">{{ errorSubForm() }}</p> }
                }
              }

              @if (repuestos().length === 0) {
                <p class="texto-vacio">Sin repuestos registrados.</p>
              } @else {
                <table class="tabla-datos tabla-sm tabla-detalle-orden">
                  <thead>
                    <tr>
                      <th>Repuesto</th><th class="col-oculta-movil">Proveedor</th><th class="col-oculta-movil">Fecha</th><th class="col-monto">Costo</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (r of repuestos(); track r.repuestoId) {
                      <tr [class.fila-editando]="repuEditando()?.repuestoId === r.repuestoId"
                          [class.fila-seleccionada]="repuestoSeleccionado()?.repuestoId === r.repuestoId"
                          (click)="seleccionarRepuesto(r)">
                        <td>{{ r.nombreRepuesto }}</td>
                        <td class="col-oculta-movil">{{ r.repuestera }}</td>
                        <td class="col-oculta-movil">{{ r.fecha | date:'dd/MM/yyyy' }}</td>
                        <td class="col-monto">&#x20A1;{{ r.costo | number:'1.2-2' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              }
            }
          </div>
        </div>

        <!-- Totales -->
        @if (modo() === 'ver') {
          <div class="form-grid solo-lectura" style="margin-top:28px;padding-top:24px;border-top:2px solid var(--color-borde)">
            <div class="form-grupo"><label>Total Repuestos</label>
              <input type="text" [value]="'₡' + (seleccionada()?.totalRepuestos | number:'1.2-2')" readonly />
            </div>
            <div class="form-grupo"><label>Total Reparaciones</label>
              <input type="text" [value]="'₡' + (seleccionada()?.totalReparaciones | number:'1.2-2')" readonly />
            </div>
            <div class="form-grupo"><label>Descuento</label>
              <input type="text" [value]="'₡' + (seleccionada()?.descuento | number:'1.2-2')" readonly />
            </div>
            <div class="form-grupo"><label>Imp. Ventas (%)</label>
              <input type="text" [value]="(seleccionada()?.impuestoVentas ?? 0) + '%'" readonly />
            </div>
            <div class="form-grupo"><label>Adelanto</label>
              <input type="text" [value]="'₡' + (seleccionada()?.adelanto | number:'1.2-2')" readonly />
            </div>
            <div class="form-grupo"><label>Total General</label>
              <input type="text" [value]="'₡' + (seleccionada()?.total | number:'1.2-2')" readonly class="campo-total-ro" />
            </div>
          </div>
        }
        @if (modo() === 'editar') {
          <div class="form-grid" style="margin-top:28px;padding-top:24px;border-top:2px solid var(--color-borde)">
            <div class="form-grupo"><label>Total Repuestos</label>
              <input type="text" [value]="'₡' + (seleccionada()?.totalRepuestos | number:'1.2-2')" readonly class="campo-readonly" />
            </div>
            <div class="form-grupo"><label>Total Reparaciones</label>
              <input type="text" [value]="'₡' + (seleccionada()?.totalReparaciones | number:'1.2-2')" readonly class="campo-readonly" />
            </div>
            <div class="form-grupo"><label>Descuento (&#x20A1;)</label>
              <input type="number" min="0" step="0.01" [value]="descuento()"
                     (input)="descuento.set(+$any($event.target).value)" />
            </div>
            <div class="form-grupo"><label>Imp. Ventas (%)</label>
              <input type="number" min="0" max="100" step="0.01" [value]="impuestoVentas()"
                     (input)="impuestoVentas.set(+$any($event.target).value)" />
            </div>
            <div class="form-grupo"><label>Adelanto (&#x20A1;)</label>
              <input type="number" min="0" step="0.01" [value]="adelanto()"
                     (input)="adelanto.set(+$any($event.target).value)" />
            </div>
            <div class="form-grupo"><label>Total General</label>
              <input type="text" [value]="'₡' + (seleccionada()?.total | number:'1.2-2')" readonly class="campo-readonly campo-total-ro" />
            </div>
          </div>
        }

        @if (modo() === 'ver') {
          <div class="form-acciones">
            <button class="btn-secundario" (click)="cancelar()">Cerrar</button>
          </div>
        }
        @if (modo() === 'editar') {
          @if (errorForm()) { <p class="error-form">{{ errorForm() }}</p> }
          <div class="form-acciones">
            <button class="btn-secundario" (click)="cancelar()" [disabled]="guardando()">Cancelar</button>
            <button class="btn-accion" (click)="guardar()" [disabled]="guardando()">
              {{ guardando() ? 'Guardando...' : 'Aceptar' }}
            </button>
          </div>
        }
      }

    </div>
  }

</div>
  `,
  styles: [`
    .formulario-titulo-fila { display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap; }
    .formulario-titulo-fila .formulario-titulo { margin: 0; }
    .acciones-factura-pdf { display: flex; gap: 8px; flex-wrap: wrap; }

    .col-monto { text-align: right; white-space: nowrap; }
    .col-acciones { white-space: nowrap; text-align: center; }
    .campo-readonly { background: var(--color-fondo); color: var(--color-texto-suave); cursor: default; }
    .campo-total-ro { font-weight: 700; color: var(--color-primario); background: var(--color-fondo); cursor: default; }

    .filtro-estado-grupo { display: flex; gap: 4px; flex-wrap: wrap; margin-top: 8px; }
    .btn-filtro-estado {
      padding: 4px 14px; border-radius: 20px; border: 1px solid var(--color-borde);
      background: transparent; cursor: pointer; font-size: 13px;
      color: var(--color-texto-suave); transition: all 0.15s;
    }
    .btn-filtro-estado.activo { background: var(--color-primario); color: white; border-color: var(--color-primario); }

    .seccion-detalle-factura { margin-top: 22px; border-top: 1px solid var(--color-borde); padding-top: 16px; }

    .tab-accion-barra { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 10px; }
    .btn-sm { padding: 5px 14px; font-size: 13px; }

    .sub-form {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 10px; padding: 14px; background: var(--color-fondo);
      border-radius: var(--radio-borde); margin-bottom: 12px;
    }
    .sub-form-editar { border: 2px solid var(--color-acento); }
    .sub-form-acciones { display: flex; align-items: flex-end; gap: 8px; }

    .tabla-sm td, .tabla-sm th { padding: 6px 8px; font-size: 13px; }
    .fila-editando { background: color-mix(in srgb, var(--color-acento) 8%, transparent) !important; }

    .btn-icono {
      background: none; border: none; cursor: pointer; font-size: 14px;
      padding: 2px 5px; border-radius: 4px; transition: background 0.15s;
    }
    .btn-icono:hover { background: #e8f0fe; }
    .btn-eliminar-fila {
      background: none; border: none; cursor: pointer; color: #e53e3e;
      font-size: 14px; padding: 2px 7px; border-radius: 4px; transition: background 0.15s;
    }
    .btn-eliminar-fila:hover { background: #fff5f5; }

    .panel-ref {
      background: color-mix(in srgb, var(--color-primario) 4%, transparent);
      border: 1px solid color-mix(in srgb, var(--color-primario) 15%, transparent);
      border-radius: var(--radio-borde); padding: 10px 14px; margin-bottom: 12px;
    }
    .btn-ref-toggle {
      background: none; border: none; cursor: pointer; font-size: 13px;
      color: var(--color-primario); font-weight: 500; padding: 0;
    }
    .btn-ref-toggle:hover { text-decoration: underline; }
    .ref-busqueda { margin-top: 10px; }
    .ref-busqueda-barra { display: flex; gap: 8px; margin-bottom: 10px; }
    .ref-busqueda-barra .filtro-input { max-width: 100%; }
    .ref-busqueda-acciones { display: flex; justify-content: flex-end; margin-top: 8px; }
  `]
})
export class FacturasComponent implements OnInit {
  private readonly svc        = inject(FacturasService);
  private readonly repSvc     = inject(ReparacionesService);
  private readonly repuSvc    = inject(RepuestosService);
  private readonly tiposRepSvc = inject(TiposReparacionService);
  private readonly histSvc    = inject(HistoricoRepuestosService);
  private readonly route      = inject(ActivatedRoute);

  // ── Lista ──────────────────────────────────────────────────────
  facturas      = signal<FacturaDto[]>([]);
  cargando      = signal(false);
  textoBusqueda = signal('');
  filtroEstado  = signal(0);
  seleccionada  = signal<FacturaDto | null>(null);

  facturasFiltradas = computed(() => {
    const texto  = this.textoBusqueda().toLowerCase().trim();
    const estado = this.filtroEstado();
    return this.facturas().filter(f => {
      const matchTexto = !texto
        || (f.placa ?? '').toLowerCase().includes(texto)
        || f.nombreCliente.toLowerCase().includes(texto)
        || f.facturaId.toString().includes(texto)
        || ((f.marca ?? '') + ' ' + (f.modelo ?? '')).toLowerCase().includes(texto);
      const matchEstado = !estado || f.estadoFacturaId === estado;
      return matchTexto && matchEstado;
    });
  });

  // ── Formulario principal ───────────────────────────────────────
  modo              = signal<Modo>('ver');
  mostrarFormulario = signal(false);
  guardando         = signal(false);
  errorForm         = signal('');
  enviandoFactura   = signal(false);
  abriendoPdf       = signal(false);

  fecha              = signal('');
  descripcionGeneral = signal('');
  descuento          = signal(0);
  adelanto           = signal(0);
  impuestoVentas     = signal(0);
  estadoFacturaId    = signal(1);

  // ── Reparaciones y Repuestos (columnas simultaneas) ────────────
  reparaciones          = signal<ReparacionDto[]>([]);
  repuestos             = signal<RepuestoDto[]>([]);
  cargandoReparaciones  = signal(false);
  cargandoRepuestos     = signal(false);
  errorSubForm          = signal('');

  // ── Agregar reparacion ────────────────────────────────────────
  mostrarFormReparacion  = signal(false);
  subDescReparacion      = signal('');
  subCostoReparacion     = signal(0);
  subDuracionReparacion  = signal<number | null>(null);

  // ── Editar reparacion ─────────────────────────────────────────
  repEditando     = signal<ReparacionDto | null>(null);
  repEditDesc     = signal('');
  repEditCosto    = signal(0);
  repEditDuracion = signal<number | null>(null);

  // ── Seleccion de fila (Modificar/Eliminar en la barra de acciones) ──
  reparacionSeleccionada = signal<ReparacionDto | null>(null);

  // ── Catalogo de reparaciones ──────────────────────────────────
  mostrarCatalogo  = signal(false);
  catFiltro        = signal('');
  catResultados    = signal<TipoReparacionDto[]>([]);
  catCargando      = signal(false);
  catSeleccionado  = signal<TipoReparacionDto | null>(null);
  catPaginaActual  = signal(0);

  private readonly TAMANO_PAGINA_CATALOGO = 10;

  catPaginasTotales = computed(() =>
    Math.ceil(this.catResultados().length / this.TAMANO_PAGINA_CATALOGO));

  catResultadosPaginados = computed(() => {
    const inicio = this.catPaginaActual() * this.TAMANO_PAGINA_CATALOGO;
    return this.catResultados().slice(inicio, inicio + this.TAMANO_PAGINA_CATALOGO);
  });

  // ── Agregar repuesto ──────────────────────────────────────────
  mostrarFormRepuesto = signal(false);
  subNombreRepuesto   = signal('');
  subCostoRepuesto    = signal(0);
  subFechaRepuesto    = signal('');
  subRepuestera       = signal('');
  subNumeroFactura    = signal('');

  // ── Editar repuesto ───────────────────────────────────────────
  repuEditando      = signal<RepuestoDto | null>(null);
  repuEditNombre    = signal('');
  repuEditCosto     = signal(0);
  repuEditFecha     = signal('');
  repuEditRepuestera = signal('');
  repuEditNumFactura = signal('');

  // ── Seleccion de fila (Modificar/Eliminar en la barra de acciones) ──
  repuestoSeleccionado = signal<RepuestoDto | null>(null);

  // ── Catalogo de repuestos (historico) ──────────────────────────
  mostrarHistorico  = signal(false);
  histFiltro        = signal('');
  histResultados    = signal<HistoricoRespuestoDto[]>([]);
  histCargando      = signal(false);
  histSeleccionado  = signal<HistoricoRespuestoDto | null>(null);
  histPaginaActual  = signal(0);

  private readonly TAMANO_PAGINA_HISTORICO = 10;

  histPaginasTotales = computed(() =>
    Math.ceil(this.histResultados().length / this.TAMANO_PAGINA_HISTORICO));

  histResultadosPaginados = computed(() => {
    const inicio = this.histPaginaActual() * this.TAMANO_PAGINA_HISTORICO;
    return this.histResultados().slice(inicio, inicio + this.TAMANO_PAGINA_HISTORICO);
  });

  ngOnInit() { this.cargarFacturas(); }

  cargarFacturas() {
    this.cargando.set(true);
    this.svc.obtener().subscribe({
      next: lista => {
        this.facturas.set(lista);
        this.cargando.set(false);
        // Si viene de Ordenes con ?id=X, auto-seleccionar esa factura
        const idParam = this.route.snapshot.queryParamMap.get('id');
        if (idParam) {
          const id = +idParam;
          const factura = lista.find(f => f.facturaId === id);
          if (factura) this.verDetalle(factura);
        }
      },
      error: () => this.cargando.set(false)
    });
  }

  seleccionar(f: FacturaDto) {
    if (this.seleccionada()?.facturaId === f.facturaId) {
      this.seleccionada.set(null);
    } else {
      this.seleccionada.set(f);
    }
  }

  verDetalle(f: FacturaDto) {
    this.seleccionada.set(f);
    this.modo.set('ver');
    this.mostrarFormulario.set(true);
    this.errorForm.set('');
    this.reiniciarTabs();
    this.cargarAmbosTabs();
  }

  abrirFormulario(m: 'editar') {
    if (!this.seleccionada()) return;
    this.errorForm.set('');
    this.guardando.set(false);

    const f = this.seleccionada()!;
    this.fecha.set(new Date(f.fecha).toISOString().substring(0, 10));
    this.descripcionGeneral.set(f.descripcionGeneral ?? '');
    this.descuento.set(f.descuento);
    this.adelanto.set(f.adelanto);
    this.impuestoVentas.set(f.impuestoVentas);
    this.estadoFacturaId.set(f.estadoFacturaId);
    this.reiniciarSubForms();
    if (this.reparaciones().length === 0 && this.repuestos().length === 0) {
      this.cargarAmbosTabs();
    }

    this.modo.set(m);
    this.mostrarFormulario.set(true);
  }

  cancelar() {
    if (this.modo() === 'ver') { this.cerrarFormulario(); return; }
    this.reiniciarSubForms();
    this.modo.set('ver');
    this.errorForm.set('');
  }

  private cerrarFormulario() {
    this.mostrarFormulario.set(false);
    this.seleccionada.set(null);
    this.errorForm.set('');
    this.reiniciarTabs();
  }

  guardar() {
    if (!this.fecha()) { this.errorForm.set('La fecha es obligatoria.'); return; }
    const id              = this.seleccionada()!.facturaId;
    const estadoAnterior  = this.seleccionada()!.estadoFacturaId;
    const nuevoEstado     = this.estadoFacturaId();
    this.guardando.set(true);
    this.svc.actualizar(id, {
      fecha: this.fecha(), descripcionGeneral: this.descripcionGeneral(),
      descuento: this.descuento(), adelanto: this.adelanto(),
      impuestoVentas: this.impuestoVentas()
    }).subscribe({
      next: () => {
        if (nuevoEstado !== estadoAnterior) {
          this.svc.cambiarEstado(id, nuevoEstado).subscribe({
            next: () => {
              this.guardando.set(false);
              this.modo.set('ver');
              this.errorForm.set('');
              this.recargarSeleccionada(id);
            },
            error: err => { this.guardando.set(false); this.errorForm.set(this.extraerError(err)); }
          });
        } else {
          this.guardando.set(false);
          this.modo.set('ver');
          this.errorForm.set('');
          this.recargarSeleccionada(id);
        }
      },
      error: err => { this.guardando.set(false); this.errorForm.set(this.extraerError(err)); }
    });
  }

  confirmarEliminar() {
    const f = this.seleccionada();
    if (!f) return;
    if (f.estadoFacturaId !== 1) {
      alert('Solo se pueden eliminar facturas en estado Cotizacion.'); return;
    }
    if (!confirm(`Eliminar Factura #${f.facturaId}? Esta accion eliminara tambien la orden y el detalle asociado.`)) return;
    this.svc.eliminar(f.facturaId).subscribe({
      next: () => { this.cerrarFormulario(); this.cargarFacturas(); },
      error: err => alert(this.extraerError(err))
    });
  }

  enviarFactura() {
    const f = this.seleccionada();
    if (!f) return;
    if (!f.emailCliente) {
      alert('El cliente no tiene correo electronico registrado. No se puede enviar la factura.');
      return;
    }
    if (!confirm(`¿Enviar la Factura #${f.facturaId} al correo ${f.emailCliente}?`)) return;

    this.enviandoFactura.set(true);
    this.svc.enviar(f.facturaId).subscribe({
      next: () => { this.enviandoFactura.set(false); alert('Factura enviada correctamente.'); },
      error: err => { this.enviandoFactura.set(false); alert(this.extraerError(err)); }
    });
  }

  abrirPdf() {
    const f = this.seleccionada();
    if (!f) return;
    this.abriendoPdf.set(true);
    this.svc.obtenerPdf(f.facturaId).subscribe({
      next: blob => {
        this.abriendoPdf.set(false);
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 60000);
      },
      error: async err => {
        this.abriendoPdf.set(false);
        const mensaje = err.error instanceof Blob
          ? this.extraerError({ error: JSON.parse(await err.error.text()) })
          : this.extraerError(err);
        alert(mensaje);
      }
    });
  }

  // ── Reparaciones y Repuestos: carga de listas ──────────────────
  private cargarTab(tab: 'reparaciones' | 'repuestos') {
    const facturaId = this.seleccionada()?.facturaId;
    if (!facturaId) return;
    if (tab === 'reparaciones') {
      this.cargandoReparaciones.set(true);
      this.repSvc.obtenerPorFactura(facturaId).subscribe({
        next: lista => { this.reparaciones.set(lista); this.cargandoReparaciones.set(false); },
        error: () => this.cargandoReparaciones.set(false)
      });
    } else {
      this.cargandoRepuestos.set(true);
      this.repuSvc.obtenerPorFactura(facturaId).subscribe({
        next: lista => { this.repuestos.set(lista); this.cargandoRepuestos.set(false); },
        error: () => this.cargandoRepuestos.set(false)
      });
    }
  }

  private cargarAmbosTabs() {
    this.cargarTab('reparaciones');
    this.cargarTab('repuestos');
  }

  private reiniciarTabs() {
    this.reparaciones.set([]);
    this.repuestos.set([]);
    this.reiniciarSubForms();
  }

  private reiniciarSubForms() {
    this.mostrarFormReparacion.set(false);
    this.mostrarFormRepuesto.set(false);
    this.repEditando.set(null);
    this.repuEditando.set(null);
    this.reparacionSeleccionada.set(null);
    this.repuestoSeleccionado.set(null);
    this.errorSubForm.set('');
    this.mostrarCatalogo.set(false);
    this.catFiltro.set('');
    this.catResultados.set([]);
    this.catSeleccionado.set(null);
    this.catPaginaActual.set(0);
    this.mostrarHistorico.set(false);
    this.histFiltro.set('');
    this.histResultados.set([]);
    this.histSeleccionado.set(null);
    this.histPaginaActual.set(0);
    this.subDescReparacion.set('');   this.subCostoReparacion.set(0);   this.subDuracionReparacion.set(null);
    this.subNombreRepuesto.set('');   this.subCostoRepuesto.set(0);
    this.subFechaRepuesto.set(new Date().toISOString().substring(0, 10));
    this.subRepuestera.set('');  this.subNumeroFactura.set('');
  }

  // ── Reparaciones: seleccion de fila ────────────────────────────
  seleccionarReparacion(r: ReparacionDto) {
    this.reparacionSeleccionada.set(
      this.reparacionSeleccionada()?.reparacionId === r.reparacionId ? null : r);
  }

  // ── Reparaciones: agregar ─────────────────────────────────────
  toggleAgregarReparacion() {
    this.repEditando.set(null);
    this.reparacionSeleccionada.set(null);
    this.mostrarFormReparacion.set(!this.mostrarFormReparacion());
    if (!this.mostrarFormReparacion()) { this.mostrarCatalogo.set(false); }
    this.errorSubForm.set('');
  }

  guardarReparacion() {
    if (!this.subDescReparacion().trim()) {
      this.errorSubForm.set('La descripcion es obligatoria.'); return;
    }
    this.guardando.set(true);
    this.errorSubForm.set('');
    this.repSvc.agregar({
      facturaId: this.seleccionada()!.facturaId,
      descripcionReparacion: this.subDescReparacion(),
      costo: this.subCostoReparacion(),
      duracionAproximadaHoras: this.subDuracionReparacion() ?? undefined
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.subDescReparacion.set('');  this.subCostoReparacion.set(0);  this.subDuracionReparacion.set(null);
        this.mostrarFormReparacion.set(false);
        this.mostrarCatalogo.set(false);
        this.cargarTab('reparaciones');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => { this.guardando.set(false); this.errorSubForm.set(this.extraerError(err)); }
    });
  }

  // ── Reparaciones: editar ──────────────────────────────────────
  editarReparacion(r: ReparacionDto) {
    this.mostrarFormReparacion.set(false);
    this.mostrarCatalogo.set(false);
    this.repEditando.set(r);
    this.repEditDesc.set(r.descripcionReparacion);
    this.repEditCosto.set(r.costo);
    this.repEditDuracion.set(r.duracionAproximadaHoras);
    this.errorSubForm.set('');
  }

  guardarEdicionReparacion() {
    if (!this.repEditDesc().trim()) {
      this.errorSubForm.set('La descripcion es obligatoria.'); return;
    }
    this.guardando.set(true);
    this.errorSubForm.set('');
    this.repSvc.actualizar(this.repEditando()!.reparacionId, {
      descripcionReparacion: this.repEditDesc(),
      costo: this.repEditCosto(),
      duracionAproximadaHoras: this.repEditDuracion()
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.repEditando.set(null);
        this.cargarTab('reparaciones');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => { this.guardando.set(false); this.errorSubForm.set(this.extraerError(err)); }
    });
  }

  eliminarReparacion(id: number) {
    if (!confirm('Eliminar esta reparacion?')) return;
    this.repSvc.eliminar(id).subscribe({
      next: () => {
        this.reparacionSeleccionada.set(null);
        this.cargarTab('reparaciones');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => alert(this.extraerError(err))
    });
  }

  // ── Catalogo de reparaciones ──────────────────────────────────
  private catFiltroTimeout?: ReturnType<typeof setTimeout>;

  onCatFiltroInput(valor: string): void {
    this.catFiltro.set(valor);
    clearTimeout(this.catFiltroTimeout);
    this.catFiltroTimeout = setTimeout(() => this.buscarCatalogo(), 300);
  }

  buscarCatalogo() {
    const filtro = this.catFiltro().trim();
    this.catPaginaActual.set(0);
    this.catSeleccionado.set(null);
    if (!filtro) { this.catResultados.set([]); return; }
    this.catCargando.set(true);
    this.tiposRepSvc.obtener(filtro).subscribe({
      next: lista => { this.catResultados.set(lista); this.catCargando.set(false); },
      error: () => this.catCargando.set(false)
    });
  }

  marcarCatalogo(t: TipoReparacionDto) {
    this.catSeleccionado.set(
      this.catSeleccionado()?.tipoReparacionId === t.tipoReparacionId ? null : t);
  }

  usarCatalogoSeleccionado() {
    if (this.catSeleccionado()) this.seleccionarCatalogo(this.catSeleccionado()!);
  }

  seleccionarCatalogo(t: TipoReparacionDto) {
    this.subDescReparacion.set(t.descripcionReparacion);
    this.subCostoReparacion.set(t.costoBase);
    this.subDuracionReparacion.set(t.duracionAproximadaHoras);
    this.mostrarCatalogo.set(false);
    this.catFiltro.set('');
    this.catResultados.set([]);
    this.catSeleccionado.set(null);
    this.catPaginaActual.set(0);
  }

  // ── Repuestos: seleccion de fila ───────────────────────────────
  seleccionarRepuesto(r: RepuestoDto) {
    this.repuestoSeleccionado.set(
      this.repuestoSeleccionado()?.repuestoId === r.repuestoId ? null : r);
  }

  // ── Repuestos: agregar ────────────────────────────────────────
  toggleAgregarRepuesto() {
    this.repuEditando.set(null);
    this.repuestoSeleccionado.set(null);
    this.mostrarFormRepuesto.set(!this.mostrarFormRepuesto());
    if (!this.mostrarFormRepuesto()) {
      this.mostrarHistorico.set(false);
      this.histFiltro.set('');
      this.histResultados.set([]);
    }
    this.errorSubForm.set('');
  }

  guardarRepuesto() {
    if (!this.subNombreRepuesto().trim() || !this.subFechaRepuesto() || !this.subRepuestera().trim()) {
      this.errorSubForm.set('Nombre, fecha y proveedor son obligatorios.'); return;
    }
    this.guardando.set(true);
    this.errorSubForm.set('');
    this.repuSvc.agregar({
      facturaId: this.seleccionada()!.facturaId,
      nombreRepuesto: this.subNombreRepuesto(),
      costo: this.subCostoRepuesto(),
      fecha: this.subFechaRepuesto(),
      repuestera: this.subRepuestera(),
      numeroFactura: this.subNumeroFactura()
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.subNombreRepuesto.set('');  this.subCostoRepuesto.set(0);
        this.subFechaRepuesto.set(new Date().toISOString().substring(0, 10));
        this.subRepuestera.set('');  this.subNumeroFactura.set('');
        this.mostrarFormRepuesto.set(false);
        this.mostrarHistorico.set(false);
        this.histResultados.set([]);
        this.cargarTab('repuestos');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => { this.guardando.set(false); this.errorSubForm.set(this.extraerError(err)); }
    });
  }

  // ── Repuestos: editar ─────────────────────────────────────────
  editarRepuesto(r: RepuestoDto) {
    this.mostrarFormRepuesto.set(false);
    this.mostrarHistorico.set(false);
    this.histResultados.set([]);
    this.repuEditando.set(r);
    this.repuEditNombre.set(r.nombreRepuesto);
    this.repuEditCosto.set(r.costo);
    this.repuEditFecha.set(new Date(r.fecha).toISOString().substring(0, 10));
    this.repuEditRepuestera.set(r.repuestera);
    this.repuEditNumFactura.set(r.factura ?? '');
    this.errorSubForm.set('');
  }

  guardarEdicionRepuesto() {
    if (!this.repuEditNombre().trim() || !this.repuEditFecha() || !this.repuEditRepuestera().trim()) {
      this.errorSubForm.set('Nombre, fecha y proveedor son obligatorios.'); return;
    }
    this.guardando.set(true);
    this.errorSubForm.set('');
    this.repuSvc.actualizar(this.repuEditando()!.repuestoId, {
      nombreRepuesto: this.repuEditNombre(),
      costo: this.repuEditCosto(),
      fecha: this.repuEditFecha(),
      repuestera: this.repuEditRepuestera(),
      numeroFactura: this.repuEditNumFactura()
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.repuEditando.set(null);
        this.cargarTab('repuestos');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => { this.guardando.set(false); this.errorSubForm.set(this.extraerError(err)); }
    });
  }

  eliminarRepuesto(id: number) {
    if (!confirm('Eliminar este repuesto?')) return;
    this.repuSvc.eliminar(id).subscribe({
      next: () => {
        this.repuestoSeleccionado.set(null);
        this.cargarTab('repuestos');
        this.recargarSeleccionada(this.seleccionada()!.facturaId);
      },
      error: err => alert(this.extraerError(err))
    });
  }

  // ── Catalogo de repuestos (historico) ──────────────────────────
  private histFiltroTimeout?: ReturnType<typeof setTimeout>;

  onHistFiltroInput(valor: string): void {
    this.histFiltro.set(valor);
    clearTimeout(this.histFiltroTimeout);
    this.histFiltroTimeout = setTimeout(() => this.buscarHistorico(), 300);
  }

  buscarHistorico() {
    const filtro = this.histFiltro().trim();
    this.histPaginaActual.set(0);
    this.histSeleccionado.set(null);
    if (!filtro) { this.histResultados.set([]); return; }
    this.histCargando.set(true);
    this.histSvc.obtener().subscribe({
      next: lista => {
        const f = filtro.toLowerCase();
        const filtrados = lista.filter(h =>
          h.marca.toLowerCase().includes(f) ||
          h.modelo.toLowerCase().includes(f) ||
          h.repuestoDecripcion.toLowerCase().includes(f) ||
          h.repuestera.toLowerCase().includes(f)
        );
        this.histResultados.set(filtrados);
        this.histCargando.set(false);
      },
      error: () => this.histCargando.set(false)
    });
  }

  marcarHistorico(h: HistoricoRespuestoDto) {
    this.histSeleccionado.set(
      this.histSeleccionado()?.respuestoHistoricoId === h.respuestoHistoricoId ? null : h);
  }

  usarHistoricoSeleccionado() {
    if (this.histSeleccionado()) this.seleccionarHistorico(this.histSeleccionado()!);
  }

  seleccionarHistorico(h: HistoricoRespuestoDto) {
    this.subNombreRepuesto.set(h.repuestoDecripcion);
    this.subCostoRepuesto.set(h.precio);
    this.subRepuestera.set(h.repuestera);
    this.mostrarHistorico.set(false);
    this.histFiltro.set('');
    this.histResultados.set([]);
    this.histSeleccionado.set(null);
    this.histPaginaActual.set(0);
  }

  // ── Helpers ───────────────────────────────────────────────────
  private recargarSeleccionada(id: number) {
    this.svc.obtener().subscribe({
      next: lista => {
        this.facturas.set(lista);
        const actualizada = lista.find(f => f.facturaId === id);
        if (actualizada) this.seleccionada.set(actualizada);
      },
      error: () => {}
    });
  }

  private extraerError(err: { error?: unknown }): string {
    const e = err.error;
    if (typeof e === 'string') return e;
    return (e as any)?.detail ?? (e as any)?.title ?? 'Error al procesar la solicitud.';
  }
}

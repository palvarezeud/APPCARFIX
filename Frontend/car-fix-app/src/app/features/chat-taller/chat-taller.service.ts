import { Injectable, inject, signal } from '@angular/core';
import { ReconocimientoVozService } from '../../core/voz/reconocimiento-voz.service';
import { AsistenteVozApiService } from '../../services/asistente-voz-api.service';
import { ClientesService } from '../../services/clientes.service';
import { VehiculosService, VehiculoRequest } from '../../services/vehiculos.service';
import { OrdenesService } from '../../services/ordenes.service';
import { FacturasService } from '../../services/facturas.service';
import { CrearClienteRequest } from '../../models/cliente.model';
import { ClienteVozDto, VehiculoVozDto, OrdenVozDto } from '../../models/interpretacion-voz.model';
import { MensajeChat, PasoChat, FaseChat } from './chat-taller.model';
import { clasificarRespuesta } from './interprete-confirmacion.util';

type BorradorCliente = Partial<CrearClienteRequest>;
type BorradorVehiculo = Partial<Omit<VehiculoRequest, 'clienteId'>>;
type BorradorOrden = { problemaGeneral: string; esGarantia: boolean };

const DIAS_PLAZO_ORDEN = 3;

@Injectable()
export class ChatTallerService {
  private readonly reconocimientoVoz = inject(ReconocimientoVozService);
  private readonly api               = inject(AsistenteVozApiService);
  private readonly clientesSvc       = inject(ClientesService);
  private readonly vehiculosSvc      = inject(VehiculosService);
  private readonly ordenesSvc        = inject(OrdenesService);
  private readonly facturasSvc       = inject(FacturasService);

  readonly soportado = this.reconocimientoVoz.soportado;

  readonly mensajes   = signal<MensajeChat[]>([]);
  readonly pasoActual = signal<PasoChat | null>(null);
  readonly fase       = signal<FaseChat>('inicio');
  readonly escuchando = signal(false);
  readonly procesando = signal(false);

  readonly clienteIdCreado  = signal<number | null>(null);
  readonly vehiculoIdCreado = signal<number | null>(null);
  readonly ordenIdCreado    = signal<number | null>(null);
  readonly facturaIdCreada  = signal<number | null>(null);
  readonly nombreClienteSesion = signal<string | null>(null);
  readonly emailClienteSesion  = signal<string | null>(null);

  private borradorCliente:  BorradorCliente  | null = null;
  private borradorVehiculo: BorradorVehiculo | null = null;
  private borradorOrden:    BorradorOrden    | null = null;

  escucharTurno(): void {
    if (this.escuchando() || this.procesando()) return;
    this.escuchando.set(true);

    this.reconocimientoVoz.escuchar().subscribe({
      next: texto => {
        this.escuchando.set(false);
        if (!texto) {
          this.agregarMensaje(
            'asistente',
            'No se detecto ningun audio. Revise que su navegador soporte reconocimiento de voz (recomendado: Chrome) e intente de nuevo.',
            true);
          return;
        }
        this.agregarMensaje('usuario', texto, false);
        this.procesarTurno(texto);
      },
      error: err => {
        this.escuchando.set(false);
        this.agregarMensaje('asistente', err?.message ?? 'No se pudo escuchar el comando.', true);
      }
    });
  }

  private procesarTurno(texto: string): void {
    if (this.fase() === 'confirmando' && this.pasoActual()) {
      this.manejarRespuestaConfirmacion(texto);
      return;
    }
    this.interpretarYDespachar(texto, null);
  }

  private interpretarYDespachar(texto: string, intentEnProgreso: PasoChat | null): void {
    this.procesando.set(true);
    this.api.interpretar(texto, '/chat', intentEnProgreso).subscribe({
      next: resultado => {
        this.procesando.set(false);

        switch (resultado.intent) {
          case 'crear_cliente':
            if (resultado.cliente) { this.iniciarPasoCliente(resultado.cliente); return; }
            break;
          case 'crear_vehiculo':
            if (resultado.vehiculo) { this.iniciarPasoVehiculo(resultado.vehiculo); return; }
            break;
          case 'crear_orden':
            if (resultado.orden) { this.iniciarPasoOrden(resultado.orden); return; }
            break;
          case 'enviar_factura':
            this.iniciarPasoFactura();
            return;
        }

        this.agregarMensaje(
          'asistente',
          resultado.mensajeParaUsuario ?? 'No entendi el comando. Intente de nuevo.',
          false);
      },
      error: () => {
        this.procesando.set(false);
        this.agregarMensaje('asistente', 'No se pudo comunicar con el asistente de voz.', true);
      }
    });
  }

  // ── Paso Cliente ─────────────────────────────────────────────────────

  private iniciarPasoCliente(datos: ClienteVozDto): void {
    this.borradorCliente = {
      nombreCliente: datos.nombreCliente ?? undefined,
      telefono1:     datos.telefono1 ?? undefined,
      telefono2:     datos.telefono2,
      email:         datos.email,
      esEmpresa:     datos.esEmpresa ?? false
    };

    if (!this.borradorCliente.nombreCliente || !this.borradorCliente.telefono1) {
      this.agregarMensaje('asistente', 'Necesito al menos el nombre y el telefono del cliente.', false);
      return;
    }

    this.pasoActual.set('cliente');
    this.fase.set('confirmando');
    this.agregarMensaje('asistente', this.resumenCliente(this.borradorCliente), false);
  }

  private resumenCliente(b: BorradorCliente): string {
    const partes = [`cliente ${b.nombreCliente}`, `telefono ${b.telefono1}`];
    if (b.email) partes.push(`correo ${b.email}`);
    return `Entendi: ${partes.join(', ')}. ¿Confirmo?`;
  }

  private confirmarCliente(): void {
    const b = this.borradorCliente;
    if (!b?.nombreCliente || !b.telefono1) return;

    this.fase.set('ejecutando');
    const req: CrearClienteRequest = {
      nombreCliente: b.nombreCliente,
      telefono1:     b.telefono1,
      telefono2:     b.telefono2 ?? null,
      email:         b.email ?? null,
      esEmpresa:     b.esEmpresa ?? false
    };

    this.clientesSvc.crear(req).subscribe({
      next: id => {
        this.clienteIdCreado.set(id);
        this.nombreClienteSesion.set(b.nombreCliente!);
        this.emailClienteSesion.set(b.email ?? null);
        this.agregarMensaje(
          'asistente',
          `Cliente ${b.nombreCliente} creado. Cuando quiera, digame la siguiente accion (ej. crear su vehiculo).`,
          false);
        this.borradorCliente = null;
        this.pasoActual.set(null);
        this.fase.set('inicio');
      },
      error: err => this.manejarErrorEjecucion(err, 'cliente')
    });
  }

  // ── Paso Vehiculo ────────────────────────────────────────────────────

  private iniciarPasoVehiculo(datos: VehiculoVozDto): void {
    if (datos.nombreClienteBuscado && !this.clienteIdCreado()) {
      this.resolverClientePorNombre(datos.nombreClienteBuscado, datos);
      return;
    }

    if (!this.clienteIdCreado()) {
      this.agregarMensaje('asistente', 'Primero necesito los datos del cliente dueño del vehiculo.', false);
      return;
    }

    this.borradorVehiculo = {
      placa:              datos.placa,
      marca:              datos.marca ?? undefined,
      modelo:             datos.modelo ?? undefined,
      vin:                datos.vin,
      annio:              datos.annio ?? undefined,
      motor:              datos.motor,
      esAutomatico:       datos.esAutomatico ?? false,
      detallesCarroceria: 'Sin detalles registrados (creado por chat de voz).'
    };

    if (!this.borradorVehiculo.marca || !this.borradorVehiculo.modelo || !this.borradorVehiculo.annio) {
      this.agregarMensaje('asistente', 'Necesito al menos marca, modelo y annio del vehiculo.', false);
      return;
    }

    this.pasoActual.set('vehiculo');
    this.fase.set('confirmando');
    this.agregarMensaje('asistente', this.resumenVehiculo(this.borradorVehiculo), false);
  }

  private resolverClientePorNombre(nombre: string, datosVehiculo: VehiculoVozDto): void {
    this.clientesSvc.obtener(nombre).subscribe({
      next: clientes => {
        if (clientes.length === 1) {
          this.clienteIdCreado.set(clientes[0].clienteId);
          this.nombreClienteSesion.set(clientes[0].nombreCliente);
          this.emailClienteSesion.set(clientes[0].email);
          this.iniciarPasoVehiculo(datosVehiculo);
        } else {
          this.agregarMensaje(
            'asistente',
            `No encontre un cliente unico llamado "${nombre}". Cree el cliente primero.`,
            false);
        }
      },
      error: () => this.agregarMensaje('asistente', 'No se pudo buscar el cliente.', true)
    });
  }

  private resumenVehiculo(b: BorradorVehiculo): string {
    const partes = [`${b.marca} ${b.modelo}`, `annio ${b.annio}`];
    if (b.placa) partes.push(`placa ${b.placa}`);
    return `Entendi: vehiculo ${partes.join(', ')}. ¿Confirmo?`;
  }

  private confirmarVehiculo(): void {
    const b = this.borradorVehiculo;
    const clienteId = this.clienteIdCreado();
    if (!b?.marca || !b.modelo || !b.annio || !clienteId) return;

    this.fase.set('ejecutando');
    const req: VehiculoRequest = {
      clienteId,
      placa:              b.placa ?? null,
      marca:              b.marca,
      modelo:             b.modelo,
      vin:                b.vin ?? null,
      annio:              b.annio,
      motor:              b.motor ?? null,
      esAutomatico:       b.esAutomatico ?? false,
      detallesCarroceria: b.detallesCarroceria ?? 'Sin detalles registrados (creado por chat de voz).'
    };

    this.vehiculosSvc.crear(req).subscribe({
      next: id => {
        this.vehiculoIdCreado.set(id);
        this.agregarMensaje(
          'asistente',
          `Vehiculo ${b.marca} ${b.modelo} creado. Cuando quiera, digame la siguiente accion (ej. crear la orden de servicio).`,
          false);
        this.borradorVehiculo = null;
        this.pasoActual.set(null);
        this.fase.set('inicio');
      },
      error: err => this.manejarErrorEjecucion(err, 'vehiculo')
    });
  }

  // ── Paso Orden ───────────────────────────────────────────────────────

  private iniciarPasoOrden(datos: OrdenVozDto): void {
    if (datos.placaBuscada && !this.vehiculoIdCreado()) {
      this.resolverVehiculoPorPlaca(datos.placaBuscada, datos);
      return;
    }

    if (!this.vehiculoIdCreado()) {
      this.agregarMensaje('asistente', 'Primero necesito crear (o encontrar) el vehiculo de la orden.', false);
      return;
    }

    if (!datos.problemaGeneral) {
      this.agregarMensaje('asistente', 'Necesito una descripcion del problema para la orden.', false);
      return;
    }

    this.borradorOrden = { problemaGeneral: datos.problemaGeneral, esGarantia: datos.esGarantia ?? false };

    this.pasoActual.set('orden');
    this.fase.set('confirmando');
    this.agregarMensaje(
      'asistente',
      `Entendi: orden de servicio por "${datos.problemaGeneral}". ¿Confirmo?`,
      false);
  }

  private resolverVehiculoPorPlaca(placa: string, datosOrden: OrdenVozDto): void {
    this.vehiculosSvc.obtener(placa).subscribe({
      next: vehiculos => {
        if (vehiculos.length === 1) {
          this.vehiculoIdCreado.set(vehiculos[0].vehiculoId);
          this.clienteIdCreado.set(vehiculos[0].clienteId);
          this.nombreClienteSesion.set(vehiculos[0].nombreCliente);
          this.iniciarPasoOrden(datosOrden);
        } else {
          this.agregarMensaje(
            'asistente',
            `No encontre un vehiculo unico con placa "${placa}". Cree el vehiculo primero.`,
            false);
        }
      },
      error: () => this.agregarMensaje('asistente', 'No se pudo buscar el vehiculo.', true)
    });
  }

  private confirmarOrden(): void {
    const b = this.borradorOrden;
    const vehiculoId = this.vehiculoIdCreado();
    if (!b || !vehiculoId) return;

    this.fase.set('ejecutando');
    const ahora = new Date();
    const salida = new Date(ahora.getTime() + DIAS_PLAZO_ORDEN * 24 * 60 * 60 * 1000);

    this.ordenesSvc.crear({
      vehiculoId,
      fechaIngreso:    ahora.toISOString(),
      fechaSalida:     salida.toISOString(),
      problemaGeneral: b.problemaGeneral,
      esGarantia:      b.esGarantia
    }).subscribe({
      next: resp => {
        this.ordenIdCreado.set(resp.ordenServicioId);
        this.facturaIdCreada.set(resp.facturaId);
        this.borradorOrden = null;

        const cliente = this.nombreClienteSesion() ?? 'el cliente';
        this.agregarMensaje(
          'asistente',
          `Se genero la orden #${resp.ordenServicioId} y la factura de cotizacion. ` +
          `¿Envio la factura al correo de ${cliente}?`,
          false);

        this.pasoActual.set('factura');
        this.fase.set('confirmando');
      },
      error: err => this.manejarErrorEjecucion(err, 'orden')
    });
  }

  // ── Paso Factura ─────────────────────────────────────────────────────

  private iniciarPasoFactura(): void {
    if (!this.facturaIdCreada()) {
      this.agregarMensaje('asistente', 'Todavia no hay ninguna factura creada en esta conversacion.', false);
      return;
    }
    this.pasoActual.set('factura');
    this.fase.set('confirmando');
    this.agregarMensaje('asistente', '¿Confirmo el envio de la factura por correo?', false);
  }

  private confirmarFactura(): void {
    const facturaId = this.facturaIdCreada();
    if (!facturaId) return;

    this.fase.set('ejecutando');
    this.facturasSvc.enviar(facturaId).subscribe({
      next: () => {
        const email = this.emailClienteSesion() ?? 'el correo del cliente';
        this.agregarMensaje('asistente', `Factura enviada a ${email}.`, false);
        this.fase.set('terminado');
      },
      error: err => this.manejarErrorEjecucion(err, 'factura')
    });
  }

  // ── Confirmacion / correccion / cancelacion ─────────────────────────

  private manejarRespuestaConfirmacion(texto: string): void {
    const clasificacion = clasificarRespuesta(texto);

    if (clasificacion === 'afirmativo') {
      switch (this.pasoActual()) {
        case 'cliente':  this.confirmarCliente();  break;
        case 'vehiculo': this.confirmarVehiculo(); break;
        case 'orden':    this.confirmarOrden();    break;
        case 'factura':  this.confirmarFactura();  break;
      }
      return;
    }

    if (clasificacion === 'negativo') {
      if (this.pasoActual() === 'factura') {
        this.agregarMensaje('asistente', 'De acuerdo, puede enviarla despues desde Facturas.', false);
        this.fase.set('terminado');
      } else {
        this.agregarMensaje('asistente', 'Operacion cancelada.', false);
        this.reiniciar();
      }
      return;
    }

    // correccion: reinterpretar con la etapa actual como pista
    const paso = this.pasoActual();
    if (paso) {
      this.interpretarYDespachar(texto, paso);
    }
  }

  private manejarErrorEjecucion(err: { error?: unknown }, paso: PasoChat): void {
    this.agregarMensaje('asistente', this.extraerError(err), true);
    this.pasoActual.set(paso);
    this.fase.set('confirmando');
  }

  private extraerError(err: { error?: unknown }): string {
    const e = err.error;
    if (typeof e === 'string') return e;
    return (e as any)?.detail ?? (e as any)?.title ?? 'Ocurrio un error al procesar la solicitud.';
  }

  private agregarMensaje(autor: 'usuario' | 'asistente', texto: string, esError: boolean): void {
    this.mensajes.update(m => [
      ...m,
      { id: crypto.randomUUID(), autor, texto, esError, fecha: new Date() }
    ]);
  }

  private reiniciar(): void {
    this.borradorCliente = null;
    this.borradorVehiculo = null;
    this.borradorOrden = null;
    this.pasoActual.set(null);
    this.fase.set('inicio');
  }
}

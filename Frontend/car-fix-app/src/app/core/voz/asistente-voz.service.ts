import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ReconocimientoVozService } from './reconocimiento-voz.service';
import { AsistenteVozApiService } from '../../services/asistente-voz-api.service';
import { AuthService } from '../auth/auth.service';
import { ClienteVozDto, VehiculoVozDto, InterpretacionVozDto } from '../../models/interpretacion-voz.model';

export type EstadoAsistenteVoz = 'inactivo' | 'escuchando' | 'procesando' | 'error';

export type AccionVozPendiente =
  | { tipo: 'abrir-crear' }
  | { tipo: 'prellenar-cliente'; datos: ClienteVozDto }
  | { tipo: 'prellenar-vehiculo'; datos: VehiculoVozDto }
  | { tipo: 'buscar'; termino: string };

@Injectable({ providedIn: 'root' })
export class AsistenteVozService {
  private readonly router              = inject(Router);
  private readonly auth                = inject(AuthService);
  private readonly reconocimientoVoz   = inject(ReconocimientoVozService);
  private readonly api                 = inject(AsistenteVozApiService);

  readonly soportado = this.reconocimientoVoz.soportado;

  private readonly _estado = signal<EstadoAsistenteVoz>('inactivo');
  readonly estado = this._estado.asReadonly();

  private readonly _mensajeUsuario = signal<string | null>(null);
  readonly mensajeUsuario = this._mensajeUsuario.asReadonly();

  private readonly mapaPermisos: Record<string, () => boolean> = {
    usuarios:                () => this.auth.esJefe(),
    configuracion:            () => this.auth.esAdmin(),
    'catalogo-reparaciones':  () => this.auth.esJefe(),
    'catalogo-repuestos':     () => this.auth.esJefe(),
  };

  private readonly _pendiente = signal<{ pantalla: string; accion: AccionVozPendiente } | null>(null);
  private mensajeTimeout?: ReturnType<typeof setTimeout>;

  activar(): void {
    if (this._estado() === 'escuchando' || this._estado() === 'procesando') return;
    this._mensajeUsuario.set(null);
    this._estado.set('escuchando');

    this.reconocimientoVoz.escuchar().subscribe({
      next:  texto => this.procesarTranscripcion(texto),
      error: err   => this.mostrarError(err?.message ?? 'No se pudo escuchar el comando.')
    });
  }

  private procesarTranscripcion(texto: string): void {
    if (!texto) {
      this._estado.set('inactivo');
      return;
    }
    this._estado.set('procesando');

    this.api.interpretar(texto, this.router.url).subscribe({
      next:  resultado => this.despachar(resultado),
      error: ()         => this.mostrarError('No se pudo comunicar con el asistente de voz.')
    });
  }

  private despachar(resultado: InterpretacionVozDto): void {
    this._estado.set('inactivo');

    let manejado = false;

    if (resultado.intent === 'navegar' && resultado.pantallaDestino) {
      manejado = this.navegarConPermiso(
        resultado.pantallaDestino,
        resultado.abrirFormularioCrear ? { tipo: 'abrir-crear' } : null);
    } else if (resultado.intent === 'buscar' && resultado.pantallaDestino && resultado.terminoBusqueda) {
      manejado = this.navegarConPermiso(
        resultado.pantallaDestino, { tipo: 'buscar', termino: resultado.terminoBusqueda });
    } else if (resultado.intent === 'crear_cliente' && resultado.cliente) {
      manejado = this.navegarConPermiso('clientes', { tipo: 'prellenar-cliente', datos: resultado.cliente });
    } else if (resultado.intent === 'crear_vehiculo' && resultado.vehiculo) {
      manejado = this.navegarConPermiso('vehiculos', { tipo: 'prellenar-vehiculo', datos: resultado.vehiculo });
    }

    if (!manejado) {
      this.mostrarMensaje(resultado.mensajeParaUsuario ?? 'No se pudo procesar el comando.');
    }
  }

  private navegarConPermiso(pantalla: string, accion: AccionVozPendiente | null): boolean {
    const verificarPermiso = this.mapaPermisos[pantalla];
    if (verificarPermiso && !verificarPermiso()) {
      this.mostrarMensaje('No tiene permisos para esa pantalla.');
      return true;
    }

    if (accion) this._pendiente.set({ pantalla, accion });
    this.router.navigate(['/' + pantalla]);
    return true;
  }

  tomarAccionPendiente(pantalla: string): AccionVozPendiente | null {
    const actual = this._pendiente();
    if (!actual || actual.pantalla !== pantalla) return null;
    this._pendiente.set(null);
    return actual.accion;
  }

  private mostrarMensaje(mensaje: string): void {
    clearTimeout(this.mensajeTimeout);
    this._mensajeUsuario.set(mensaje);
    this.mensajeTimeout = setTimeout(() => this._mensajeUsuario.set(null), 5000);
  }

  private mostrarError(mensaje: string): void {
    this._estado.set('error');
    this.mostrarMensaje(mensaje);
    setTimeout(() => {
      if (this._estado() === 'error') this._estado.set('inactivo');
    }, 4000);
  }
}

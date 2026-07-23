import { Injectable, inject, signal } from '@angular/core';
import { AutenticacionService } from '../../services/autenticacion.service';
import { TokenResponse } from '../../models/token-response.model';

const DB_NOMBRE  = 'carfix-biometria';
const DB_VERSION = 1;
const ALMACEN    = 'credencial';
const CLAVE      = 'actual';

interface RegistroBiometria {
  nombreUsuario:  string;
  credencialId:   string;
  tokenRefresco:  string;
  fechaRegistro:  string;
}

function base64UrlABuffer(base64url: string): ArrayBuffer {
  const base64  = base64url.replace(/-/g, '+').replace(/_/g, '/');
  const relleno = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
  const binario = atob(relleno);
  const bytes   = new Uint8Array(binario.length);
  for (let i = 0; i < binario.length; i++) bytes[i] = binario.charCodeAt(i);
  return bytes.buffer;
}

@Injectable({ providedIn: 'root' })
export class BiometriaService {
  private readonly autenticacionSvc = inject(AutenticacionService);

  private readonly _soportado = signal(false);
  readonly soportado = this._soportado.asReadonly();

  private readonly _habilitado = signal(false);
  readonly habilitado = this._habilitado.asReadonly();

  private readonly _nombreUsuarioHabilitado = signal<string | null>(null);
  readonly nombreUsuarioHabilitado = this._nombreUsuarioHabilitado.asReadonly();

  constructor() {
    this.detectarSoporte();
    this.cargarEstado();
  }

  private async detectarSoporte(): Promise<void> {
    if (!('credentials' in navigator) || !window.PublicKeyCredential) return;
    try {
      const disponible = await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
      this._soportado.set(disponible);
    } catch {
      this._soportado.set(false);
    }
  }

  private async cargarEstado(): Promise<void> {
    const registro = await this.leerRegistro();
    this._habilitado.set(registro !== null);
    this._nombreUsuarioHabilitado.set(registro?.nombreUsuario ?? null);
  }

  async registrar(nombreUsuario: string, tokenRefresco: string): Promise<boolean> {
    try {
      const challenge = crypto.getRandomValues(new Uint8Array(32));
      const userId    = crypto.getRandomValues(new Uint8Array(16));

      const credencial = await navigator.credentials.create({
        publicKey: {
          challenge,
          rp: { name: 'CarFix' },
          user: { id: userId, name: nombreUsuario, displayName: nombreUsuario },
          pubKeyCredParams: [{ type: 'public-key', alg: -7 }, { type: 'public-key', alg: -257 }],
          authenticatorSelection: {
            authenticatorAttachment: 'platform',
            userVerification: 'required',
            residentKey: 'preferred'
          },
          timeout: 60000
        }
      }) as PublicKeyCredential | null;

      if (!credencial) return false;

      await this.guardarRegistro({
        nombreUsuario,
        credencialId:  credencial.id,
        tokenRefresco,
        fechaRegistro: new Date().toISOString()
      });

      this._habilitado.set(true);
      this._nombreUsuarioHabilitado.set(nombreUsuario);
      return true;
    } catch {
      return false;
    }
  }

  async desbloquear(): Promise<TokenResponse | null> {
    const registro = await this.leerRegistro();
    if (!registro) return null;

    try {
      const challenge = crypto.getRandomValues(new Uint8Array(32));
      const asercion = await navigator.credentials.get({
        publicKey: {
          challenge,
          allowCredentials: [{ id: base64UrlABuffer(registro.credencialId), type: 'public-key' }],
          userVerification: 'required',
          timeout: 60000
        }
      });
      if (!asercion) return null;
    } catch {
      return null;
    }

    try {
      const respuesta = await new Promise<TokenResponse>((resolve, reject) => {
        this.autenticacionSvc.refrescarSesion(registro.tokenRefresco).subscribe({
          next: resolve,
          error: reject
        });
      });

      await this.guardarRegistro({ ...registro, tokenRefresco: respuesta.tokenRefresco });
      return respuesta;
    } catch {
      await this.deshabilitar();
      return null;
    }
  }

  async deshabilitar(): Promise<void> {
    await this.borrarRegistro();
    this._habilitado.set(false);
    this._nombreUsuarioHabilitado.set(null);
  }

  async revocarYDeshabilitar(): Promise<void> {
    const registro = await this.leerRegistro();
    if (registro) {
      this.autenticacionSvc.cerrarSesion(registro.tokenRefresco).subscribe({ error: () => {} });
    }
    await this.deshabilitar();
  }

  private abrirDb(): Promise<IDBDatabase> {
    return new Promise((resolve, reject) => {
      const peticion = indexedDB.open(DB_NOMBRE, DB_VERSION);
      peticion.onupgradeneeded = () => {
        if (!peticion.result.objectStoreNames.contains(ALMACEN)) {
          peticion.result.createObjectStore(ALMACEN);
        }
      };
      peticion.onsuccess = () => resolve(peticion.result);
      peticion.onerror   = () => reject(peticion.error);
    });
  }

  private async leerRegistro(): Promise<RegistroBiometria | null> {
    try {
      const db = await this.abrirDb();
      return await new Promise((resolve, reject) => {
        const tx = db.transaction(ALMACEN, 'readonly');
        const peticion = tx.objectStore(ALMACEN).get(CLAVE);
        peticion.onsuccess = () => resolve(peticion.result ?? null);
        peticion.onerror   = () => reject(peticion.error);
      });
    } catch {
      return null;
    }
  }

  private async guardarRegistro(registro: RegistroBiometria): Promise<void> {
    const db = await this.abrirDb();
    await new Promise<void>((resolve, reject) => {
      const tx = db.transaction(ALMACEN, 'readwrite');
      tx.objectStore(ALMACEN).put(registro, CLAVE);
      tx.oncomplete = () => resolve();
      tx.onerror    = () => reject(tx.error);
    });
  }

  private async borrarRegistro(): Promise<void> {
    try {
      const db = await this.abrirDb();
      await new Promise<void>((resolve, reject) => {
        const tx = db.transaction(ALMACEN, 'readwrite');
        tx.objectStore(ALMACEN).delete(CLAVE);
        tx.oncomplete = () => resolve();
        tx.onerror    = () => reject(tx.error);
      });
    } catch {
      // sin registro que borrar
    }
  }
}

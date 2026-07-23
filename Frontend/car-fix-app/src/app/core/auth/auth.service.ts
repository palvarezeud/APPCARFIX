import { Injectable, computed, signal, inject } from '@angular/core';
import { Router } from '@angular/router';

const ROL_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const TOKEN_KEY = 'carfix_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly router = inject(Router);

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _rol   = signal<string | null>(this.extraerRol(localStorage.getItem(TOKEN_KEY)));
  private readonly _usuario = signal<string | null>(this.extraerUsuario(localStorage.getItem(TOKEN_KEY)));

  readonly token           = this._token.asReadonly();
  readonly rol             = this._rol.asReadonly();
  readonly nombreUsuario   = this._usuario.asReadonly();
  readonly estaAutenticado = computed(() => this._token() !== null);
  readonly esAdmin         = computed(() => this._rol() === 'Administrador');
  readonly esJefe          = computed(() => this._rol() === 'JefeMecanicos' || this.esAdmin());
  readonly esMecanico      = computed(() => this.estaAutenticado());

  guardarSesion(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
    this._rol.set(this.extraerRol(token));
    this._usuario.set(this.extraerUsuario(token));
  }

  cerrarSesion(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
    this._rol.set(null);
    this._usuario.set(null);
    this.router.navigate(['/login']);
  }

  private extraerRol(token: string | null): string | null {
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload[ROL_CLAIM] ?? null;
    } catch { return null; }
  }

  private extraerUsuario(token: string | null): string | null {
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? null;
    } catch { return null; }
  }
}

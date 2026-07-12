# Guía de Buenas Prácticas en Angular 21 (Modern Angular)

Esta guía detalla las mejores prácticas y patrones arquitectónicos recomendados para el desarrollo de aplicaciones robustas, escalables y eficientes utilizando **Angular 21**. El enfoque principal es adoptar las características modernas del framework y **evitar características obsoletas (deprecated) o heredadas** de versiones antiguas.

---

## 1. Arquitectura y Estructura de Archivos

### Práctica Moderna: Componentes Standalone (Por Defecto)

A partir de las versiones recientes de Angular, los `NgModule` tradicionales están obsoletos en la práctica diaria. **Todos los componentes, directivas y pipes deben ser autónomos (`standalone: true`)**. En Angular 21, esta es la opción por defecto.

- **Ventajas:** Menor acoplamiento, mejor tree-shaking, inicialización más rápida y componentes completamente modulares.
- **Estructura recomendada:** Organización por características (Features) o dominios funcionales, no por tipo de archivo técnico.

```text
src/app/
├── core/                         # Proveedores globales, guardias, interceptores
│   ├── auth/
│   │   ├── auth.service.ts       # Estado de sesion y rol (Signals)
│   │   ├── auth.interceptor.ts   # Adjunta Bearer token a peticiones HTTP
│   │   └── guards.ts             # Guardias funcionales por rol
│   └── interceptors/
├── shared/                       # Componentes de UI comunes y directivas reutilizables
│   ├── components/
│   │   ├── button/
│   │   └── card/
│   └── pipes/
├── features/                     # Módulos funcionales de la aplicación
│   ├── dashboard/
│   │   ├── components/
│   │   └── dashboard.component.ts
│   └── users/
├── app.config.ts                 # Configuración global basada en funciones
└── app.component.ts              # Componente raíz (Standalone)
```

### Evitar (Obsoleto/Legacy):

- Uso de `NgModule` (`@NgModule`) para declarar componentes y gestionar dependencias.
- Crear carpetas gigantescas como `components/` globales que mezclan lógica de negocio pesada con UI atómica.

---

## 2. Gestión de Estado y Reactividad: Signals

### Práctica Moderna: Angular Signals

Para el estado síncrono local del componente, la comunicación input/output y el renderizado eficiente, se deben utilizar **Signals**.

- **`signal()`**: Para estado mutable local.
- **`computed()`**: Para valores derivados reactivos y optimizados (memoizados).
- **`effect()`**: Para efectos secundarios controlados (como logging o sincronización externa).

```typescript
import { Component, signal, computed } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-counter',
  template: `
    <div class="counter-box">
      <p>Contador: {{ count() }}</p>
      <p>Doble: {{ doubleCount() }}</p>
      <button (click)="increment()">Incrementar</button>
    </div>
  `
})
export class CounterComponent {
  count = signal<number>(0);
  doubleCount = computed(() => this.count() * 2);

  increment() {
    this.count.update(value => value + 1);
  }
}
```

### Interoperabilidad con RxJS

RxJS sigue siendo esencial para operaciones asíncronas complejas, peticiones HTTP y manejo de streams de datos. Usa las funciones puente de `@angular/core/rxjs-interop`:

- `toSignal()`: Convierte un Observable en un Signal para simplificar el binding en la plantilla sin usar el pipe `async`.
- `toObservable()`: Convierte un Signal en un Observable cuando necesites operadores como `switchMap` o `debounceTime`.

### Evitar (Prácticas Obsoletas):

- Depender exclusivamente de `BehaviorSubject` o `ReplaySubject` para estados locales simples de UI.
- Uso excesivo del pipe `async` en plantillas cuando un Signal (`toSignal`) proporciona una sintaxis más limpia sin preocuparse por fugas de memoria.

---

## 3. Plantillas y Sintaxis de Control de Flujo

### Práctica Moderna: Built-in Control Flow (`@if`, `@for`, `@switch`)

La sintaxis nativa basada en bloques (`@`) es significativamente más eficiente que las antiguas directivas estructurales y no requiere importaciones adicionales.

```html
@if (isLoading()) {
  <app-spinner />
} @else if (hasError()) {
  <p class="error">Ocurrió un error al cargar los datos.</p>
} @else {
  <ul class="item-list">
    @for (item of items(); track item.id) {
      <li>{{ item.name }}</li>
    } @empty {
      <li>No hay elementos disponibles.</li>
    }
  </ul>
}
```

### Evitar (Obsoleto/Legacy):

- Directivas estructurales antiguas: `*ngIf`, `*ngFor` y `*ngSwitch`.
- La función de trackBy anticuada (`trackBy: trackByIdItem`).

---

## 4. Comunicación entre Componentes (Inputs, Outputs y Modelos)

### Práctica Moderna: Funciones Basadas en Signals

Angular 21 estandariza la declaración de entradas y salidas mediante funciones de Signal, proporcionando consistencia reactiva total.

```typescript
import { Component, input, output, model } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-user-profile',
  template: `
    <div class="profile">
      <h3>{{ username() }}</h3>
      <p>Rol: {{ role() }}</p>
      <input [(ngModel)]="status" />
      <button (click)="onAction()">Notificar</button>
    </div>
  `
})
export class UserProfileComponent {
  username = input.required<string>();
  role = input<string>('Usuario');
  status = model<string>('Activo');
  userNotified = output<void>();

  onAction() {
    this.userNotified.emit();
  }
}
```

### Evitar (Obsoleto/Legacy):

- Decoradores `@Input()`, `@Output()` y `@Input() @Output() model`.

---

## 5. Inyección de Dependencias (DI)

### Práctica Moderna: Función `inject()`

Usa la función `inject()` para resolver dependencias dentro del contexto de inyección.

```typescript
import { Component, inject } from '@angular/core';
import { UserService } from './user.service';

@Component({
  standalone: true,
  selector: 'app-user-list',
  template: `...`
})
export class UserListComponent {
  private userService = inject(UserService);
  users = toSignal(this.userService.getUsers(), { initialValue: [] });
}
```

- **Ventajas:** Facilita la herencia de clases (no es necesario pasar dependencias mediante `super()`), permite crear funciones utilitarias inyectables personalizadas y simplifica la sintaxis del componente.

### Evitar (Prácticas Obsoletas):

- Declarar e inyectar dependencias exclusivamente a través del parámetro del `constructor(private userService: UserService)`.

---

## 6. Configuración de la Aplicación y Enrutamiento

### Práctica Moderna: Configuración Funcional (`app.config.ts`)

Toda la configuración global de la aplicación debe ser declarada mediante APIs funcionales en lugar de módulos masivos.

```typescript
// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

### Carga Perezosa (Lazy Loading) por Componente

```typescript
// src/app/app.routes.ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent)
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
```

### Evitar (Obsoleto/Legacy):

- Uso de `HttpClientModule`, `RouterModule.forRoot()` o inicializaciones basadas en clases tradicionales.
- Interceptores basados en clases que implementan `HttpInterceptor`. (Usar interceptores funcionales en su lugar).

---

## 7. Directivas Deferibles (`@defer`)

### Práctica Moderna: Carga Diferida en Plantillas

Usa `@defer` para reducir el tamaño inicial del bundle cargando componentes pesados de manera diferida según interacciones del usuario o condiciones de scroll.

```html
@defer (on viewport) {
  <app-heavy-charts-component [data]="analyticsData()" />
} @placeholder {
  <div class="skeleton-loader">Cargando sección de gráficos...</div>
} @loading (after 100ms; minimum 500ms) {
  <app-spinner />
} @error {
  <p>Error al cargar los gráficos dinámicos.</p>
}
```

---

## 8. Autenticacion JWT y Control de Acceso por Rol

### Estructura de archivos

```text
src/app/core/auth/
├── auth.service.ts          # Estado de sesion y rol (Signals)
├── auth.interceptor.ts      # Adjunta el Bearer token a cada peticion HTTP
└── guards.ts                # Guardias funcionales por rol
```

### `AuthService` — estado de sesion con Signals

El servicio centraliza el token JWT y el rol del usuario. Al hacer login, decodifica el payload del token para extraer el rol sin necesidad de una peticion adicional al backend.

```typescript
// src/app/core/auth/auth.service.ts
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';

const ROL_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const TOKEN_KEY = 'carfix_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly router = inject(Router);

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _rol   = signal<string | null>(this.extraerRol(localStorage.getItem(TOKEN_KEY)));

  readonly token           = this._token.asReadonly();
  readonly rol             = this._rol.asReadonly();
  readonly estaAutenticado = computed(() => this._token() !== null);
  readonly esAdmin         = computed(() => this._rol() === 'Administrador');
  readonly esJefe          = computed(() => this._rol() === 'JefeMecanicos' || this.esAdmin());
  readonly esMecanico      = computed(() => this.estaAutenticado());

  guardarSesion(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
    this._rol.set(this.extraerRol(token));
  }

  cerrarSesion(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
    this._rol.set(null);
    this.router.navigate(['/login']);
  }

  private extraerRol(token: string | null): string | null {
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload[ROL_CLAIM] ?? null;
    } catch {
      return null;
    }
  }
}
```

> `localStorage` persiste la sesion entre recargas. Para aplicaciones con requisitos de seguridad mas estrictos considerar `sessionStorage` o cookies HttpOnly.

### Interceptor HTTP — adjuntar el Bearer token

```typescript
// src/app/core/auth/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token();

  if (!token) return next(req);

  return next(req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  }));
};
```

Se registra en `app.config.ts` (ya incluido en la seccion 6):
```typescript
provideHttpClient(withInterceptors([authInterceptor]))
```

### Guardias funcionales por rol

```typescript
// src/app/core/auth/guards.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const autenticadoGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() || inject(Router).createUrlTree(['/login']);
};

export const soloAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.esAdmin() || inject(Router).createUrlTree(['/sin-acceso']);
};

export const soloJefeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.esJefe() || inject(Router).createUrlTree(['/sin-acceso']);
};
```

Uso en rutas:

```typescript
// src/app/app.routes.ts
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    canActivate: [autenticadoGuard],
    children: [
      // Accesibles por todos los roles
      { path: 'clientes',    loadComponent: () => import('./features/clientes/clientes.component').then(m => m.ClientesComponent) },
      { path: 'vehiculos',   loadComponent: () => import('./features/vehiculos/vehiculos.component').then(m => m.VehiculosComponent) },
      { path: 'ordenes',     loadComponent: () => import('./features/ordenes/ordenes.component').then(m => m.OrdenesComponent) },
      { path: 'facturas',    loadComponent: () => import('./features/facturas/facturas.component').then(m => m.FacturasComponent) },
      { path: 'repuestos',   loadComponent: () => import('./features/repuestos/repuestos.component').then(m => m.RepuestosComponent) },
      { path: 'reparaciones',loadComponent: () => import('./features/reparaciones/reparaciones.component').then(m => m.ReparacionesComponent) },

      // Solo Jefe de Mecánicos y Administrador
      { path: 'catalogo-reparaciones', loadComponent: () => import('./features/catalogo-reparaciones/catalogo-reparaciones.component').then(m => m.CatalogoReparacionesComponent), canActivate: [soloJefeGuard] },
      { path: 'catalogo-repuestos',    loadComponent: () => import('./features/catalogo-repuestos/catalogo-repuestos.component').then(m => m.CatalogoRepuestosComponent),       canActivate: [soloJefeGuard] },
      { path: 'usuarios',              loadComponent: () => import('./features/usuarios/usuarios.component').then(m => m.UsuariosComponent),                                    canActivate: [soloJefeGuard] },

      // Solo Administrador
      { path: 'configuracion', loadComponent: () => import('./features/configuracion/configuracion.component').then(m => m.ConfiguracionComponent), canActivate: [soloAdminGuard] },

      { path: '', redirectTo: 'ordenes', pathMatch: 'full' }
    ]
  },
  { path: 'sin-acceso', loadComponent: () => import('./features/sin-acceso/sin-acceso.component').then(m => m.SinAccesoComponent) },
  { path: '**', redirectTo: 'ordenes' }
];
```

### Control de visibilidad en plantillas

Ocultar elementos del menu segun el rol del usuario autenticado. Ocultar en el menu es UX; la guardia en la ruta es la seguridad real — ambas son necesarias.

```typescript
// src/app/features/nav/nav.component.ts
@Component({
  standalone: true,
  selector: 'app-nav',
  imports: [RouterLink],
  template: `
    <nav>
      <!-- Accesibles por todos los roles autenticados -->
      <a routerLink="/clientes">Clientes</a>
      <a routerLink="/vehiculos">Vehiculos</a>
      <a routerLink="/ordenes">Ordenes de servicio</a>
      <a routerLink="/facturas">Facturas</a>
      <a routerLink="/repuestos">Repuestos</a>
      <a routerLink="/reparaciones">Reparaciones</a>

      <!-- Solo Jefe de Mecanicos y Administrador -->
      @if (auth.esJefe()) {
        <a routerLink="/catalogo-reparaciones">Catalogo de reparaciones</a>
        <a routerLink="/catalogo-repuestos">Catalogo de repuestos</a>
        <a routerLink="/usuarios">Usuarios y perfiles</a>
      }

      <!-- Solo Administrador -->
      @if (auth.esAdmin()) {
        <a routerLink="/configuracion">Configuracion del sistema</a>
      }

      <button (click)="auth.cerrarSesion()">Cerrar sesion</button>
    </nav>
  `
})
export class NavComponent {
  protected readonly auth = inject(AuthService);
}
```

### Jerarquia de roles (referencia rapida)

| Signal          | Administrador | JefeMecanicos | Mecanico |
|-----------------|:---:|:---:|:---:|
| `esAdmin()`     | ✓   |     |     |
| `esJefe()`      | ✓   | ✓   |     |
| `esMecanico()`  | ✓   | ✓   | ✓   |

---

## 9. Resumen de Migración Rápida (Cheat Sheet)

| Elemento Viejo / Obsoleto                          | Reemplazo Moderno (Angular 21)                         |
|----------------------------------------------------|--------------------------------------------------------|
| `@NgModule({ ... })`                               | `standalone: true` en cada declaración                 |
| `*ngIf="condicion"`                                | `@if (condicion) { ... }`                              |
| `*ngFor="let item of items"`                       | `@for (item of items; track item.id) { ... }`          |
| `@Input() valor: string;`                          | `valor = input<string>();`                             |
| `@Output() cambio = new EventEmitter();`           | `cambio = output<string>();`                           |
| `constructor(private service: Svc)`                | `private service = inject(Svc);`                       |
| `HttpClientModule`                                 | `provideHttpClient()`                                  |
| Interceptores de Clase (`HttpInterceptor`)         | Interceptores Funcionales (`HttpFnInterceptor`)        |

# Guia para IA — Diseno Grafico, Interfaz y PWA (Angular)

> **Proposito:** explicarle a un asistente de IA como se diseno y configuro la interfaz de CAR_FIX en
> Angular 21 para funcionar como **PWA instalable** que sirve igual en **navegador de escritorio** y en
> **celular**, para replicar el mismo enfoque en proyectos futuros.
>
> Complementa: `CLAUDE.md` §11 (patron de pantallas, detalle de signals y template) y
> `.claude/angular_buenas_practicas.md` (convenciones de Angular moderno).

---

## 1. Principios de Diseno

1. **Una sola aplicacion, dos experiencias.** El mismo codigo Angular sirve escritorio y celular; se adapta por CSS (breakpoint) y, cuando el flujo debe ser distinto, por un servicio de deteccion (`EsMovilService`).
2. **Escritorio = mantenimiento completo; celular = trabajo en el taller.** En escritorio se aterriza en `/ordenes` con tablas completas. En celular se aterriza en `/chat` (chat de voz) y las tablas muestran solo columnas clave.
3. **Sin librerias de UI.** No se usa Angular Material, Bootstrap ni Tailwind: un unico `styles.css` global con variables CSS y clases semanticas en espannol. Menos peso, control total, cero conflictos de version.
4. **La IA asiste, el usuario confirma.** Todo dato que llena la IA (foto, voz) se muestra prellenado y editable; nunca se guarda sin confirmacion.
5. **Instalable y actualizable.** Manifest + Service Worker de Angular; banner de instalacion y de nueva version.

---

## 2. Stack del Frontend

| Pieza | Eleccion |
|---|---|
| Framework | Angular 21, componentes **standalone**, **Signals**, control de flujo `@if/@for` |
| Build | `@angular/build:application` (salida en `dist/<proyecto>/browser`) |
| PWA | `@angular/service-worker` (`provideServiceWorker`) + `manifest.webmanifest` |
| Estilos | `src/styles.css` global (≈640 lineas) + variables CSS; sin framework CSS |
| Mapas | `leaflet` (tiles CARTO) para ubicaciones GPS |
| Formularios | Signals + eventos `(input)` — **no** `[(ngModel)]` ni Reactive Forms |
| Hosting | Azure Static Web Apps (ver `GUIA_AZURE_DESPLIEGUE.md`) |

Estructura relevante:

```
src/
├── index.html                 # <link rel="manifest">, viewport
├── styles.css                 # sistema de diseno completo
├── environments/environment.ts
└── app/
    ├── app.config.ts          # router, http, service worker, inicializador
    ├── app.routes.ts          # lazy loading + guards por rol
    ├── core/
    │   ├── auth/              # AuthService, interceptor, guards, BiometriaService (WebAuthn)
    │   ├── comun/             # EsMovilService, SaludService
    │   ├── pwa/               # InstalacionPwaService, InstallPrompt, ActualizacionPwa
    │   └── voz/               # Reconocimiento de voz (Web Speech API)
    ├── features/              # una carpeta por pantalla (shell, nav, inicio, clientes, ...)
    ├── models/  services/  shared/  utils/
public/
├── manifest.webmanifest
├── favicon.ico / favicon.svg
└── icons/icon-{72,96,128,144,152,192,384,512}x*.png
staticwebapp.config.json       # fallback SPA en Azure
ngsw-config.json               # cache del service worker
```

---

## 3. Sistema de Diseno Visual

### 3.1 Tokens (variables CSS en `:root`)

```css
:root {
  --color-sidebar-bg:    #16213e;   /* azul noche: menu lateral, theme_color de la PWA */
  --color-sidebar-hover: #0f3460;
  --color-sidebar-activo:#0f3460;
  --color-acento:        #f5c518;   /* amarillo taller: resaltes, boton instalar */
  --color-acento-oscuro: #d4a800;
  --color-texto-claro:   #dde1e7;
  --color-fondo:         #f0f2f5;   /* fondo de la app */
  --color-tarjeta:       #ffffff;
  --color-borde:         #dee2e6;
  --color-texto:         #212529;
  --color-texto-suave:   #6c757d;
  --color-primario:      #0f3460;   /* botones de accion, titulos */
  --color-exito:         #198754;
  --color-peligro:       #e94560;
  --radio-borde:         6px;
  --sombra:              0 2px 8px rgba(0,0,0,.1);
  --ancho-sidebar:       240px;
}
body { font-family: 'Segoe UI', system-ui, sans-serif; font-size: 14px; line-height: 1.5; }
```

**Reglas:**
- Ningun componente usa colores literales: siempre `var(--color-...)`. Cambiar la marca de un sistema nuevo = cambiar este bloque + `theme_color`/`background_color` del manifest + iconos.
- Tipografia del sistema (`system-ui`): no se descargan fuentes, carga instantanea y aspecto nativo en cada plataforma.
- Paleta: un color oscuro de marca (menu), un acento llamativo (amarillo) y colores semanticos (exito/peligro).

### 3.2 Badges de estado

Cada estado de negocio tiene un color fijo y consistente en toda la app:

| Clase | Color | Ordenes (`badge-N`) | Facturas (`badge-f-N`) |
|---|---|---|---|
| amarillo `#fff3cd` | en espera | 1 Cotizacion | 1 Cotizacion |
| azul `#cfe2ff` | en curso | 2 Recibido | — |
| rojo `#fde8ec` | atencion | 3 Reparando | 2 Pendiente |
| verde `#d1e7dd` | terminado | 4 Finalizado | 3 Pagada |
| gris `#e9ecef` | cerrado | 5 Entregado | — |

```html
<span [class]="'badge badge-' + orden.estadoOrdenId">{{ orden.estadoOrdenDescripcion }}</span>
```

### 3.3 Clases semanticas (catalogo)

Todas definidas en `styles.css`; las pantallas nuevas **solo** usan estas (lista completa en `CLAUDE.md` §12.9):

| Grupo | Clases |
|---|---|
| Estructura | `.pantalla-contenedor`, `.pantalla-encabezado`, `.pantalla-titulo`, `.acciones-barra` |
| Botones | `.btn-accion` (azul), `.btn-accion.btn-peligro` (rojo), `.btn-secundario` (gris) |
| Listado | `.filtros-barra`, `.filtro-input`, `.tabla-contenedor`, `.tabla-datos`, `.fila-seleccionada`, `.texto-vacio`, `.texto-cargando`, `.paginacion` |
| Formulario | `.formulario-panel`, `.formulario-titulo`, `.form-grid`, `.form-grid.solo-lectura`, `.form-grupo`, `.form-grupo-ancho`, `.form-acciones`, `.error-form`, `.banner-solo-lectura` |
| Movil | `.col-oculta-movil` |
| PWA | `.install-banner`, `.install-btn-instalar` |

> En CAR_FIX quedaron pantallas antiguas con clases legacy (`.pagina-header`, `.btn.btn-primario`). En un proyecto nuevo, definir el catalogo de clases **antes** de la primera pantalla para no tener dos sistemas.

---

## 4. Layout y Navegacion

### 4.1 Shell

Todas las rutas autenticadas cuelgan de un `ShellComponent` (lazy, con `autenticadoGuard`) que monta el menu, el contenido y los banners globales de PWA:

```ts
template: `
  <div class="layout">
    <app-nav />
    <main class="contenido-principal"><router-outlet /></main>
  </div>
  <app-install-prompt />
  <app-actualizacion-pwa />
`
```

### 4.2 Menu: sidebar en escritorio, hamburguesa en celular

`NavComponent` renderiza **una sola** barra lateral que cambia de comportamiento por CSS:

| | Escritorio (>768px) | Celular (≤768px) |
|---|---|---|
| Sidebar | Fija a la izquierda, 240px | Oculta con `transform: translateX(-100%)`, se desliza al abrir (`.abierto`, transicion .28s) |
| Topbar | Oculta | Visible (56px): boton hamburguesa + logo |
| Overlay | — | Fondo oscuro semitransparente; al tocarlo cierra el menu |
| Contenido | `margin-left: var(--ancho-sidebar)` | `margin-left: 0; padding-top: 72px` |

- Estado del menu: `menuAbierto = signal(false)`; cada enlace llama `cerrar()` para que el menu se cierre al navegar en celular.
- Opciones del menu filtradas por rol con `@if (auth.esJefe())` / `@if (auth.esAdmin())` (UX); la seguridad real son los guards de ruta.
- Opcion "🏠 Inicio" (chat) solo en celular; "📲 Instalar aplicacion" solo si `mostrarOpcionInstalar()`.
- Iconografia: **emojis** en el menu y SVG inline para el logo — cero dependencias de librerias de iconos.

### 4.3 Pantalla de aterrizaje segun dispositivo

```ts
// features/inicio/inicio.component.ts — ruta '' dentro del shell
export class InicioComponent {
  constructor() {
    const esMovil = inject(EsMovilService).esMovil();
    inject(Router).navigateByUrl(esMovil ? '/chat' : '/ordenes', { replaceUrl: true });
  }
}
```

### 4.4 Rutas

- Todas con `loadComponent` (lazy loading por pantalla → carga inicial pequenna, clave en celular).
- `provideRouter(routes, withComponentInputBinding(), withViewTransitions())` — las transiciones nativas dan sensacion de app.
- `{ path: '**', redirectTo: '' }`.

---

## 5. Diseno Responsivo

### 5.1 Breakpoints

| Breakpoint | Uso |
|---|---|
| `max-width: 768px` | **Movil.** Unico breakpoint "logico": menu hamburguesa, columnas ocultas, paginas de 15 filas, encabezados apilados, grids de detalle a 1 columna |
| `max-width: 400px` | Ajuste fino para celulares angostos (referencia real: **360×800**): menos padding, fuentes y botones mas compactos |

### 5.2 Deteccion en TypeScript — `EsMovilService`

Cuando la diferencia no es solo visual (otra ruta de inicio, ocultar un boton, otro tamanno de pagina), se usa el **mismo** breakpoint via `matchMedia`, expuesto como signal:

```ts
const CONSULTA_MOVIL = '(max-width: 768px)';

@Injectable({ providedIn: 'root' })
export class EsMovilService {
  private readonly mediaMovil = window.matchMedia(CONSULTA_MOVIL);
  private readonly _esMovil = signal(this.mediaMovil.matches);
  readonly esMovil = this._esMovil.asReadonly();

  constructor() {
    this.mediaMovil.addEventListener('change', e => this._esMovil.set(e.matches));  // rotar / redimensionar
  }
}
```

Regla: el valor `768px` debe ser **identico** en CSS y en TS. Nunca detectar movil por `userAgent` para decisiones de layout.

### 5.3 Tablas en celular

- Clase `col-oculta-movil` en `<th>`/`<td>` que sobran: en celular solo quedan las columnas identificativas (ej. Clientes: `#`, Nombre, Telefono; Ordenes: `#`, Placa, Vehiculo, Cliente).
- Si quedan pocas columnas, anular el `min-width` forzado de la tabla con una clase especifica (`.tabla-clientes`) para evitar scroll horizontal.
- Ver el detalle completo = tocar la fila (abre el formulario en modo `ver`).

### 5.4 Paginacion adaptable (no scroll infinito)

```ts
tamanoPagina   = computed(() => this.esMovil() ? 15 : 25);
paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / this.tamanoPagina()));
itemsMostrados = computed(() => {
  const inicio = this.paginaActual() * this.tamanoPagina();
  return this.itemsFiltrados().slice(inicio, inicio + this.tamanoPagina());
});
```

> **Leccion:** se probo scroll infinito en celular y se descarto: el re-render durante la carga interferia con el `(click)` de la fila (habia que tocar dos veces). Paginacion con paginas mas pequennas resolvio el problema. Al cambiar filtros o el tamanno de pantalla, `paginaActual` vuelve a 0.

### 5.5 Otros ajustes moviles

- Encabezado de pantalla: titulo arriba, botones abajo a todo el ancho.
- Filtros por estado (pastillas) en filas de 3 para no desbordar.
- Formularios: `.form-grid` pasa a una columna.
- Botones con area tactil suficiente (padding ≥ 7px vertical incluso en 360px).
- `<meta name="viewport" content="width=device-width, initial-scale=1">` en `index.html`.

---

## 6. Patron de Pantallas (resumen)

Detalle completo en `CLAUDE.md` §11. Lo esencial para disennar pantallas nuevas:

- **Lista y formulario se reemplazan** (no modal): con el formulario abierto la lista se oculta → funciona igual en 360px que en escritorio.
- Estado con `modo = signal<'ver' | 'crear' | 'editar' | 'estado'>` (nunca booleanos sueltos).
- Modo `ver`: banner amarillo "🔒 Solo lectura" + `.form-grid.solo-lectura` (`pointer-events: none`), sin atributos `readonly` por campo.
- Un clic selecciona la fila; doble clic (o clic en celular) abre el detalle.
- Inputs con `[value]="campo()" (input)="campo.set($any($event.target).value)"`.
- Errores del backend con el helper `extraerError()` (lee `ProblemDetails.detail`).
- Sub-entidades en tabs dentro del detalle (patron Facturas: Reparaciones / Repuestos).
- Listas vacias resaltadas en amarillo para que el mecanico note que falta informacion.

---

## 7. Configuracion PWA

### 7.1 Instalar el soporte

```bash
ng add @angular/pwa      # crea manifest, ngsw-config.json, iconos base y registra el service worker
```

Es el punto de partida recomendado; en CAR_FIX el resultado final de cada archivo quedo como se describe a continuacion.

### 7.2 `public/manifest.webmanifest`

```json
{
  "name": "CAR FIX — Sistema de Taller",
  "short_name": "CAR FIX",
  "description": "Sistema de administracion de taller mecanico",
  "display": "standalone",
  "orientation": "portrait",
  "scope": "./",
  "start_url": "./",
  "background_color": "#16213e",
  "theme_color": "#16213e",
  "icons": [
    { "src": "icons/icon-72x72.png",   "sizes": "72x72",   "type": "image/png", "purpose": "maskable any" },
    "... 96, 128, 144, 152, 192, 384 ...",
    { "src": "icons/icon-512x512.png", "sizes": "512x512", "type": "image/png", "purpose": "maskable any" }
  ]
}
```

- `display: standalone` → sin barra del navegador; `orientation: portrait` → pensado para usar con una mano en el taller.
- `theme_color`/`background_color` = color del sidebar (splash y barra de estado coherentes con la app).
- Iconos en 8 tamannos; **192 y 512 son obligatorios** para que Chrome ofrezca instalar. Disennarlos con margen de seguridad (zona segura ~80%) porque son `maskable`.
- `index.html`: `<link rel="manifest" href="manifest.webmanifest" />`.

### 7.3 Service Worker — `app.config.ts`

```ts
provideServiceWorker('ngsw-worker.js', {
  enabled: !isDevMode(),                         // solo en build de produccion
  registrationStrategy: 'registerWhenStable:30000',
}),
```

`angular.json` → configuracion `production`: `"serviceWorker": "ngsw-config.json"`.

### 7.4 `ngsw-config.json` — que se cachea

```json
{
  "index": "/index.html",
  "assetGroups": [
    { "name": "app", "installMode": "prefetch",
      "resources": { "files": ["/favicon.ico", "/index.csr.html", "/index.html", "/manifest.webmanifest", "/*.css", "/*.js"] } },
    { "name": "assets", "installMode": "lazy", "updateMode": "prefetch",
      "resources": { "files": ["/**/*.(svg|cur|jpg|jpeg|png|apng|webp|avif|gif|otf|ttf|woff|woff2)"] } }
  ]
}
```

- Se cachea el **cascaron** de la app (JS/CSS/HTML/iconos) → abre instantaneo y sobrevive a mala senal.
- **No** hay `dataGroups`: las llamadas al API siempre van a la red (datos de facturas/ordenes deben estar frescos). La app no esta disennada para trabajar offline con datos.

### 7.5 Instalacion — `InstalacionPwaService` (`core/pwa/`)

Servicio `providedIn: 'root'` que centraliza la logica, porque `beforeinstallprompt` se dispara **una sola vez** y debe poder usarlo tanto el banner como el menu:

```ts
window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault();                 // evita el mini-infobar de Chrome; lo mostramos nosotros
  this.eventoInstalacion = e;
  this._puedeInstalarAndroid.set(true);
});
window.addEventListener('appinstalled', () => { this._yaInstalado.set(true); ... });

readonly mostrarOpcionInstalar = computed(() =>
  !this._yaInstalado() && (this._puedeInstalarAndroid() || this._esIOS()));
```

| Plataforma | Comportamiento |
|---|---|
| Android / Chrome / Edge | Banner automatico inferior (`InstallPromptComponent`) + opcion de menu → `evento.prompt()` nativo |
| iOS / iPadOS (Safari) | No existe API de instalacion → la opcion de menu abre instrucciones: "Toca Compartir y selecciona *Agregar a pantalla de inicio*" |
| Ya instalada | Se oculta todo (`display-mode: standalone` o `navigator.standalone`) |

Detalles:
- Deteccion iOS: `/iPad|iPhone|iPod/` en `userAgent` **o** `platform === 'MacIntel' && maxTouchPoints > 1` (iPadOS moderno se presenta como Mac).
- El banner automatico se puede cerrar y recuerda la decision en `sessionStorage['install-prompt-cerrado']`.
- La opcion "Instalar" esta en el menu para **todos los roles** (no en Configuracion, que es solo Admin).

### 7.6 Actualizaciones — `ActualizacionPwaService`

```ts
this.swUpdate.versionUpdates.subscribe(evento => {
  if (evento.type === 'VERSION_READY') this._actualizacionDisponible.set(true);
});
actualizar() { window.location.reload(); }
```

Cuando se despliega una nueva version, el usuario ve un banner "Hay una nueva version disponible → **Actualizar**" (reutiliza las clases `.install-banner`). Sin esto, una PWA instalada puede quedarse con la version vieja hasta cerrar todas las pestannas.

### 7.7 Requisitos de hosting

- **HTTPS obligatorio** para service worker, camara, microfono y WebAuthn (`localhost` es la excepcion).
- `staticwebapp.config.json` con `navigationFallback` → `/index.html` (rutas SPA) excluyendo assets.
- Para probar en un celular dentro de la red local se usa la skill `/arrancar-app` (certificado autofirmado + IP Wi-Fi).

---

## 8. Capacidades del Dispositivo (Web APIs)

| Capacidad | Implementacion | Nota de diseno |
|---|---|---|
| **Camara** (escanear tarjeta de circulacion / factura) | `<input type="file" accept="image/*" capture="environment" hidden>` disparado por un boton | Mismo codigo: en celular abre la camara trasera, en escritorio el selector de archivos |
| **Compresion de fotos** | `utils/imagen.util.ts` → `comprimirImagen()`: canvas, lado maximo **1600px**, JPEG calidad **0.8** | Reduce MB antes de subir por datos moviles; el backend limita a 10MB |
| **Voz** | Web Speech API (`SpeechRecognition`/`webkitSpeechRecognition`, `es-CR`); tipos en `src/types/speech-recognition.d.ts` | Si `!soportado()`, el boton se deshabilita con `title` explicativo. Fiable en Chrome/Android; irregular en Safari; no en Firefox |
| **Biometria** (huella / Face ID) | WebAuthn (`navigator.credentials.create/get`) en `BiometriaService`; credencial + token de refresco en **IndexedDB** (`carfix-biometria`) | Solo si `isUserVerifyingPlatformAuthenticatorAvailable()`; desbloquea con huella sin redigitar contrasenna |
| **Mapas / GPS** | Leaflet (`shared/components/mapa-ubicacion`) con tiles **CARTO** | Los tiles de OpenStreetMap devolvieron 403 → se cambio a CARTO |

---

## 9. Experiencia Movil: Chat de Voz

En celular la pantalla principal es un chat (`features/chat-taller/`) con un microfono grande centrado, porque en el taller el mecanico tiene las manos ocupadas:

- Cada accion (crear cliente, vehiculo, orden, enviar factura) se dicta como un comando; la IA resume lo entendido y **espera confirmacion hablada** ("si"/"no") antes de ejecutar.
- El "si/no" se interpreta localmente (`interprete-confirmacion.util.ts`), sin llamar a la IA.
- El chat recuerda en signals los IDs creados en la sesion para encadenar pasos ("ahora crea un vehiculo..." usa el ultimo cliente).

Detalle en `CLAUDE.md` §12.13–12.14. Patron reutilizable: **en movil, priorizar un flujo guiado/conversacional sobre formularios largos**.

---

## 10. Rendimiento y Arranque

- Lazy loading por pantalla + componentes standalone → bundle inicial pequenno (con `budgets` en `angular.json`).
- Sin librerias de UI ni fuentes externas.
- **Despertar la BD al abrir la app:** Azure SQL Serverless se pausa sola; `provideAppInitializer` llama `GET /api/salud` en segundo plano sin bloquear el arranque, asi la BD ya esta despierta cuando el usuario termina de iniciar sesion:
  ```ts
  provideAppInitializer(() => { inject(SaludService).activar().subscribe({ error: () => {} }); }),
  ```

---

## 11. Pruebas en Dispositivo

| Que probar | Como |
|---|---|
| Layout movil | DevTools → modo dispositivo **360×800**; ademas un celular real |
| Service worker / instalacion | Build de produccion (`ng build`), servido por HTTPS (Azure o `/arrancar-app`); DevTools → Application → Manifest / Service Workers; Lighthouse → PWA |
| Instalacion Android | Chrome → banner o menu "Instalar aplicacion" |
| Instalacion iOS | Safari → Compartir → Agregar a pantalla de inicio |
| Actualizacion | Desplegar una version nueva con la app instalada abierta → debe aparecer el banner |
| Camara / voz / biometria | Solo en celular real y por HTTPS |

---

## 12. Checklist para un Proyecto Nuevo

- [ ] Definir tokens de color en `:root` y catalogo de clases semanticas antes de la primera pantalla
- [ ] Un solo breakpoint movil (768px) compartido entre CSS y `EsMovilService`; ajuste fino a 400px
- [ ] Shell con sidebar fija (escritorio) / hamburguesa + overlay (movil)
- [ ] Rutas lazy + guards por rol; pantalla de inicio distinta segun dispositivo si el uso lo amerita
- [ ] Tablas con `col-oculta-movil` y paginacion 25/15 (no scroll infinito)
- [ ] Lista y formulario que se reemplazan (sin modales), modos `ver/crear/editar`
- [ ] `ng add @angular/pwa`; personalizar manifest (nombre, colores, `standalone`, `portrait`), iconos 72→512 maskable
- [ ] `ngsw-config.json`: cachear solo el cascaron; API siempre a red
- [ ] `InstalacionPwaService` (Android prompt + instrucciones iOS) y `ActualizacionPwaService` con banner
- [ ] `staticwebapp.config.json` con `navigationFallback`
- [ ] Camara con `capture="environment"` + compresion en cliente
- [ ] Funciones opcionales (voz, biometria) detectan soporte y se degradan con mensaje claro
- [ ] Inicializador que despierta el backend/BD si es serverless
- [ ] Probar en 360×800 y en celular real por HTTPS

## 13. Reglas para la IA (resumen)

1. Usar solo las clases de `styles.css` y variables `--color-*`; no agregar librerias de UI ni colores literales.
2. Toda pantalla nueva debe verse bien en 360px: columnas ocultas, paginacion de 15, botones a todo el ancho.
3. El breakpoint es 768px en todas partes; para logica usar `EsMovilService`, no `userAgent`.
4. No usar `[(ngModel)]` con signals; usar `(input)` + `.set()`.
5. Cualquier API del dispositivo (camara, voz, biometria) debe detectar soporte y degradarse sin romper la pantalla.
6. No cachear respuestas del API en el service worker sin una decision explicita del usuario.
7. Probar el service worker solo con build de produccion; en `ng serve` esta deshabilitado.

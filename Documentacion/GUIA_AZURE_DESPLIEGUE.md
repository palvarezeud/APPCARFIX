# Guia para IA — Infraestructura Azure y Despliegue

> **Proposito:** explicarle a un asistente de IA (Claude Code u otro) como esta montado CAR_FIX en Azure,
> como se crearon los recursos y como se despliega, para poder **operar este sistema** y **replicar el
> mismo esquema en sistemas futuros** sin repetir los errores que costaron tiempo.
>
> Fuentes: `DeployAzure.MD` (runbook original con el detalle del troubleshooting), skills
> `.claude/skills/desplegar-azure` y `.claude/skills/sincronizar-esquema-azure`, y la configuracion
> real leida con Azure CLI el 2026-10-21.
>
> **Este documento no contiene secretos.** Contrasennas, llaves y cadenas de conexion completas viven
> solo en Azure (App Settings / Connection Strings) y en `dotnet user-secrets` local.

---

## 1. Arquitectura

```
                 GitHub (palvarezeud/APPCARFIX, privado, rama master)
                    │ push                                  │ (manual)
                    ▼                                       │
        GitHub Actions (workflow SWA)                       │ dotnet publish + az webapp deploy
                    │                                       ▼
┌───────────────────────────────┐   HTTPS /api   ┌───────────────────────────────┐   TDS 1433   ┌──────────────────────────┐
│ Azure Static Web Apps         │ ─────────────▶ │ Azure App Service             │ ───────────▶ │ Azure SQL Database       │
│ appCarFixWeb (Free, East US 2)│   CORS         │ appCarFix (.NET 10, F1 Free,  │  SQL Auth    │ appcarfixdb2             │
│ Angular 21 (archivos estaticos)│                │ Windows, Canada East)         │              │ (Serverless GP_S_Gen5,   │
└───────────────────────────────┘                └───────────────────────────────┘              │  1 vCore, Canada Central)│
                                                        │                                       └──────────────────────────┘
                                                        ├──▶ API de Anthropic (escaneo IA / voz)
                                                        └──▶ SMTP Gmail (envio de facturas)
```

| Recurso | Nombre | Plan / SKU | Region | URL / Host |
|---|---|---|---|---|
| Resource Group | `appCarFixRG` | — | — | — |
| App Service Plan | `ASP-appCarFixRG-8dc5` | F1 (Free), Windows | Canada East | — |
| App Service (backend) | `appCarFix` | runtime .NET 10, HTTPS Only | Canada East | `https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net` |
| Static Web App (frontend) | `appCarFixWeb` | Free | East US 2 | `https://icy-wave-040fef60f.7.azurestaticapps.net` |
| Azure SQL Server (logico) | `appcarfixinstance` | — | Canada Central | `appcarfixinstance.database.windows.net` |
| Azure SQL Database (en uso) | `appcarfixdb2` | General Purpose Serverless, Gen5, 1 vCore | Canada Central | — |
| Azure SQL Database (original) | `appcarfixdb` | General Purpose Serverless, Gen5, 1 vCore | Canada Central | — |
| Usuario SQL de la app | `appcarfixuser` | SQL Authentication | — | — |

> **La app usa `appcarfixdb2`** (verificado el 2026-10-21 leyendo la connection string `CarFix` del App Service). `appcarfixdb` fue la BD original (la que menciona `DeployAzure.MD`); no tocarla sin confirmar con el usuario. Ante la duda, consultar siempre la connection string real en vez de asumir el nombre.

**Por que regiones distintas:** Static Web Apps no esta disponible en Canada East/Central, por eso el frontend esta en East US 2. No es un problema: el frontend son archivos estaticos servidos por CDN; la latencia relevante es App Service ↔ SQL (ambos en Canada).

---

## 2. Conexion con Azure desde la maquina local

### 2.1 Azure CLI

```powershell
winget install Microsoft.AzureCLI
az login --tenant <tenant-id>          # interactivo: lo ejecuta el USUARIO, nunca la IA por su cuenta
az account show                         # verificar suscripcion activa antes de cualquier operacion
```

### 2.2 Restriccion critica — MFA step-up del tenant (UNED)

El tenant exige **MFA "step-up" (Conditional Access, authentication context `p1`) para cualquier operacion de escritura** de control plane (`az staticwebapp create`, `az webapp cors add`, crear recursos, etc.). El CLI falla con:

```
AADSTS50076: ... you must use multi-factor authentication ...
```

aunque se repita `az login` limpio. **Patron a seguir:**

| Tipo de operacion | Herramienta |
|---|---|
| Lectura (`az webapp show`, `az ... list`, leer connection strings) | Azure CLI |
| Crear/modificar recursos, CORS de plataforma, App Settings | **Portal de Azure** (ahi el step-up se resuelve en el navegador) |
| Desplegar codigo del backend (`az webapp deploy`) | Azure CLI (funciona: es data plane via Kudu) |

**Reglas para la IA:**
- No ejecutar `az login` ni `az account set` sin que el usuario lo pida; si `az account show` falla o apunta a otra suscripcion, **detenerse y avisar**.
- Si una operacion de escritura falla con `AADSTS50076`, no reintentar en bucle: indicar al usuario el paso exacto a hacer en el Portal.

### 2.3 Otras credenciales locales

| Archivo / mecanismo | Uso | Regla |
|---|---|---|
| `appCarFix.PublishSettings` (raiz del repo) | Credenciales de despliegue (usuario `$appCarFix`) para Kudu / Web Deploy | En `.gitignore`. **Nunca** commitear ni copiar su contenido a documentos |
| `dotnet user-secrets` (proyecto `CarFix.WebApi`) | `Jwt:Llave`, `Smtp:Contrasenna`, `Anthropic:ApiKey` y opcionalmente `ConnectionStrings:CarFix` en desarrollo | Tiene prioridad sobre `appsettings.json` en `Development` |
| `dev-certs/` | Certificado autofirmado para correr el backend en la red local | En `.gitignore`; no se usa en Azure |

---

## 3. Base de Datos — Azure SQL

### 3.1 Creacion (Portal → Create a resource → SQL Database)

| Campo | Valor usado |
|---|---|
| Resource Group | `appCarFixRG` |
| Database name | `appcarfixdb` |
| Server | nuevo: `appcarfixinstance`, region Canada Central |
| Autenticacion del servidor | SQL Authentication (usuario administrador + contrasenna) |
| Compute + storage | General Purpose → **Serverless**, Gen5, 1 vCore (se pausa sola cuando no hay uso → bajo costo) |
| Networking | Public endpoint + **"Allow Azure services and resources to access this server" = Yes** (crea la regla `AllowAllWindowsAzureIps`) + agregar la IP del cliente actual para administrar desde SSMS |

Usuario de la aplicacion: `appcarfixuser` (SQL Auth). Para sistemas nuevos, crear un usuario propio de la app en vez de usar el admin del servidor:

```sql
-- En la BD de la aplicacion (no en master), conectado como admin:
CREATE USER appusuario WITH PASSWORD = '<contrasenna fuerte>';
ALTER ROLE db_datareader ADD MEMBER appusuario;
ALTER ROLE db_datawriter ADD MEMBER appusuario;
```

### 3.2 Esquema

- **Database First:** el esquema se crea con scripts SQL (ver `Documentacion/ESTANDAR_BASE_DATOS.md`); EF Core no ejecuta migraciones.
- Los cambios de esquema se aplican en Azure **a mano y con confirmacion explicita del usuario** — ninguna skill escribe en la BD de Azure. Scripts versionados en `Backend/Scripts/sql/AAAA-MM-DD_*.sql`.
- **Azure es la fuente de verdad** de esquema y catalogos. La skill `/sincronizar-esquema-azure` compara Azure vs LocalDB y trae a **local** lo que falte (columnas de `Catalogo`/`Sistema` y las tablas `Parametros`, `EstadoOrden`, `EstadoFactura`, `Roles`, `Taller`). Nunca escribe en Azure.
- `appcarfixdb2` (BD en uso) contiene **datos reales**: no usarla como sandbox para pruebas destructivas.

### 3.3 Connection string

Formato ADO.NET de Azure SQL (la contrasenna nunca va al repo):

```
Server=tcp:appcarfixinstance.database.windows.net,1433;Initial Catalog=appcarfixdb2;Persist Security Info=False;User ID=appcarfixuser;Password=<secreto>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

- `Encrypt=True;TrustServerCertificate=False` en Azure (local usa `TrustServerCertificate=True` por LocalDB).
- Obtenerla "en caliente" sin escribirla a disco:
  ```powershell
  az webapp config connection-string list --name appCarFix --resource-group appCarFixRG --query "[?name=='CarFix'].value" -o tsv
  ```
- `Invoke-Sqlcmd` (modulo SQLPS) **no acepta** esa cadena con `-ConnectionString` (palabras clave `Connect Retry Count`/`Interval` no soportadas): parsearla y usar `-ServerInstance/-Database/-Username/-Password` (ver la skill `sincronizar-esquema-azure`, paso 1).

---

## 4. Backend — Azure App Service

### 4.1 Creacion (Portal → Create a resource → Web App)

| Campo | Valor usado |
|---|---|
| Resource Group | `appCarFixRG` |
| Name | `appCarFix` (Azure agrega un sufijo unico al hostname: `appcarfix-hdeyb4dncbdwbchg.canadaeast-01...`) |
| Publish | Code |
| Runtime stack | .NET 10 (LTS) |
| Operating System | Windows (IIS + ASP.NET Core Module) |
| Region | Canada East |
| Pricing plan | F1 Free (nuevo plan `ASP-appCarFixRG-8dc5`) |
| HTTPS Only | Activado |

El certificado HTTPS de `*.azurewebsites.net` lo gestiona Microsoft: no hay que subir certificado.

### 4.2 Configuracion (Portal → `appCarFix` → Configuration / Environment variables)

**Connection strings** (pestanna propia, no Application settings):

| Nombre | Tipo | Valor |
|---|---|---|
| `CarFix` | `SQLAzure` | Cadena de la seccion 3.3 |

ASP.NET Core la mapea automaticamente a `ConnectionStrings:CarFix`. Guardarla reinicia el App Service. No marcar "deployment slot setting" (no hay slots).

**Application settings** (los `:` de `appsettings.json` se escriben como `__`; las claves no distinguen mayusculas):

| Nombre | Contenido |
|---|---|
| `Jwt__Llave` | Llave de firma JWT (minimo 32 caracteres) |
| `Anthropic__ApiKey` | API key de Anthropic (facturacion por token, separada de suscripciones Claude) |
| `Smtp__Host` | Servidor SMTP |
| `Smtp__UsuarioRemitente` | Cuenta remitente |
| `Smtp__Contrasenna` | Contrasenna de aplicacion SMTP |

Los valores no secretos (`Jwt:Emisor`, `Smtp:Puerto`, `Anthropic:Modelo`, etc.) se toman de `appsettings.json`. **En `appsettings.json` los secretos quedan como `""`**: ese archivo viaja en cada publish y esta en el repo.

### 4.3 CORS — dos capas independientes (lo que mas tiempo hizo perder)

| Capa | Donde | Cuando aplica |
|---|---|---|
| **CORS de plataforma** | Portal → `appCarFix` → API → CORS | Inmediato, sin redeploy. Actua **antes** que el codigo |
| **CORS de codigo** | `Program.cs` → `AddCors(...).WithOrigins(...)`, politica `OrigenAngular` | Requiere compilar y republicar |

**Ambas listas deben incluir el origen** (`https://icy-wave-040fef60f.7.azurestaticapps.net`, `http://localhost:4200`).
Sintoma si falta la de plataforma: el preflight `OPTIONS` devuelve `400 The origin '...' is not allowed` aunque el DLL desplegado ya tenga el origen y se haya reiniciado.

Otros detalles:
- `http://localhost:4200` ≠ `https://localhost:4200`: si `ng serve` corre con `--ssl` el origen no coincide. Para probar contra Azure usar `ng serve` **sin** `--ssl`.
- Mejora pendiente: mover la lista de origenes a configuracion (`Cors:OrigenesPermitidos`) en vez de tenerla fija en C#.

### 4.4 Despliegue del backend (manual — no tiene GitHub Actions)

```bash
cd Backend && dotnet build CarFix.slnx                       # nunca desplegar sin compilar

cd Backend/WebApi/CarFix.WebApi
rm -rf publish-tmp publish.zip
dotnet publish -c Release -o ./publish-tmp
```
```powershell
Compress-Archive -Path "Backend\WebApi\CarFix.WebApi\publish-tmp\*" -DestinationPath "Backend\WebApi\CarFix.WebApi\publish.zip" -Force
```
```bash
az webapp deploy --resource-group appCarFixRG --name appCarFix --src-path Backend/WebApi/CarFix.WebApi/publish.zip --type zip
# esperar "Deployment has completed successfully" / "provisioningState": "Succeeded"

rm -rf Backend/WebApi/CarFix.WebApi/publish-tmp Backend/WebApi/CarFix.WebApi/publish.zip   # limpiar siempre
```

Alternativa desde Visual Studio: boton **Publish** con el perfil `appCarFix - Web Deploy` o `appCarFix - Zip Deploy` (`Properties/PublishProfiles/`).

Verificacion:
```bash
curl -s -o /dev/null -w "HTTP:%{http_code}\n" "https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api/salud"
# + login real y una llamada autenticada que lea BD (confirma connection string y esquema, no solo que el proceso arranco)
```

### 4.5 Diagnostico de errores de arranque (HTTP 500.30)

Firewall, connection string incorrecta y falta de recursos dan el **mismo** sintoma externo (`500.30 ASP.NET Core app failed to start`). No adivinar: leer el log.

- `Program.cs` configura **Serilog antes de crear el builder**, asi que cualquier excepcion de arranque queda en `logs/carfix-<fecha>.log` en la raiz del sitio.
- Leerlo via Kudu VFS con las credenciales de `appCarFix.PublishSettings` (atributo `userPWD`):
  ```bash
  curl -u '$appCarFix:<userPWD>' "https://appcarfix-hdeyb4dncbdwbchg.scm.canadaeast-01.azurewebsites.net/api/vfs/site/wwwroot/logs/"
  ```
- Casos reales ya vistos:

| Error en el log | Causa | Solucion |
|---|---|---|
| `SqlException 40615 ... Client with IP address ... is not allowed` | Firewall de Azure SQL | Activar "Allow Azure services..." (puede tardar en propagar) |
| `Login failed for user 'appcarfixuser'` (18456) | Contrasenna mal pegada en la connection string del Portal | Re-pegar la cadena completa, tipo `SQLAzure` |
| `400 The origin ... is not allowed` (no es 500.30) | CORS de plataforma o `https` vs `http` en localhost | Ver seccion 4.3 |

---

## 5. Frontend — Azure Static Web Apps

### 5.1 Preparacion del repositorio (una sola vez)

1. Sacar secretos de `appsettings.json` a `dotnet user-secrets` y dejarlos como `""`.
2. `.gitignore` en la raiz con `appCarFix.PublishSettings`, `dev-certs/`, `secrets/`, `bin/`, `obj/`, `node_modules/`.
3. `git init` → commit → repo **privado** en GitHub → push a `master`.

### 5.2 Creacion (Portal → Create a resource → Static Web App)

| Campo | Valor |
|---|---|
| Resource Group | `appCarFixRG` |
| Name | `appCarFixWeb` |
| Plan type | Free |
| Region | East US 2 |
| Source | GitHub → cuenta / repo / rama `master` |
| Build preset | Angular |
| App location | `/Frontend/car-fix-app` |
| Api location | *(vacio — el API es el App Service)* |
| Output location | `dist/car-fix-app/browser` |

Azure commitea solo un workflow `.github/workflows/azure-static-web-apps-<nombre>.yml` y guarda el token de despliegue como secret del repo (`AZURE_STATIC_WEB_APPS_API_TOKEN_...`).

**Trampas conocidas:**
- **Angular 21 (`@angular/build:application`) genera la salida en `dist/<proyecto>/browser`** aunque no haya SSR. `output_location` debe incluir `/browser`.
- El wizard del Portal **autogenero valores incorrectos** de `app_location`/`output_location` la primera vez. Despues de crear el recurso: `git pull`, revisar el workflow, corregir y hacer push.
- Mientras el primer build no termina, la URL muestra la pagina por defecto de Azure. Confirmar con `curl` que el HTML ya tiene `<title>CarFixApp</title>`.

### 5.3 Ruteo SPA

`Frontend/car-fix-app/staticwebapp.config.json` (en la raiz de `app_location`; el action de SWA lo detecta ahi):

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "/icons/*",
      "/*.{css,js,json,png,jpg,jpeg,gif,svg,ico,webmanifest,map,ttf,woff,woff2,eot}"]
  }
}
```

Sin esto, recargar una ruta de Angular (`/ordenes`) devuelve 404.

### 5.4 URL del API — `environment.ts`

`src/environments/environment.ts` **no usa `fileReplacements`**: es un unico archivo con un bloque comentado (IP local para `/arrancar-app`) y uno activo. **Antes de cada push**, el bloque activo debe ser:

```ts
export const environment = {
  production: false,
  apiUrl: 'https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api'
};
```

Si se sube apuntando a una IP privada, el frontend en Azure falla en silencio para todos los usuarios. (Mejora para sistemas nuevos: usar `environment.development.ts` + `fileReplacements`.)

### 5.5 Despliegue

Automatico: cualquier `git push origin master` dispara el workflow (1–3 minutos). Verificar por contenido comparando el hash del chunk:

```bash
curl -s "https://icy-wave-040fef60f.7.azurestaticapps.net/" | grep -o 'main-[a-zA-Z0-9]*\.js'
# debe coincidir con dist/car-fix-app/browser/main-*.js del build local
```

---

## 6. Flujo completo de publicacion (skill `/desplegar-azure`)

1. `az account show` y `git remote -v` (si falla, detenerse).
2. `git status`: revisar que ningun archivo de configuracion tenga secretos reales.
3. Verificar `environment.ts` (bloque de Azure activo) y origenes CORS en `Program.cs`.
4. `dotnet build CarFix.slnx` y `npx ng build --configuration production`; si algo falla, detenerse.
5. Commit + `git push origin master` (despliega el frontend).
6. `dotnet publish` + zip + `az webapp deploy` (despliega el backend) + limpiar.
7. Verificar backend (`/api/salud` + llamada autenticada) y frontend (hash del chunk).
8. Si hubo cambios de esquema: aplicar el script SQL en Azure **solo con confirmacion explicita del usuario**.

---

## 7. Checklist para replicar en un sistema nuevo

- [ ] Resource Group nuevo para el sistema
- [ ] Azure SQL: servidor logico + BD Serverless, "Allow Azure services" activado, IP del administrador en el firewall
- [ ] Usuario SQL propio de la app (`db_datareader` + `db_datawriter`), no el admin del servidor
- [ ] Ejecutar los scripts de esquema y semillas (`ESTANDAR_BASE_DATOS.md`)
- [ ] App Service Windows con el runtime .NET correcto y HTTPS Only
- [ ] Connection string tipo `SQLAzure` + App Settings con `__` para cada secreto
- [ ] Serilog configurado antes del builder, escribiendo a `logs/` (diagnostico de 500.30)
- [ ] Endpoint de salud (`/api/salud`) sin autenticacion
- [ ] Repo privado en GitHub con `.gitignore` de secretos y perfiles de publicacion
- [ ] Static Web App enlazada al repo; revisar `app_location` y `output_location` (`.../browser`) del workflow generado
- [ ] `staticwebapp.config.json` con `navigationFallback`
- [ ] Origen del Static Web App en **las dos** capas de CORS
- [ ] `environment` del frontend apuntando a la URL del App Service
- [ ] Prueba end-to-end: login desde la URL publica + crear un registro + verificarlo en la BD

---

## 8. Reglas para la IA (resumen)

1. **Nunca** escribir secretos en archivos del repo ni en documentos; leerlos de Azure cuando se necesiten.
2. **Nunca** modificar la BD de Azure sin confirmacion explicita del usuario; las skills solo leen de Azure.
3. Lectura por CLI, escritura de recursos por Portal (MFA step-up). No ejecutar `az login` por iniciativa propia.
4. No desplegar codigo que no compila; limpiar siempre `publish-tmp`/`publish.zip`.
5. Ante un 500.30, leer el log de Serilog via Kudu antes de suponer la causa.
6. Ante un error de CORS, revisar primero la capa de plataforma del Portal.
7. `appcarfixdb2` (BD en uso) tiene datos reales: pruebas destructivas solo en LocalDB.

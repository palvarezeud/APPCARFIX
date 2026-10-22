# Guia para IA — GitHub, Control de Versiones y CI/CD hacia Azure

> **Proposito:** explicarle a un asistente de IA como se conecto CAR_FIX con GitHub, como se guarda el
> codigo en el repositorio y como se automatiza el despliegue a Azure con GitHub Actions, para
> replicarlo en proyectos futuros.
>
> Complementa a `GUIA_AZURE_DESPLIEGUE.md` (recursos de Azure) y `ESTANDAR_BASE_DATOS.md` (scripts SQL).
>
> **Estado real en CAR_FIX (2026-10-21):**
>
> | Componente | Como llega a Azure hoy | Automatizado |
> |---|---|---|
> | Frontend Angular → Static Web Apps | GitHub Actions al hacer push a `master` | ✓ |
> | Backend .NET → App Service | Manual: `dotnet publish` + `az webapp deploy` (skill `/desplegar-azure`) | ✗ |
> | Base de datos → Azure SQL | Manual: scripts SQL ejecutados con confirmacion del usuario | ✗ |
>
> Las secciones 6 y 7 traen los workflows **recomendados** para automatizar backend y BD en proyectos
> nuevos. Estan marcados como plantilla: **no existen todavia en CAR_FIX**.

---

## 1. Repositorio

| Dato | Valor |
|---|---|
| Repositorio | `https://github.com/palvarezeud/APPCARFIX` |
| Visibilidad | **Privado** |
| Rama principal | `master` (tambien es la rama que despliega) |
| Protocolo | HTTPS |
| Estructura | Monorepo: `Backend/` (.NET 10), `Frontend/car-fix-app/` (Angular 21), `Documentacion/`, `.claude/`, `.github/workflows/` |

Monorepo a proposito: un solo push lleva backend, frontend, scripts SQL y documentacion juntos, y cada workflow filtra por carpeta (`paths`) para desplegar solo lo que cambio.

---

## 2. Conexion de la maquina local con GitHub

### 2.1 Git

```powershell
winget install Git.Git
git config --global user.name  "<Nombre Apellido>"
git config --global user.email "<correo de la cuenta GitHub>"
```

Autenticacion de `git push`: **Git Credential Manager** (`credential.helper = manager`, viene con Git para Windows). La primera vez que se hace push abre el navegador para iniciar sesion en GitHub y guarda el token en el almacen de credenciales de Windows. No se usan llaves SSH.

### 2.2 GitHub CLI (`gh`)

```powershell
winget install GitHub.cli
gh auth login        # interactivo: lo ejecuta el USUARIO (sugerirle "! gh auth login")
gh auth status       # verificar: cuenta activa, protocolo https, token en keyring
```

`gh` sirve a la IA para: crear el repo, ver ejecuciones de Actions (`gh run list`, `gh run watch`, `gh run view --log-failed`), administrar secrets (`gh secret set`) y abrir PRs.

**Reglas para la IA:** no ejecutar `gh auth login`/`git config --global` por iniciativa propia; si `gh auth status` falla, pedirle al usuario que inicie sesion.

---

## 3. Creacion del repositorio (como se hizo en CAR_FIX)

El proyecto existia sin git. Pasos, en este orden (**el orden importa: los secretos se limpian antes del primer commit**, porque una vez en el historial quedan ahi aunque se borren despues):

1. **Sacar secretos del codigo:** mover contrasennas/llaves de `appsettings.json` a `dotnet user-secrets` y dejar `""` en el archivo.
   ```powershell
   dotnet user-secrets set "Jwt:Llave" "<valor>" --project Backend/WebApi/CarFix.WebApi
   dotnet user-secrets set "Smtp:Contrasenna" "<valor>" --project Backend/WebApi/CarFix.WebApi
   ```
2. **Crear `.gitignore` en la raiz** (ver seccion 4).
3. Inicializar y hacer el commit inicial:
   ```bash
   git init -b master
   git add .
   git status            # revisar que NO aparezcan .PublishSettings, dev-certs/, secrets/, bin/, obj/, node_modules/
   git commit -m "Commit inicial: CAR_FIX (backend .NET 10 + frontend Angular 21)"
   ```
4. Crear el repo **privado** en GitHub y subir:
   ```bash
   gh repo create APPCARFIX --private --source . --remote origin --push
   ```
5. Crear el Static Web App en el Portal enlazado al repo (ver `GUIA_AZURE_DESPLIEGUE.md` §5). Azure commitea solo el workflow → `git pull` → corregir `app_location`/`output_location` → push.

---

## 4. `.gitignore` — que nunca se sube

```gitignore
# Credenciales de despliegue — NUNCA subir esto
appCarFix.PublishSettings
*.PublishSettings
docs/
# Respaldo local de dotnet user-secrets
secrets/

# Certificados de desarrollo autofirmados
dev-certs/

# .NET
Backend/**/bin/
Backend/**/obj/
Backend/**/logs/
*.user
.vs/

# Artefactos temporales de despliegue manual
Backend/WebApi/CarFix.WebApi/publish-tmp/
Backend/WebApi/CarFix.WebApi/publish.zip

# Angular
node_modules/
dist/

# Sistema operativo / editores
.DS_Store
Thumbs.db
.idea/
```

| Nunca en el repo | Donde vive |
|---|---|
| Contrasennas, API keys, llave JWT | `dotnet user-secrets` (local) / App Settings (Azure) / GitHub Secrets (Actions) |
| Perfil de publicacion (`*.PublishSettings`) | Solo en la maquina local |
| Certificados y llaves privadas | `dev-certs/` local |
| Connection string de Azure con contrasenna | Connection Strings del App Service / GitHub Secret |

Los perfiles `Properties/PublishProfiles/*.pubxml` **si** se suben: no contienen la contrasenna (esa esta en el `.PublishSettings`).

---

## 5. Convenciones de commits y flujo de trabajo

- Mensajes **en espannol, imperativo**, describiendo el por que: `Agregar busqueda de factura al chat de voz`, `Corregir app_location del workflow de Static Web Apps`.
- Commits pequennos por feature (facilita revertir en produccion).
- Commits hechos por la IA terminan con la linea de co-autoria indicada por el entorno (`Co-Authored-By: Claude ... <noreply@anthropic.com>`).
- La IA **solo** hace commit/push cuando el usuario lo pide.
- Antes de commitear: `git status` + `git diff --stat`, confirmar que no hay secretos y que `environment.ts` apunta a Azure (ver `GUIA_AZURE_DESPLIEGUE.md` §5.4).
- Antes de push: el backend y el frontend compilan (`dotnet build`, `npx ng build --configuration production`).
- Hoy se trabaja directo sobre `master` (un solo desarrollador). Para equipos: rama por feature + PR; el workflow de SWA ya crea **entornos de preview** por cada PR.

---

## 6. GitHub Actions — Frontend (implementado en CAR_FIX)

Archivo: `.github/workflows/azure-static-web-apps-icy-wave-040fef60f.yml`. **Lo genera Azure** al crear el Static Web App enlazado a GitHub, junto con el secret del repo `AZURE_STATIC_WEB_APPS_API_TOKEN_ICY_WAVE_040FEF60F` (token de despliegue del SWA).

```yaml
name: Azure Static Web Apps CI/CD

on:
  push:
    branches: [master]
  pull_request:
    types: [opened, synchronize, reopened, closed]
    branches: [master]

jobs:
  build_and_deploy_job:
    if: github.event_name == 'push' || (github.event_name == 'pull_request' && github.event.action != 'closed')
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          submodules: true
          lfs: false
      - name: Build And Deploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_ICY_WAVE_040FEF60F }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "./Frontend/car-fix-app"     # carpeta con package.json
          api_location: ""                           # sin Azure Functions: el API es el App Service
          output_location: "dist/car-fix-app/browser" # Angular 21 anida en /browser

  close_pull_request_job:
    if: github.event_name == 'pull_request' && github.event.action == 'closed'
    runs-on: ubuntu-latest
    steps:
      - uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_ICY_WAVE_040FEF60F }}
          action: "close"
```

Como funciona: el action (Oryx) detecta Angular, ejecuta `npm install` + `npm run build` dentro de `app_location`, toma lo que queda en `output_location` y lo publica. `staticwebapp.config.json` en `app_location` define el fallback de rutas SPA.

**Lecciones:**
- El wizard del Portal dejo mal `app_location`/`output_location` la primera vez (commit `6f32295` lo corrigio). Revisar siempre el YAML generado.
- Si se recrea el SWA, el nombre del archivo y del secret cambian (llevan el sufijo del hostname).
- Monitorear: `gh run list --workflow "Azure Static Web Apps CI/CD"` / `gh run watch` / `gh run view <id> --log-failed`.
- Mejora sugerida: agregar `paths: ['Frontend/**']` al trigger `push` para no reconstruir el frontend cuando solo cambia el backend.

---

## 7. GitHub Actions — Backend (PLANTILLA recomendada, no implementada en CAR_FIX)

En CAR_FIX el backend se despliega a mano (skill `/desplegar-azure`). Para proyectos nuevos, automatizarlo:

### 7.1 Autenticacion de GitHub con Azure — OIDC (sin contrasennas guardadas)

Se recomienda **federated credentials (OIDC)** en vez del publish profile: GitHub obtiene un token temporal de Azure en cada ejecucion; no hay secreto que rotar.

1. Portal → **Microsoft Entra ID → App registrations → New registration** (ej. `github-<proyecto>-deploy`). Anotar *Application (client) ID* y *Directory (tenant) ID*.
2. En la app registration → **Certificates & secrets → Federated credentials → Add** → escenario *GitHub Actions deploying Azure resources*: organizacion/usuario, repo, entidad `Branch` = `master`.
3. Resource Group del proyecto → **Access control (IAM) → Add role assignment** → rol `Website Contributor` (o `Contributor` en el RG) a esa app registration.
4. Secrets del repo (no son secretos sensibles, pero se guardan ahi por convencion):
   ```bash
   gh secret set AZURE_CLIENT_ID       --body "<client-id>"
   gh secret set AZURE_TENANT_ID       --body "<tenant-id>"
   gh secret set AZURE_SUBSCRIPTION_ID --body "<subscription-id>"
   ```

> En el tenant de CAR_FIX las escrituras por CLI exigen MFA step-up: crear la app registration, la credencial federada y el rol **desde el Portal**. Si el tenant no permite registrar aplicaciones, la alternativa es el publish profile: Portal → App Service → *Download publish profile* → `gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < perfil.PublishSettings` y usar `publish-profile:` en el paso de deploy (y nunca commitear ese archivo).

### 7.2 Workflow `.github/workflows/backend-appservice.yml`

```yaml
name: Backend - App Service

on:
  push:
    branches: [master]
    paths: ['Backend/**', '.github/workflows/backend-appservice.yml']
  workflow_dispatch:

permissions:
  id-token: write   # necesario para OIDC
  contents: read

env:
  PROYECTO_API: Backend/WebApi/CarFix.WebApi/CarFix.WebApi.csproj
  APP_SERVICE: appCarFix

jobs:
  build-deploy:
    runs-on: windows-latest          # mismo SO que el App Service
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Compilar y probar
        run: |
          dotnet build Backend/CarFix.slnx -c Release
          dotnet test  Backend/CarFix.slnx -c Release --no-build

      - name: Publicar
        run: dotnet publish ${{ env.PROYECTO_API }} -c Release -o ${{ runner.temp }}/publish

      - uses: azure/login@v2
        with:
          client-id:       ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id:       ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ env.APP_SERVICE }}
          package:  ${{ runner.temp }}/publish

      - name: Verificar salud
        shell: bash
        run: |
          sleep 20
          curl -sf "https://<hostname-app-service>/api/salud"
```

Los secretos de la aplicacion (connection string, JWT, SMTP, API keys) **siguen en las App Settings del App Service**; el workflow no los necesita.

---

## 8. GitHub Actions — Base de Datos (PLANTILLA recomendada, no implementada en CAR_FIX)

En CAR_FIX los scripts de `Backend/Scripts/sql/` se aplican a mano en Azure con confirmacion del usuario. Para automatizarlo de forma segura:

### 8.1 Principios

- Los scripts deben ser **idempotentes** (`IF NOT EXISTS`, `IF COL_LENGTH(...) IS NULL`) — ver `ESTANDAR_BASE_DATOS.md` §9. Asi re-ejecutarlos no rompe nada.
- Se ejecutan en orden por nombre (`AAAA-MM-DD_descripcion.sql`).
- Registrar lo aplicado en una tabla de control para no re-ejecutar:
  ```sql
  CREATE TABLE Sistema.ScriptsAplicados (
      NombreScript   VARCHAR(250) NOT NULL CONSTRAINT PK_ScriptsAplicados PRIMARY KEY,
      FechaAplicado  DATETIME     NOT NULL CONSTRAINT DF_ScriptsAplicados_FechaAplicado DEFAULT (GETUTCDATE())
  );
  ```
- **Aprobacion humana obligatoria:** el job usa un *environment* de GitHub (`produccion-bd`) con *required reviewers* (Settings → Environments). El workflow se pausa hasta que el usuario aprueba. Esto conserva la regla de CAR_FIX de "nunca modificar la BD de Azure sin confirmacion".
- Firewall de Azure SQL: los runners de GitHub tienen IPs variables. La opcion simple es que el usuario de BD use una conexion desde un runner con "Allow Azure services" + regla temporal (ver paso del workflow), o un runner self-hosted dentro de Azure.

### 8.2 Workflow `.github/workflows/base-datos.yml`

```yaml
name: Base de datos - Azure SQL

on:
  push:
    branches: [master]
    paths: ['Backend/Scripts/sql/**']
  workflow_dispatch:

permissions:
  id-token: write
  contents: read

jobs:
  aplicar-scripts:
    runs-on: windows-latest
    environment: produccion-bd        # requiere aprobacion manual
    steps:
      - uses: actions/checkout@v4

      - uses: azure/login@v2
        with:
          client-id:       ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id:       ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Abrir firewall para la IP del runner
        id: firewall
        shell: pwsh
        run: |
          $ip = (Invoke-RestMethod https://api.ipify.org)
          az sql server firewall-rule create -g <resource-group> -s <servidor-sql> -n gh-runner-${{ github.run_id }} --start-ip-address $ip --end-ip-address $ip

      - name: Aplicar scripts pendientes
        shell: pwsh
        env:
          CADENA_SQL: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}
        run: |
          Install-Module SqlServer -Force -Scope CurrentUser
          Invoke-Sqlcmd -ConnectionString $env:CADENA_SQL -Query "IF OBJECT_ID('Sistema.ScriptsAplicados') IS NULL CREATE TABLE Sistema.ScriptsAplicados (NombreScript VARCHAR(250) NOT NULL PRIMARY KEY, FechaAplicado DATETIME NOT NULL DEFAULT GETUTCDATE());"
          $aplicados = (Invoke-Sqlcmd -ConnectionString $env:CADENA_SQL -Query "SELECT NombreScript FROM Sistema.ScriptsAplicados").NombreScript
          Get-ChildItem Backend/Scripts/sql/*.sql | Sort-Object Name | Where-Object { $aplicados -notcontains $_.Name } | ForEach-Object {
            Write-Host "Aplicando $($_.Name)"
            Invoke-Sqlcmd -ConnectionString $env:CADENA_SQL -InputFile $_.FullName -ErrorAction Stop
            Invoke-Sqlcmd -ConnectionString $env:CADENA_SQL -Query "INSERT INTO Sistema.ScriptsAplicados (NombreScript) VALUES ('$($_.Name)')"
          }

      - name: Cerrar firewall
        if: always()
        run: az sql server firewall-rule delete -g <resource-group> -s <servidor-sql> -n gh-runner-${{ github.run_id }}
```

- `AZURE_SQL_CONNECTION_STRING`: `gh secret set AZURE_SQL_CONNECTION_STRING --env produccion-bd` (pegado por el usuario, nunca escrito por la IA en un archivo). Usar el modulo `SqlServer` (no el viejo `SQLPS`, que no acepta las palabras clave modernas de la cadena de Azure).
- El usuario SQL del workflow necesita permisos DDL (`db_ddladmin`); usar uno distinto al de la aplicacion.
- Orden de despliegue cuando un cambio toca BD y codigo: **BD primero** (script idempotente y compatible hacia atras), luego backend.

---

## 9. Secrets del repositorio (resumen)

| Secret | Usado por | Origen |
|---|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN_<sufijo>` | Workflow frontend | Lo crea Azure al enlazar el SWA (existe en CAR_FIX) |
| `GITHUB_TOKEN` | Comentarios en PRs | Automatico de GitHub |
| `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` | Backend y BD (plantilla) | App registration con OIDC |
| `AZURE_SQL_CONNECTION_STRING` (environment `produccion-bd`) | BD (plantilla) | Usuario SQL con permisos DDL |

Administrar con `gh secret list` / `gh secret set <NOMBRE>` (el valor lo pega el usuario; la IA no debe imprimir ni guardar secretos).

---

## 10. Checklist para un proyecto nuevo

- [ ] Git + Git Credential Manager + `gh auth login` hechos por el usuario
- [ ] Secretos movidos a `user-secrets` **antes** del primer commit
- [ ] `.gitignore` con `*.PublishSettings`, certificados, `secrets/`, `bin/`, `obj/`, `node_modules/`, `dist/`
- [ ] Repo **privado** creado con `gh repo create --private --source . --push`
- [ ] Static Web App enlazado → revisar y corregir el workflow generado (`app_location`, `output_location` con `/browser`)
- [ ] App registration + credencial federada OIDC + rol en el Resource Group
- [ ] Workflow de backend con filtro `paths: Backend/**` y verificacion de salud
- [ ] Environment `produccion-bd` con aprobacion manual + workflow de scripts SQL idempotentes
- [ ] Filtros `paths` en cada workflow para desplegar solo lo que cambio
- [ ] Primer push → `gh run watch` → verificar las tres piezas end-to-end

## 11. Reglas para la IA (resumen)

1. Commit y push solo cuando el usuario lo pide; mensajes en espannol e imperativo.
2. Revisar `git status`/diff antes de commitear: ningun secreto, `environment.ts` apuntando a Azure.
3. No ejecutar logins interactivos (`gh auth login`, `az login`); pedirselos al usuario.
4. Nunca escribir valores de secretos en archivos, logs ni en la conversacion.
5. Cambios a la BD de produccion solo con aprobacion explicita (manual o via environment protegido).
6. Si un workflow falla: `gh run view <id> --log-failed` antes de suponer la causa.

# Despliegue y prueba del backend CAR_FIX en Azure App Service

Runbook para publicar el backend (`CarFix.WebApi`) en el App Service ya existente y probarlo desde
un Angular corriendo en `localhost`. No documenta la creación de infraestructura desde cero — esa
infraestructura ya existe (ver abajo).

## Infraestructura existente

| Recurso | Valor |
|---|---|
| Subscription | `238f43d1-4e1d-4bbd-9760-e895f64ce26a` |
| Resource Group | `appCarFixRG` |
| App Service | `appCarFix` |
| URL pública | `https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net` |
| Azure SQL Server | `appcarfixinstance.database.windows.net` |
| Azure SQL Database | `appcarfixdb` |
| SQL Auth — usuario | `appcarfixuser` |

Perfiles de publicación disponibles en `Backend/WebApi/CarFix.WebApi/Properties/PublishProfiles/`:
`appCarFix - Web Deploy.pubxml`, `appCarFix - Zip Deploy.pubxml`, `appCarFix - FTP.pubxml`,
`appCarFix - ReadOnly - FTP.pubxml`, `FolderProfile.pubxml`. El método recomendado es **Visual
Studio → botón Publish** con el perfil `appCarFix - Web Deploy` (o `Zip Deploy`, ambos apuntan al
mismo sitio).

El proyecto backend es `net10.0` y usa `UserSecretsId` (`281ef206-991b-4528-88c3-74531ac9deb5`)
para secretos en desarrollo, igual que documenta el `CLAUDE.md` del repo.

## Consideraciones antes de publicar

### 1. Certificado — no requiere trabajo
- `*.azurewebsites.net` ya sirve HTTPS con certificado gestionado por Microsoft, válido para
  cualquier navegador. No hace falta el `.pfx` autofirmado de `dev-certs/` (ese es solo para
  cuando el backend corre en esta máquina, vía la skill `arrancar-app`).
- `localhost` es "contexto seguro" para Chrome aunque se sirva por HTTP plano, así que para probar
  Angular local contra el backend de Azure alcanza con `ng serve` **sin** `--ssl`.
- `Program.cs` ya llama `app.UseHttpsRedirection()`. En este App Service (Windows, IIS + ANCM) esto
  funciona sin configuración adicional. Verificar en la prueba final que no haya bucles de
  redirección.

### 2. CORS
- `Program.cs` ya incluye `http://localhost:4200` en la política `WithOrigins(...)`, junto con los
  orígenes por IP que gestiona la skill `arrancar-app` para pruebas desde celular. No hace falta
  agregar nada para probar desde Angular local.
- El App Service puede estar corriendo una versión más vieja del código (con otra lista de
  orígenes) — **hay que republicar** para que la política CORS vigente en Azure coincida con la de
  `Program.cs`.
- Mejora pendiente (no bloqueante): la lista de orígenes está hardcodeada en C#. Cada vez que
  cambia la IP Wi-Fi (la skill `arrancar-app` la reescribe) habría que recompilar y republicar en
  Azure para que un celular la use contra la nube. Si ese flujo se vuelve habitual, mover
  `Cors:OrigenesPermitidos` a configuración en vez de código.

### 3. Ruta / URL del API
- `Frontend/car-fix-app/src/environments/environment.ts` no usa `fileReplacements` de Angular (no
  hay `environment.development.ts` / `environment.production.ts` en `angular.json`); se edita a
  mano alternando bloques comentados. Para probar contra Azure, `apiUrl` debe ser
  `https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api`.
- Los endpoints del backend cuelgan de `/api/...` en la raíz del sitio (`MapearAutenticacion`,
  `MapearClientes`, etc.); el publish profile no define un sub-path adicional (`DeployIisAppPath`
  es solo el nombre del sitio en IIS). No hay ajuste de ruta pendiente del lado del backend.

### 4. Configuración / secrets en el App Service
- Verificar en el Portal (`appCarFixRG` → `appCarFix` → Configuration) antes de dar la prueba por
  buena:
  - **Connection strings** (pestaña dedicada, no Application settings) → `CarFix`, tipo `SQLAzure`,
    con la cadena de conexión de más abajo. ASP.NET Core la mapea automáticamente a
    `ConnectionStrings:CarFix`.
  - `Jwt__Llave`, `Smtp__Host`, `Smtp__Puerto`, `Smtp__UsuarioRemitente`, `Smtp__NombreRemitente`,
    `Smtp__Contrasenna`, `Smtp__UsarSsl`, `Anthropic__ApiKey`, `Anthropic__Modelo` en Application
    settings (nombres con `__` en vez de `:`).
  - El firewall de la Azure SQL Database permite conexiones desde el App Service ("Allow Azure
    services and resources to access this server", o la IP saliente del App Service).
  - `ASPNETCORE_ENVIRONMENT` está en `Production` (o el valor que corresponda).
- **Nunca cargar secretos reales en `appsettings.json`** — ese archivo se sube tal cual en cada
  publish. Hoy contiene en texto plano una contraseña SMTP real y una llave JWT de desarrollo;
  mientras las Application Settings del App Service sobreescriban esos valores no hay impacto
  funcional, pero es una mejora pendiente sacarlos del archivo.

### Connection string a Azure SQL

Formato ADO.NET estándar de Azure SQL (confirmado correcto):

```
Server=tcp:appcarfixinstance.database.windows.net,1433;Initial Catalog=appcarfixdb;Persist Security Info=False;User ID=appcarfixuser;Password=***;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

- `Encrypt=True` + `TrustServerCertificate=False` es lo correcto para Azure SQL (a diferencia de la
  cadena local con `SQL2022`, que usa `TrustServerCertificate=True` por el certificado autofirmado).
- Para probar el backend **localmente** contra esta misma Azure SQL Database antes de publicar:
  ```
  dotnet user-secrets set "ConnectionStrings:CarFix" "<cadena completa>" --project Backend/WebApi/CarFix.WebApi
  ```
- La contraseña actual del usuario `appcarfixuser` (`Linux01.`) es débil/predecible — conviene
  rotarla en algún momento; no bloquea el uso actual.

## Pasos para publicar y probar

1. En `Frontend/car-fix-app/src/environments/environment.ts`, dejar activo el bloque con
   `apiUrl: 'https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api'`.
2. Verificar en el Portal de Azure la configuración del punto 4 (connection string, secrets,
   firewall, `ASPNETCORE_ENVIRONMENT`).
3. Publicar el backend desde Visual Studio con el perfil `appCarFix - Web Deploy` (configuración
   Release), para que la política CORS vigente en el código quede activa en Azure.
4. Levantar Angular local: `cd Frontend/car-fix-app && npx ng serve` (puerto 4200, sin `--ssl`).
5. Abrir `http://localhost:4200`, iniciar sesión con credenciales que ya existan en la Azure SQL
   Database (no necesariamente `admin`/`123456` — ese es solo el seed de una base nueva) y
   verificar que las pantallas cargan datos reales de Azure sin errores de CORS ni de certificado en
   la consola del navegador.

## Troubleshooting — caso real (2026-07-12)

La primera vez que se probó este flujo, el sitio devolvía **HTTP 500.30 "ASP.NET Core app failed
to start"** tanto en `/` como en cualquier endpoint de `/api/...`. Se descartó certificado, CORS,
ruta y runtime (`.NET 10 LTS` confirmado) antes de encontrar la causa real. Pasos que sirvieron:

1. **Diagnóstico sin abrir el portal:** Serilog (`Program.cs`) ya escribe cualquier excepción de
   arranque a `logs/carfix-<fecha>.log` en la raíz del sitio, independientemente de si el
   "Application Logging" de Azure está activado. Se puede leer directo por la API de Kudu (VFS) con
   las credenciales del perfil de publicación (`<raiz-repo>/appCarFix.PublishSettings`, atributo
   `userPWD` del perfil `Web Deploy`/`Zip Deploy` — usuario `$appCarFix`):
   ```
   curl -u '$appCarFix:<userPWD del .PublishSettings>' \
     "https://appcarfix-hdeyb4dncbdwbchg.scm.canadaeast-01.azurewebsites.net/api/vfs/site/wwwroot/logs/"
   ```
   Y bajar el archivo más reciente (`.../logs/carfix-YYYYMMDD.log`) para ver la línea `[FTL]` con el
   stack trace completo.
2. **Primer error real, en el log:** `SqlException: Cannot open server 'appcarfixinstance'... Client
   with IP address 'x.x.x.x' is not allowed to access the server` (Error 40615) — esto sí era
   firewall, pero se resolvió solo (la regla `AllowAllWindowsAzureIps` de "Allow Azure services" ya
   estaba activa, solo faltaba que propagara).
3. **Segundo error, después de resuelto el firewall:** `Login failed for user 'appcarfixuser'`
   (Error 18456) — el firewall ya no bloqueaba (la conexión llegó hasta el login TDS), pero la
   contraseña cargada en el Portal (**Configuration → Connection strings → `CarFix`**) no coincidía
   con la real. Se resolvió re-pegando la cadena completa y correcta, tipo `SQLAzure`, sin marcar
   "Configuración de ranura de implementación" (no aplica — no hay deployment slots en este App
   Service). Guardar la connection string reinicia el App Service automáticamente.
4. **Gotcha de CORS con `localhost`:** el `ng serve` había quedado corriendo con `--ssl` (heredado
   de una sesión anterior probando contra el backend local), sirviendo en `https://localhost:4200`.
   La política CORS del backend solo permite `http://localhost:4200` (sin TLS) — la `s` de más
   causaba `400 The origin 'https://localhost:4200' is not allowed`. Se resolvió reiniciando
   `ng serve` **sin** `--ssl` (confirma el punto 1 de "Certificado": no hace falta HTTPS local para
   este escenario, `localhost` plano ya es contexto seguro).
5. **Resultado confirmado:** login exitoso desde `http://localhost:4200`, alta de un cliente desde
   la UI, y verificado directamente en SSMS/Azure SQL que el registro quedó en `appcarfixdb`.

**Lección para la próxima vez:** si vuelve a aparecer un 500.30, ir directo al log vía Kudu VFS (paso
1) en vez de asumir la causa — firewall, connection string y falta de recursos dan el mismo síntoma
externo (500.30) pero excepciones distintas en el log.

## Riesgos

- **Datos reales:** `appcarfixdb` no es un sandbox descartable — cualquier alta/edición durante la
  prueba queda ahí. Si se necesita un entorno aislado, coordinar antes de probar flujos de
  escritura (crear cliente, orden, etc.).
- **Redeploy pisa cambios manuales:** si alguien tocó código o configuración directo en Azure (Kudu,
  portal) sin reflejarlo en el repo, publicar desde Visual Studio lo sobreescribe.
- **Secretos en `appsettings.json`:** ver punto 4 arriba — no bloquea la prueba, pero es deuda
  técnica a resolver.

## Verificación de que la prueba salió bien

- Login exitoso desde `http://localhost:4200` contra el backend de Azure.
- Sin errores CORS ni de certificado en la consola del navegador (DevTools → Network/Console).
- Las listas (Clientes, Vehículos, Órdenes, Facturas) muestran datos que vienen de `appcarfixdb`,
  no de la base local.

**Confirmado funcionando end-to-end el 2026-07-12:** login desde `http://localhost:4200`, alta de
un cliente desde la UI, y verificado en SSMS que el registro llegó a `appcarfixdb` en Azure SQL.

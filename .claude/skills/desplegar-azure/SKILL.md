# desplegar-azure

Sube los cambios pendientes a GitHub y despliega backend + frontend en Azure (App Service + Static Web Apps), verificando al final que ambos respondan con los cambios nuevos. Pensada para ejecutarse cuando ya se probó localmente (`/arrancar-app`) y se quiere llevar todo a producción de una sola vez.

## Recursos de Azure (fijos para este proyecto)

| Recurso | Nombre | Grupo de recursos | Hostname |
|---|---|---|---|
| Backend (App Service) | `appCarFix` | `appCarFixRG` | `appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net` |
| Frontend (Static Web App) | `appCarFixWeb` | `appCarFixRG` | `icy-wave-040fef60f.7.azurestaticapps.net` |

**Solo el frontend tiene CI/CD automático** (`.github/workflows/azure-static-web-apps-icy-wave-040fef60f.yml`, dispara con cualquier push a `master`). **El backend no tiene workflow de GitHub Actions** — esta skill lo publica y despliega manualmente con `dotnet publish` + `az webapp deploy`.

La cadena de conexión a BD (`ConnectionStrings:CarFix`) y los secretos (`Anthropic__ApiKey`, `Jwt__Llave`, `Smtp__Host`, `Smtp__UsuarioRemitente`, `Smtp__contrasenna`) ya están configurados como App Settings/Connection Strings en el App Service — esta skill nunca los toca. Si un despliegue falla por credenciales, el problema esta ahi, no en el codigo.

## Prerrequisitos (verificar antes de empezar, no asumir)

```powershell
az account show    # debe mostrar una suscripcion activa; si falla, avisar al usuario y detenerse (no ejecutar `az login` sin pedir permiso, es una accion interactiva)
git remote -v       # confirmar que "origin" apunta al repo de GitHub esperado
```

Si `az account show` falla, **detenerse y avisar al usuario** — no continuar con el resto de la skill sin CLI autenticado.

## Pasos

### 1. Revisar el estado de git antes de tocar nada

```bash
git status
git diff --stat
```

Leer la lista de archivos modificados/nuevos. Prestar atencion especial a:
- Archivos de configuracion (`appsettings.json`, `environment.ts`, `Program.cs`) — ver paso 2.
- Cualquier archivo que no se reconozca de la sesion de trabajo actual (podria ser WIP de una sesion anterior sin commitear — no descartarlo, pero confirmar con el usuario si no es obvio a que corresponde antes de incluirlo en un commit).
- Que ningun `appsettings.json`/similar tenga un secreto real en texto plano (`Contrasenna`, `ApiKey`, etc. deben seguir vacios — los secretos reales viven en Azure App Settings, nunca en el repo).

### 2. Trampa conocida — `environment.ts` no usa `fileReplacements`

**Este es el error mas facil de cometer.** A diferencia de un proyecto Angular estandar, `Frontend/car-fix-app/src/environments/environment.ts` **no** tiene un `environment.prod.ts` separado ni configuracion de `fileReplacements` en `angular.json`. Es un unico archivo donde se comenta/descomenta manualmente el bloque activo. Si se dejo apuntando a una IP local (de haber corrido `/arrancar-app` antes), el frontend desplegado en Azure quedaria intentando llamar a esa IP privada y todo fallaria en silencio para cualquier usuario real.

**Verificar y corregir antes de commitear:**

```powershell
Get-Content "Frontend\car-fix-app\src\environments\environment.ts"
```

El bloque **activo** (sin `/* ... */`) debe ser el que tiene `apiUrl: 'https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api'`. Si el activo es el de una IP local (`https://192.168.x.x:5152/api`), reescribir el archivo invirtiendo cual bloque esta comentado:

```
/*export const environment = {
  production: false,
  apiUrl: 'https://192.168.x.x:5152/api'
};*/

export const environment = {
  production: false,
  apiUrl: 'https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api'
};
```

Tambien revisar `Backend/WebApi/CarFix.WebApi/Program.cs` (politica CORS, `WithOrigins(...)`) — debe seguir incluyendo `"https://icy-wave-040fef60f.7.azurestaticapps.net"` en la lista. No hace falta quitar los origenes de IP local que haya agregado `/arrancar-app`; son inofensivos en produccion (CORS es una lista blanca, no un valor unico), asi que no requieren limpieza.

### 3. Build de verificacion (backend y frontend)

No commitear ni desplegar sin haber compilado primero.

```bash
cd Backend && dotnet build CarFix.slnx
```

```bash
cd Frontend/car-fix-app && npx ng build --configuration production
```

Si cualquiera falla, **detenerse** y arreglar el error antes de continuar — nunca desplegar codigo que no compila.

### 4. Commit y push (dispara el despliegue automatico del frontend)

Seguir las convenciones normales de commit del proyecto (mensajes en espannol, imperativo, `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`). Si los cambios pendientes cubren varias features no relacionadas, preferir varios commits chicos en vez de uno gigante — facilita revertir si algo sale mal en produccion.

```bash
git add <archivos correspondientes>
git commit -m "..."
git push origin master
```

El push dispara el workflow de Static Web Apps automaticamente — no hace falta ni se puede forzar manualmente desde aqui.

### 5. Publicar y desplegar el backend manualmente

```bash
cd Backend/WebApi/CarFix.WebApi
rm -rf publish-tmp publish.zip
dotnet publish -c Release -o ./publish-tmp
```

Comprimir (usar PowerShell `Compress-Archive` incluso desde Git Bash, es mas confiable que `zip` en Windows):

```powershell
Compress-Archive -Path "Backend\WebApi\CarFix.WebApi\publish-tmp\*" -DestinationPath "Backend\WebApi\CarFix.WebApi\publish.zip" -Force
```

Desplegar:

```bash
az webapp deploy --resource-group appCarFixRG --name appCarFix --src-path Backend/WebApi/CarFix.WebApi/publish.zip --type zip
```

Esperar el mensaje `"Deployment has completed successfully"` y `"provisioningState": "Succeeded"` en la respuesta JSON.

**Limpiar siempre al terminar** (exista o no error), para no dejar basura de 40+ MB en el repo local ni arriesgar que `publish-tmp`/`publish.zip` se cuelen en un commit futuro:

```bash
rm -rf Backend/WebApi/CarFix.WebApi/publish-tmp Backend/WebApi/CarFix.WebApi/publish.zip
```

### 6. Verificar ambos despliegues

**Backend** — health check simple y una llamada autenticada que toque BD real (confirma que la cadena de conexion y el esquema siguen funcionando, no solo que el proceso arranco):

```bash
curl -s -o /dev/null -w "HTTP:%{http_code}\n" "https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api/salud"

TOKEN=$(curl -s -X POST "https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api/autenticacion/iniciar-sesion" \
  -H "Content-Type: application/json" -d '{"nombreUsuario":"admin","password":"123456"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

curl -s "https://appcarfix-hdeyb4dncbdwbchg.canadaeast-01.azurewebsites.net/api/parametros" -H "Authorization: Bearer $TOKEN"
```

**Frontend** — el push dispara el workflow de forma asincrona; no hay `gh` CLI autenticado en este entorno para monitorear el run directamente (verificar con `gh auth status` si esto cambio). En su lugar, confirmar por contenido: el nombre de los archivos JS con hash de contenido va a coincidir exactamente entre el build local (paso 3) y lo servido en produccion una vez el despliegue termine (usualmente 1-3 minutos despues del push):

```bash
# Tomar el nombre del chunk relevante del build local en dist/car-fix-app, luego:
curl -s "https://icy-wave-040fef60f.7.azurestaticapps.net/" | grep -o 'main-[a-zA-Z0-9]*\.js'
curl -s "https://icy-wave-040fef60f.7.azurestaticapps.net/<mismo-nombre-de-chunk>" | grep -o "<algun texto nuevo que se agrego en este cambio>"
```

Si el hash del chunk principal en produccion coincide con el del build local, o el chunk especifico donde se hizo el cambio contiene el texto/logica esperada, el despliegue ya se completo. Si no coincide, esperar ~1 minuto y reintentar (no hay necesidad de sondear en loop corto; un solo reintento tras una espera razonable es suficiente).

### 7. Reportar resultado

Informar al usuario:
- Que se commiteo y pusheo (resumen corto de cada commit).
- Estado del backend (`Succeeded`/error, resultado del healthcheck).
- Estado del frontend (confirmado por hash de contenido o pendiente de propagar).
- Recordar que la verificacion visual real (abrir la app en el navegador y probar el flujo) le queda al usuario, salvo que se disponga de herramienta de navegador en la sesion.

## Notas

- Esta skill asume que el usuario ya aplico manualmente cualquier cambio de esquema/datos necesario en la base de datos de Azure (esta skill nunca migra ni modifica la BD de Azure — eso es responsabilidad explicita del usuario o de un paso separado y confirmado).
- No usar `az login` ni cambiar la suscripcion activa (`az account set`) dentro de esta skill sin que el usuario lo pida explicitamente — si `az account show` apunta a la suscripcion equivocada, avisar y detenerse en vez de cambiarla por cuenta propia.
- Si en el futuro se agrega un workflow de GitHub Actions para el backend, actualizar el paso 5 para que sea solo verificacion (esperar el workflow) en vez de despliegue manual.

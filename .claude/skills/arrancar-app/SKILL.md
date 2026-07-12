# arrancar-app

Arranca el backend ASP.NET Core y el frontend Angular de CAR_FIX usando la IP actual de la máquina (Wi-Fi), **en HTTPS**. Detiene cualquier instancia previa antes de levantar los servicios.

## Por que HTTPS y no HTTP

Chrome en Android (y Chrome en general) solo expone las APIs que requieren "contexto seguro" cuando la pagina se sirve por HTTPS (o `localhost`). Esto incluye:

- **Reconocimiento de voz** (`SpeechRecognition`/`webkitSpeechRecognition`, usado por el FAB de voz y el chat de voz movil).
- **Camara** (`getUserMedia`, usado por el escaneo IA de tarjeta de circulacion y factura de repuestos).

Sobre `http://<ip>:...` estas APIs quedan ocultas por completo en el navegador (no aparecen ni fallan, simplemente no existen) — el boton de microfono no reacciona y el input de camara no abre. Por eso el frontend y el backend se sirven siempre por HTTPS con un certificado autofirmado de desarrollo, no solo para probar voz puntualmente.

**Samsung Internet no sirve para probar voz** aunque sea Chromium-based: el objeto de reconocimiento existe pero nunca produce resultados ni errores (falla en silencio). Para probar en celular hay que usar **Google Chrome real**.

## Pasos

### 1. Obtener la IP de la máquina

```powershell
$ip = (Get-NetIPAddress -AddressFamily IPv4 |
       Where-Object { $_.InterfaceAlias -like "*Wi-Fi*" -and $_.PrefixOrigin -eq "Dhcp" } |
       Select-Object -First 1).IPAddress

if (-not $ip) {
    # Fallback: primera IP no loopback que no sea APIPA
    $ip = (Get-NetIPAddress -AddressFamily IPv4 |
           Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.254.*" } |
           Select-Object -First 1).IPAddress
}
```

Si no se puede obtener la IP, usar `localhost`.

### 2. Verificar (y regenerar si hace falta) el certificado autofirmado de desarrollo

Los certificados viven en `D:\CLAUDE-REPO\AppCarFix\dev-certs\` (`carfix-dev-cert.pem`, `carfix-dev-key.pem`, `carfix-dev.pfx`, password `carfix123dev`). El certificado incluye la IP de la máquina como SAN (Subject Alternative Name); si la IP cambió desde que se generó, hay que regenerarlo:

```powershell
$certPath = "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-cert.pem"
$sanOk = $false
if (Test-Path $certPath) {
    $texto = & openssl x509 -in $certPath -noout -text
    if ($texto -match [regex]::Escape($ip)) { $sanOk = $true }
}

if (-not $sanOk) {
    New-Item -ItemType Directory -Force -Path "D:\CLAUDE-REPO\AppCarFix\dev-certs" | Out-Null
    & openssl req -x509 -newkey rsa:2048 -sha256 -days 1095 -nodes `
        -keyout "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-key.pem" `
        -out    "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-cert.pem" `
        -subj "/CN=CarFix Dev" `
        -addext "subjectAltName=IP:$ip,DNS:localhost,IP:127.0.0.1"

    & openssl pkcs12 -export `
        -out "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev.pfx" `
        -inkey "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-key.pem" `
        -in    "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-cert.pem" `
        -passout pass:carfix123dev
}
```

Si se regenera el certificado, el celular va a tener que volver a aceptar la advertencia de certificado no confiable (paso 7) porque el certificado anterior quedó invalidado.

### 3. Actualizar la IP en el frontend (`environment.ts`) y en el backend (CORS)

Hay **dos** lugares con la IP hardcodeada; si alguno queda desactualizado la app falla (el login falla en el navegador con error de CORS, aunque `curl`/`Invoke-WebRequest` funcionen porque no disparan preflight):

- `Frontend/car-fix-app/src/environments/environment.ts` → `apiUrl` (siempre `https://<ip>:5152/api`, puerto **5152**, no 5151)
- `Backend/WebApi/CarFix.WebApi/Program.cs` → lista de origenes en `AddCors` / `WithOrigins(...)` (debe incluir el origen `https://<ip>:4200`)

Sincronizar ambos con la IP detectada en el paso 1:

```powershell
# Frontend — apiUrl (HTTPS, puerto 5152)
$envPath = "D:\CLAUDE-REPO\AppCarFix\Frontend\car-fix-app\src\environments\environment.ts"
$contenido = Get-Content $envPath -Raw
$nuevoContenido = $contenido -replace "apiUrl:\s*'https?://[^']+'", "apiUrl: 'https://${ip}:5152/api'"
if ($nuevoContenido -ne $contenido) {
    [System.IO.File]::WriteAllText($envPath, $nuevoContenido, (New-Object System.Text.UTF8Encoding $false))
}

# Backend — origenes CORS permitidos para el frontend (http local + https por IP)
$programPath = "D:\CLAUDE-REPO\AppCarFix\Backend\WebApi\CarFix.WebApi\Program.cs"
$contenidoProgram = Get-Content $programPath -Raw
$nuevoContenidoProgram = $contenidoProgram -replace `
    'WithOrigins\("http://localhost:4200",\s*"http[s]?://[^"]+",?\s*"?h?t?t?p?s?:?/?/?[^"]*"?\)', `
    "WithOrigins(`"http://localhost:4200`", `"http://${ip}:4200`", `"https://${ip}:4200`")"
if ($nuevoContenidoProgram -ne $contenidoProgram) {
    [System.IO.File]::WriteAllText($programPath, $nuevoContenidoProgram, (New-Object System.Text.UTF8Encoding $false))
}
```

**Verificar el resultado del reemplazo de CORS con un `grep`/`Select-String` después de escribir** — la regex de `WithOrigins` es la parte más frágil del script (el número de orígenes previos puede variar). Si no calzó bien, corregir `Program.cs` a mano dejando exactamente:

```csharp
.WithOrigins("http://localhost:4200", "http://<ip>:4200", "https://<ip>:4200")
```

`ng serve` detecta el cambio en `environment.ts` y recompila solo. **El backend NO tiene hot-reload** (se arranca con `dotnet run`, no `dotnet watch`) — el cambio en `Program.cs` solo toma efecto si el backend se reinicia después (paso 5 ya lo hace en cada ejecución de la skill).

### 4. Detener instancias previas

```powershell
# Matar dotnet que tenga el puerto 5151 (http) o 5152 (https) — mismo proceso Kestrel
foreach ($puerto in 5151, 5152) {
    $pid = (Get-NetTCPConnection -LocalPort $puerto -ErrorAction SilentlyContinue).OwningProcess
    if ($pid) { Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue }
}

# Matar node que tenga el puerto 4200
$pid4200 = (Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue).OwningProcess
if ($pid4200) { Stop-Process -Id $pid4200 -Force -ErrorAction SilentlyContinue }

Start-Sleep -Seconds 2
```

### 5. Arrancar el backend en background

El perfil `http` de `launchSettings.json` ya expone **ambos** puertos (`http://0.0.0.0:5151` y `https://0.0.0.0:5152` con el `.pfx` de `dev-certs`) — no hace falta cambiar el comando:

Usar la herramienta Bash con `run_in_background: true`:

```
dotnet run --project "D:\CLAUDE-REPO\AppCarFix\Backend\WebApi\CarFix.WebApi" --launch-profile http
```

Esperar hasta que el puerto 5152 (https) esté escuchando (máximo 30 segundos):

```powershell
$ok = $false
for ($i = 0; $i -lt 15; $i++) {
    Start-Sleep -Seconds 2
    if (Get-NetTCPConnection -LocalPort 5152 -ErrorAction SilentlyContinue) { $ok = $true; break }
}
```

### 6. Arrancar el frontend en background (con SSL)

Usar la herramienta Bash con `run_in_background: true`:

```
cd "D:\CLAUDE-REPO\AppCarFix\Frontend\car-fix-app" && npx ng serve --host 0.0.0.0 --port 4200 --ssl --ssl-cert "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-cert.pem" --ssl-key "D:\CLAUDE-REPO\AppCarFix\dev-certs\carfix-dev-key.pem"
```

Esperar hasta que el puerto 4200 esté escuchando (máximo 60 segundos):

```powershell
$ok = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 2
    if (Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue) { $ok = $true; break }
}
```

### 7. Verificar reglas de firewall (Private) para los puertos usados

Si esta es la primera vez que se corre la skill en la máquina, Windows Firewall bloquea por defecto las conexiones entrantes desde otros dispositivos (como el celular) a los puertos 4200/5151/5152 aunque el servicio esté corriendo — desde el navegador de la misma PC funciona igual (va por loopback) pero desde el celular no llega ni al firewall. Verificar y crear las reglas si faltan:

```powershell
foreach ($item in @(
    @{ Puerto = 4200; Nombre = "CarFix Frontend (Angular 4200)" },
    @{ Puerto = 5151; Nombre = "CarFix Backend HTTP (5151)" },
    @{ Puerto = 5152; Nombre = "CarFix Backend HTTPS (5152)" }
)) {
    $existe = Get-NetFirewallRule -Direction Inbound -Enabled True -Action Allow -ErrorAction SilentlyContinue |
        Get-NetFirewallPortFilter -ErrorAction SilentlyContinue |
        Where-Object { $_.LocalPort -eq [string]$item.Puerto }
    if (-not $existe) {
        New-NetFirewallRule -DisplayName $item.Nombre -Direction Inbound -Action Allow -Protocol TCP -LocalPort $item.Puerto -Profile Private | Out-Null
    }
}
```

Requiere sesión de PowerShell con privilegios de administrador; si no los tiene, avisar al usuario en vez de fallar en silencio.

### 8. Reportar resultado

Informar al usuario:

- Backend:  `https://<IP>:5152` (y `http://<IP>:5151` sin TLS) — estado (corriendo / error)
- Frontend: `https://<IP>:4200` — estado (corriendo / error)
- URL de acceso: `https://<IP>:4200`
- Credenciales de prueba: `admin` / `123456`
- **Recordatorio para probar desde celular** (obligatorio la primera vez o si se regeneró el certificado): el certificado es autofirmado, así que Chrome del celular va a mostrar advertencia de sitio no seguro. Hay que aceptarla manualmente **dos veces**, visitando primero `https://<IP>:5152` (cualquier ruta, ej. `/api`) y luego `https://<IP>:4200`, tocando Avanzado → Continuar en ambas — si solo se acepta la del frontend, las llamadas a la API fallan en silencio por certificado no confiable. Usar **Google Chrome real**, no el navegador por defecto del fabricante (ej. Samsung Internet no soporta bien el reconocimiento de voz).

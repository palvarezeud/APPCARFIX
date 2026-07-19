# sincronizar-esquema-azure

Compara el esquema y las tablas catalogo/configuracion entre la BD local (`localhost\SQL2022`) y la BD de Azure, y trae a **local** lo que falte o difiera. **Azure es la fuente de verdad** — esta skill nunca escribe en Azure, solo lee de ahi y aplica cambios en local.

Nace de dos casos reales de drift detectados en desarrollo: columnas `SubTotal`/`Pendiente` que existian en un entorno y no en el otro, y parametros (`HoraApertura`/`HoraCierre`/`ImpuestoVentas`) con valores distintos a los esperados. Correr esta skill al empezar una sesion de trabajo evita descubrir el drift a mitad de otra tarea.

## Alcance

- **Esquema:** todas las columnas de los esquemas `Catalogo` y `Sistema` (comparacion completa, mismo estilo de consultas que usa la skill `sync-db-docs`).
- **Datos:** solo estas 5 tablas catalogo/configuracion:
  - `Catalogo.Parametros` (clave `Nombre`)
  - `Catalogo.EstadoOrden` (clave `EstadoOrdenID`)
  - `Catalogo.EstadoFactura` (clave `EstadoFacturaID`)
  - `Catalogo.Roles` (clave `RolID`)
  - `Catalogo.Taller` (mono-registro, sin clave — se compara la unica fila esperada)
- **No toca:** `Catalogo.Usuarios`, `Catalogo.TipoReparacion`, `Catalogo.HistoricoRespuesto`, `Catalogo.MarcaModelo`, `Catalogo.Vehiculos`, `Catalogo.Clientes`, ni ninguna tabla de `Sistema` (`OrdenServicio`, `Facturas`, `Reparacion`, `Repuesto`) — son datos de prueba o especificos de cada entorno, no se sincronizan.

### Reglas de seguridad (asimetricas a proposito)

| Situacion | Accion |
|---|---|
| Columna/tabla existe en Azure pero no en local | Generar y **aplicar** `ALTER TABLE`/`ADD` en local |
| Columna/tabla existe en local pero no en Azure | Solo **advertir** — probablemente es trabajo local en progreso aun no desplegado (ver skill `desplegar-azure`) |
| Fila/valor de catalogo distinto o faltante en local | Generar y **aplicar** `INSERT`/`UPDATE` en local |
| Fila de catalogo existe en local pero no en Azure | Solo **advertir**, no se elimina |
| Tabla nueva completa solo en Azure | Solo **advertir** el nombre — no se genera `CREATE TABLE` automatico (recrear PK/FK/defaults desde `INFORMATION_SCHEMA` es mas riesgoso que un `ADD COLUMN`; requiere revision manual y probablemente re-scaffolding de EF) |
| Columna `NOT NULL` faltante en local, sin `DEFAULT` en Azure, y la tabla local ya tiene filas | Solo **advertir** — un `ALTER ADD` asi fallaria contra las filas existentes; requiere decidir un valor a mano |

## Pasos

### 1. Obtener la cadena de conexion de Azure en caliente

Nunca se escribe la contrasenna de Azure SQL a disco ni al repo — se obtiene fresca en cada ejecucion:

```powershell
$connStrAzure = az webapp config connection-string list --name appCarFix --resource-group appCarFixRG --query "[?name=='CarFix'].value" -o tsv
if (-not $connStrAzure) { Write-Error "No se pudo obtener la cadena de conexion de Azure. Verificar 'az account show'."; return }
```

**Importante:** el modulo `SQLPS`/`Invoke-Sqlcmd` instalado en esta maquina **no soporta pasar la cadena completa** via `-ConnectionString` — la cadena que devuelve Azure App Service incluye palabras clave modernas (`Connect Retry Count`, `Connect Retry Interval`) que hacen fallar `Invoke-Sqlcmd` con `"Palabra clave no admitida"`. Hay que parsearla en sus partes y usar `-ServerInstance`/`-Database`/`-Username`/`-Password`:

```powershell
function Parse-ConnString($cs) {
    $partes = @{}
    foreach ($par in $cs.Split(';')) {
        if ($par.Trim() -eq '') { continue }
        $kv = $par.Split('=', 2)
        $partes[$kv[0].Trim().ToLower()] = $kv[1].Trim()
    }
    return $partes
}

$p = Parse-ConnString $connStrAzure
$serverAzure = ($p['server'] -replace '^tcp:','') -replace ',1433$',''
$dbAzure     = $p['initial catalog']
$userAzure   = $p['user id']
$passAzure   = $p['password']
```

Todas las llamadas de los pasos siguientes contra Azure usan `Invoke-Sqlcmd -ServerInstance $serverAzure -Database $dbAzure -Username $userAzure -Password $passAzure -Query ...` en vez de `-ConnectionString`.

### 2. Comparar esquema (columnas de `Catalogo` y `Sistema`)

```powershell
Import-Module SQLPS -DisableNameChecking 3>$null

$qColumnas = @"
SELECT c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE,
       c.CHARACTER_MAXIMUM_LENGTH, c.NUMERIC_PRECISION, c.NUMERIC_SCALE,
       c.IS_NULLABLE, c.ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_SCHEMA IN ('Catalogo','Sistema')
ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;
"@

$colsLocal = Invoke-Sqlcmd -ServerInstance "localhost\SQL2022" -Database "CAR_FIX" -Query $qColumnas
$colsAzure = Invoke-Sqlcmd -ServerInstance $serverAzure -Database $dbAzure -Username $userAzure -Password $passAzure -Query $qColumnas
```

Comparar por `(TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME)`. Para cada columna que este en `$colsAzure` y no en `$colsLocal`: es candidata a `ALTER TABLE ... ADD`. Antes de generar el `ALTER`, revisar si la tabla local ya tiene filas (`SELECT COUNT(*) FROM esquema.tabla`) y si la columna es `NOT NULL` — de ser asi, traer tambien el `DEFAULT` real desde Azure:

```sql
SELECT s.name AS TABLE_SCHEMA, t.name AS TABLE_NAME, c.name AS COLUMN_NAME, dc.definition AS DEFAULT_DEFINITION
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
JOIN sys.tables  t ON c.object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id;
```

Si hay `DEFAULT`, incluirlo en el `ALTER` (`ADD [Columna] tipo NOT NULL DEFAULT (valor)`); si no hay y la tabla tiene filas, pasar esa columna a la lista de advertencias en vez de generar el `ALTER`.

Columnas que estan en `$colsLocal` y no en `$colsAzure` → agregar a la lista de advertencias (no se tocan).

### 3. Comparar datos de las 5 tablas catalogo

Mismo patron para cada una — `SELECT *` de ambos lados, comparar por la clave indicada, generar `INSERT` para lo que falte en local y `UPDATE` para lo que difiera. Ejemplo completo con `Parametros`:

```powershell
$paramsLocal = Invoke-Sqlcmd -ServerInstance "localhost\SQL2022" -Database "CAR_FIX" -Query "SELECT Nombre, Valor FROM Catalogo.Parametros;"
$paramsAzure = Invoke-Sqlcmd -ServerInstance $serverAzure -Database $dbAzure -Username $userAzure -Password $passAzure -Query "SELECT Nombre, Valor FROM Catalogo.Parametros;"
```

Para cada fila de `$paramsAzure`:
- Si `Nombre` no existe en `$paramsLocal` → `INSERT INTO Catalogo.Parametros (Nombre, Valor) VALUES ('<Nombre>', '<Valor>');`
- Si existe pero `Valor` difiere → `UPDATE Catalogo.Parametros SET Valor = '<Valor>' WHERE Nombre = '<Nombre>';`

Repetir el mismo patron para:

| Tabla | Clave de comparacion | Columnas a diffear |
|---|---|---|
| `Catalogo.EstadoOrden` | `EstadoOrdenID` | `Descripcion` |
| `Catalogo.EstadoFactura` | `EstadoFacturaID` | `Descipcion` (typo real de la columna, no corregir aqui) |
| `Catalogo.Roles` | `RolID` | `Nombre`, `Descripcion` |
| `Catalogo.Taller` | (mono-registro, sin filtro) | `Nombre`, `UbicacionDescripcion`, `Telefonos`, `Email` — si Azure tiene una fila y local no tiene ninguna, `INSERT`; si ambos tienen fila, `UPDATE` la de local; ignorar `UbicaciónGPS` (geography, formato distinto, no critico para desarrollo) |

Filas que existen en local y no en Azure → advertencia, no se eliminan.

### 4. Generar y mostrar el script combinado

Unir todos los `ALTER`/`INSERT`/`UPDATE` detectados en un solo bloque SQL y mostrarlo en la conversacion junto con la lista de advertencias (columnas/tablas/filas solo-en-local, tablas nuevas solo-en-Azure) **antes** de ejecutarlo. El riesgo es bajo (BD local de desarrollo) pero mantener la transparencia de que se va a modificar la BD.

Si no hay ningun cambio detectado, decirlo explicitamente ("local ya esta sincronizado con Azure en el alcance de esta skill") y saltar los pasos 5-6.

### 5. Aplicar en local

```powershell
Invoke-Sqlcmd -ServerInstance "localhost\SQL2022" -Database "CAR_FIX" -Query $scriptGenerado
```

Volver a correr las comparaciones de los pasos 2-3 y confirmar que ya no quedan diffs pendientes (aparte de las advertencias local-only, que son esperadas y no se resuelven aqui).

### 6. Reportar

Resumen corto: cuantas columnas/filas se agregaron o actualizaron en local, y la lista de advertencias local-only (columnas, tablas o filas que existen solo en local) para que el usuario decida si eso debe subirse a Azure (con la skill `desplegar-azure`) o descartarse.

## Notas

- Esta skill es de un solo sentido (Azure → local). Para llevar cambios de local a Azure, ese es el trabajo de la skill `desplegar-azure` (que incluye el codigo de aplicacion, no cambios de esquema/datos de BD sueltos — un `ALTER`/`INSERT` manual en Azure sigue siendo una accion aparte que el usuario debe confirmar explicitamente, igual que hoy).
- No usar `az login` ni cambiar la suscripcion activa dentro de esta skill — si `az account show` falla o apunta a la suscripcion equivocada, avisar y detenerse.
- Si en el futuro se agregan mas tablas catalogo/configuracion al proyecto, agregarlas a la tabla del paso 3 en vez de crear una skill nueva.

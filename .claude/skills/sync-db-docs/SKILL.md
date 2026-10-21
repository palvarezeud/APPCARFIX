# sync-db-docs

Conecta a la base de datos CAR_FIX en `(localdb)\MSSQLLocalDB` y regenera `.claude/DATABASE_DOCUMENTATION.md` con el esquema actual.

## Pasos

### 1. Extraer el esquema actual

Ejecuta los siguientes bloques con `Invoke-Sqlcmd` (o `sqlcmd` si no está disponible el módulo). La cadena de conexión es:
```
Server=(localdb)\MSSQLLocalDB;Database=CAR_FIX;Integrated Security=True;TrustServerCertificate=True
```

Parámetros comunes para `Invoke-Sqlcmd`:
```powershell
$server   = "(localdb)\MSSQLLocalDB"
$database = "CAR_FIX"
```

> **Esquemas:** las tablas de la aplicación viven en los esquemas `Catalogo` (maestros y configuración) y `Sistema` (transaccionales), **no** en `dbo`. Todas las consultas filtran por esos dos esquemas y devuelven el nombre calificado `Esquema.Tabla`. `dbo.sysdiagrams` (creada por el diseñador de diagramas de SQL Server) queda excluida a propósito.

#### 1.1 Tablas del usuario

```sql
SELECT
    s.name + '.' + t.name AS Tabla
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name IN ('Catalogo', 'Sistema')
ORDER BY s.name, t.name;
```

#### 1.2 Columnas de cada tabla (tipo, nulabilidad, PK, identity y default en una sola consulta)

```sql
SELECT
    s.name + '.' + t.name AS Tabla,
    c.column_id           AS Orden,
    c.name                AS Columna,
    ty.name + CASE
        WHEN ty.name IN ('varchar', 'char', 'varbinary', 'binary')
            THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS varchar) END + ')'
        WHEN ty.name IN ('nvarchar', 'nchar')
            THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length / 2 AS varchar) END + ')'
        WHEN ty.name IN ('decimal', 'numeric')
            THEN '(' + CAST(c.precision AS varchar) + ',' + CAST(c.scale AS varchar) + ')'
        ELSE ''
    END                   AS Tipo,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS Nulo,
    CASE WHEN pk.column_id IS NOT NULL THEN 'PK' ELSE '' END AS PK,
    CASE WHEN ic.column_id IS NOT NULL
        THEN '(' + CAST(ic.seed_value AS varchar) + ',' + CAST(ic.increment_value AS varchar) + ')'
        ELSE '' END       AS Identidad,
    ISNULL(dc.definition, '') AS ValorDefault
FROM sys.tables t
JOIN sys.schemas s  ON s.schema_id = t.schema_id
JOIN sys.columns c  ON c.object_id = t.object_id
JOIN sys.types   ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.identity_columns    ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
LEFT JOIN (
    SELECT icx.object_id, icx.column_id
    FROM sys.indexes i
    JOIN sys.index_columns icx ON icx.object_id = i.object_id AND icx.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
WHERE s.name IN ('Catalogo', 'Sistema')
ORDER BY Tabla, c.column_id;
```

#### 1.3 Nombres de las claves primarias

```sql
SELECT
    s.name + '.' + t.name AS Tabla,
    i.name                AS ConstraintPK
FROM sys.indexes i
JOIN sys.tables  t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE i.is_primary_key = 1
  AND s.name IN ('Catalogo', 'Sistema')
ORDER BY Tabla;
```

#### 1.4 Claves foráneas con DELETE/UPDATE rules

```sql
SELECT
    fk.name                            AS FK_Name,
    sp.name + '.' + tp.name            AS Tabla_Hija,
    cp.name                            AS Columna_Hija,
    sr.name + '.' + tr.name            AS Tabla_Padre,
    cr.name                            AS Columna_Padre,
    fk.delete_referential_action_desc  AS On_Delete,
    fk.update_referential_action_desc  AS On_Update
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables  tp ON fkc.parent_object_id     = tp.object_id
JOIN sys.schemas sp ON sp.schema_id             = tp.schema_id
JOIN sys.columns cp ON fkc.parent_object_id     = cp.object_id
                   AND fkc.parent_column_id     = cp.column_id
JOIN sys.tables  tr ON fkc.referenced_object_id = tr.object_id
JOIN sys.schemas sr ON sr.schema_id             = tr.schema_id
JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id
                   AND fkc.referenced_column_id = cr.column_id
ORDER BY Tabla_Hija, Columna_Hija;
```

#### 1.5 Valores de las tablas catálogo (para documentar estados y semillas)

```sql
SELECT 'EstadoOrden'                 AS Tabla, EstadoOrdenID                 AS ID, Descripcion                          AS Valor FROM Catalogo.EstadoOrden
UNION ALL
SELECT 'EstadoFactura',                        EstadoFacturaID,                     RTRIM(Descipcion)                              FROM Catalogo.EstadoFactura
UNION ALL
SELECT 'Roles',                                RolID,                               Nombre + ' / ' + ISNULL(Descripcion, '')       FROM Catalogo.Roles
UNION ALL
SELECT 'Parametros',                           ParametroID,                         Nombre + ' = ' + Valor                         FROM Catalogo.Parametros
UNION ALL
SELECT 'HojaInspeccionDetalleEstado',          HojaInspeccionDetalleEstadoID,       DescripcionEstado                              FROM Catalogo.HojaInspeccionDetalleEstado
UNION ALL
SELECT 'HojaInspeccionSeccion',                HojaInspeccionSeccionID,             DescripcionSeccion                             FROM Catalogo.HojaInspeccionSeccion
ORDER BY Tabla, ID;
```

Si alguna de estas tablas ya no existe (la consulta 1.1 no la lista), quitar su `UNION ALL` antes de ejecutar. Para catálogos grandes (ej. `Catalogo.MarcaModelo`) documentar solo el conteo (`SELECT COUNT(*) FROM Catalogo.MarcaModelo`), no las filas.

### 2. Actualizar el documento

Esta skill **solo reemplaza** las siguientes dos secciones de `.claude/DATABASE_DOCUMENTATION.md`. El resto del documento (encabezado, descripción general, flujo de negocio, observaciones, resumen) se mantiene intacto tal como está.

#### Sección 1 — `## Diagrama de Relaciones (ERD)`

Reemplazar el contenido entre `## Diagrama de Relaciones (ERD)` y el siguiente `---` con:

```
## Diagrama de Relaciones (ERD)

[diagrama ASCII actualizado basado en las FKs reales encontradas]

### Relaciones principales

| Tabla hija | FK Column | Tabla padre | PK Column | ON DELETE | ON UPDATE |
|------------|-----------|-------------|-----------|-----------|-----------|
[una fila por cada FK constraint real de la BD]

> [nota si alguna relación es solo lógica (sin FK constraint definido)]
```

#### Sección 2 — `## Tablas`

Reemplazar el contenido entre `## Tablas` y `## Flujo de Negocio` con el esquema real obtenido de las consultas. Esto incluye tablas nuevas, tablas eliminadas, columnas nuevas, columnas eliminadas y cambios de tipo o nulabilidad. La sección se regenera completa a partir del estado actual de la BD.

```
## Tablas

[por cada tabla, orden: primero transaccionales, luego catálogos]

### `Esquema.NombreTabla`

[descripción de una línea de qué almacena]

| Columna | Tipo | Nulo | PK | Identity | Default | Descripción |
|---------|------|------|----|----------|---------|-------------|
[fila por columna con datos reales de la BD]

**Relaciones:** [FKs entrantes y salientes de esta tabla]

[observación solo si hay algo no obvio: typo, FK faltante, tipo inadecuado, etc.]

---
```

### 3. Reglas al escribir

- Actualizar la fecha en el encabezado `> **Fecha de análisis:**` a la fecha de hoy (YYYY-MM-DD).
- Nombrar las tablas siempre calificadas con su esquema (`Catalogo.Clientes`, `Sistema.Facturas`).
- Para el tipo de columna, incluir longitud/precisión real: `varchar(250)`, `money`, `tinyint`, etc.
- Identity: mostrar `✓ (seed,inc)` si aplica, vacío si no.
- Si una FK no tiene constraint en la BD, indicarlo como "FK lógica".
- No tocar ninguna otra sección del documento.

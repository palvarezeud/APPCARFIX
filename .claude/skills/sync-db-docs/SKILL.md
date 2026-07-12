# sync-db-docs

Conecta a la base de datos CAR_FIX en `localhost\SQL2022` y regenera `.claude/DATABASE_DOCUMENTATION.md` con el esquema actual.

## Pasos

### 1. Extraer el esquema actual

Ejecuta los siguientes bloques con `Invoke-Sqlcmd` (o `sqlcmd` si no está disponible el módulo). La cadena de conexión es:
```
Server=localhost\SQL2022;Database=CAR_FIX;Integrated Security=True;TrustServerCertificate=True
```

Parámetros comunes para `Invoke-Sqlcmd`:
```powershell
$server   = "localhost\SQL2022"
$database = "CAR_FIX"
```

#### 1.1 Tablas del usuario

```sql
SELECT
    t.TABLE_NAME,
    t.TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_SCHEMA = 'dbo'
  AND t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME;
```

#### 1.2 Columnas de cada tabla

```sql
SELECT
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT,
    c.ORDINAL_POSITION,
    COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_SCHEMA = 'dbo'
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;
```

#### 1.3 Claves primarias

```sql
SELECT
    tc.TABLE_NAME,
    kcu.COLUMN_NAME,
    tc.CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
   AND tc.TABLE_SCHEMA    = kcu.TABLE_SCHEMA
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND tc.TABLE_SCHEMA    = 'dbo'
ORDER BY tc.TABLE_NAME, kcu.ORDINAL_POSITION;
```

#### 1.4 Claves foráneas con DELETE/UPDATE rules

```sql
SELECT
    fk.name                          AS FK_Name,
    tp.name                          AS Parent_Table,
    cp.name                          AS Parent_Column,
    tr.name                          AS Referenced_Table,
    cr.name                          AS Referenced_Column,
    fk.delete_referential_action_desc AS On_Delete,
    fk.update_referential_action_desc AS On_Update
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables  tp ON fkc.parent_object_id      = tp.object_id
JOIN sys.columns cp ON fkc.parent_object_id      = cp.object_id
                    AND fkc.parent_column_id      = cp.column_id
JOIN sys.tables  tr ON fkc.referenced_object_id  = tr.object_id
JOIN sys.columns cr ON fkc.referenced_object_id  = cr.object_id
                    AND fkc.referenced_column_id  = cr.column_id
ORDER BY tp.name, cp.name;
```

#### 1.5 Valores semilla de columnas IDENTITY

```sql
SELECT
    OBJECT_NAME(object_id) AS TABLE_NAME,
    name                   AS COLUMN_NAME,
    seed_value,
    increment_value
FROM sys.identity_columns
ORDER BY TABLE_NAME;
```

#### 1.6 Valores de las tablas catálogo (para documentar estados)

```sql
SELECT 'EstadoOrden'    AS Tabla, CAST(EstadoOrdenID AS varchar) AS ID, Descripcion AS Valor FROM EstadoOrden
UNION ALL
SELECT 'EstadoFactura'  AS Tabla, CAST(EstadoFacturaID AS varchar), Descipcion FROM EstadoFactura
ORDER BY Tabla, ID;
```

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

### `NombreTabla`

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
- Para el tipo de columna, incluir longitud/precisión real: `varchar(250)`, `money`, `tinyint`, etc.
- Identity: mostrar `✓ (seed,inc)` si aplica, vacío si no.
- Si una FK no tiene constraint en la BD, indicarlo como "FK lógica".
- No tocar ninguna otra sección del documento.

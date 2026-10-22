# Estandar de Base de Datos

> Estandar de diseno de bases de datos relacionales (SQL Server) derivado de la experiencia con **CAR_FIX**.
> Sirve como referencia obligatoria para sistemas futuros. Cada regla indica, cuando aplica, como se hizo en CAR_FIX
> y que se debe hacer distinto (seccion "Lecciones aprendidas").

---

## 1. Motor y Entorno

| Aspecto | Estandar |
|---|---|
| Motor | SQL Server (LocalDB en desarrollo, Azure SQL / SQL Server en produccion) |
| Desarrollo local | `(localdb)\MSSQLLocalDB` con `Integrated Security=True;TrustServerCertificate=True` |
| Nombre de la BD | MAYUSCULAS con guion bajo: `CAR_FIX`, `NOMBRE_SISTEMA` |
| Acceso desde la app | Entity Framework Core **Database First** (la BD es la fuente de verdad; las entidades se generan con `scaffold`, nunca a mano) |
| Cadena de conexion | Nunca en el repositorio para produccion: `dotnet user-secrets` en desarrollo y variables de entorno en produccion |

---

## 2. Nomenclatura General

**Todo el esquema se escribe en espanol, sin caracteres especiales.** Esto incluye esquemas, tablas, columnas, constraints e indices.

| Regla | Correcto | Incorrecto |
|---|---|---|
| Sin tildes | `Direccion`, `Localizacion`, `UbicacionGPS` | `Dirección`, `UbicaciónGPS` |
| Sin ñ — se reemplaza por `nn` | `Annio`, `Contrasenna` | `Año`, `Anio`, `Contraseña` |
| PascalCase, sin espacios ni guiones bajos | `NombreCliente`, `FechaIngreso` | `nombre_cliente`, `Fecha Ingreso` |
| Sufijo `ID` en mayusculas para identificadores | `ClienteID`, `FacturaID` | `ClienteId`, `IdCliente`, `id` |
| Siglas reconocidas en mayusculas | `VIN`, `GPS` | `Vin`, `Gps` |
| Revisar ortografia antes de crear el objeto | `RepuestoDescripcion`, `Descripcion` | `RepuestoDecripcion`, `Descipcion` |

> **Por que importa:** EF Core Database First genera las clases y propiedades con el nombre exacto de la BD.
> Un typo o una tilde en la BD se propaga a todo el backend, los DTOs y el frontend, y corregirlo despues
> obliga a renombrar en BD + regenerar entidades + tocar cada referencia.

---

## 3. Esquemas

Las tablas se separan por proposito en dos esquemas (nunca en `dbo`):

| Esquema | Contenido | Ejemplos en CAR_FIX |
|---|---|---|
| `Catalogo` | Maestros, catalogos de estados, configuracion y seguridad (datos que cambian poco o que otras tablas referencian) | `Clientes`, `Vehiculos`, `EstadoOrden`, `EstadoFactura`, `TipoReparacion`, `MarcaModelo`, `Parametros`, `Taller`, `Roles`, `Usuarios` |
| `Sistema` | Tablas transaccionales/documentos (crecen con la operacion diaria) | `OrdenServicio`, `Facturas`, `Reparacion`, `Repuesto`, `TokenRefresco` |

Si un sistema futuro lo amerita se pueden agregar esquemas adicionales (ej. `Auditoria`, `Integracion`), siempre con nombre en espanol y PascalCase.

---

## 4. Tablas

### 4.1 Nombre

- **Plural** para todas las tablas: `Clientes`, `Vehiculos`, `Facturas`, `Reparaciones`, `EstadosOrden`.
- Nombre descriptivo del concepto de negocio, no de la pantalla.
- Una tabla **nunca se renombra sin renombrar tambien su PK y sus constraints** (ver Lecciones aprendidas: `PK_OrdenReparacion` quedo en la tabla `OrdenServicio`).

> En CAR_FIX se mezclo plural (`Clientes`, `Facturas`) y singular (`OrdenServicio`, `Reparacion`, `EstadoOrden`). En sistemas nuevos usar plural en todas.

### 4.2 Clasificacion de tablas

Toda tabla debe poder clasificarse en uno de estos tipos (y documentarse asi):

| Tipo | Descripcion | Ejemplo |
|---|---|---|
| **Transaccional** | Documentos y movimientos de la operacion | `OrdenServicio`, `Facturas`, `Reparacion` |
| **Maestro** | Entidades de negocio que se mantienen por pantalla CRUD | `Clientes`, `Vehiculos` |
| **Catalogo de estados** | Lista cerrada de valores con IDs fijos usados por la logica | `EstadoOrden`, `EstadoFactura` |
| **Catalogo de consulta** | Datos de referencia para busqueda/autocompletado, sin FK | `TipoReparacion`, `MarcaModelo`, `HistoricoRespuesto` |
| **Configuracion** | Parametros del sistema | `Parametros` (clave/valor), `Taller` (mono-registro) |
| **Seguridad** | Usuarios, roles, tokens | `Roles`, `Usuarios`, `TokenRefresco` |

---

## 5. Columnas

### 5.1 Llave primaria

- Una sola columna, `int IDENTITY(1,1) NOT NULL`, llamada **`<EntidadSingular>ID`**: `ClienteID`, `FacturaID`, `OrdenServicioID`.
- Siempre es la primera columna de la tabla.
- **Excepcion — catalogos de estados:** IDs asignados explicitamente (sin IDENTITY o insertando con `SET IDENTITY_INSERT ON`) para garantizar que el ID 1, 2, 3... sea el mismo en todos los ambientes, porque el codigo depende de esos valores (`const int estadoCotizacionId = 1;`).
- No usar llaves compuestas ni naturales (placa, email, VIN) como PK; si deben ser unicas, usar un indice `UNIQUE`.

### 5.2 Llaves foraneas

- La columna FK se llama **exactamente igual** que la PK que referencia: `Vehiculos.ClienteID → Clientes.ClienteID`.
- Si una tabla referencia dos veces la misma tabla, agregar un prefijo de rol: `UsuarioCreaID`, `UsuarioModificaID`.
- Toda FK es una **constraint real** en la BD (no "FK logicas" solo en la aplicacion).
- No poner `DEFAULT` a una columna FK salvo que el valor por defecto exista en el catalogo (ver Lecciones aprendidas).

### 5.3 Tipos de datos

| Concepto | Tipo | Nombre de columna | Ejemplo |
|---|---|---|---|
| Identificador | `int` | `<Entidad>ID` | `VehiculoID` |
| Texto corto | `varchar(n)` con largo realista | sustantivo | `Placa varchar(50)`, `NombreCliente varchar(250)` |
| Texto largo libre | `varchar(MAX)` **solo** si de verdad puede ser largo | `Descripcion*`, `Notas`, `ProblemaGeneral` | `ProblemaGeneral varchar(MAX)` |
| Telefono | `varchar(50)` | `Telefono1`, `Telefono2` | |
| Email | `varchar(250)` | `Email` | |
| Monto de dinero | `money` (o `decimal(18,2)`) — **nunca** `float` | `Costo`, `Precio`, `Total*` | `Costo money NOT NULL DEFAULT 0` |
| Booleano | `bit NOT NULL` con `DEFAULT` explicito | `Es*` o participio/adjetivo | `EsEmpresa`, `EsGarantia`, `Activo`, `Listo`, `Incluido`, `Revocado` |
| Fecha y hora | `datetime` (o `datetime2(0)` en sistemas nuevos), en **UTC** cuando la registra el sistema | `Fecha*` | `FechaIngreso`, `FechaCreacion DEFAULT GETUTCDATE()` |
| Anio | `smallint` o `int` — **nunca** `tinyint` (maximo 255) | `Annio` | `Annio smallint` |
| Cantidad de horas/unidades | `int` | `Duracion*Horas`, `Cantidad` | `DuracionAproximadaHoras int` |
| Coordenadas | `geography` | `Localizacion`, `UbicacionGPS` | |
| Hash de contrasenna/token | `varchar(100..500)` | `PasswordHash`, `TokenHash` | |
| Texto de catalogo | `varchar` — **no** `nchar`/`char` (rellena con espacios) | `Descripcion` | |

**Reglas adicionales:**
- Usar `varchar` (no `nvarchar`) mientras el sistema sea solo en espanol con collation latina; los datos si pueden llevar tildes (`'Cotización'`), solo los **nombres de objetos** no.
- Si en el futuro se necesitan caracteres fuera de Latin1 (emojis, otros alfabetos), migrar a `nvarchar` desde el diseno inicial, no a mitad del proyecto.

### 5.4 Nulabilidad y defaults

- `NOT NULL` por defecto; `NULL` solo cuando el negocio dice que el dato es opcional (ej. `Telefono2`, `Email`, `Placa`).
- La nulabilidad de la BD debe coincidir con la obligatoriedad documentada en la especificacion funcional.
- Todo `bit` y todo monto acumulado (`Adelanto`, `Pendiente`, `SubTotal`) lleva `DEFAULT 0`.
- Los `DEFAULT` deben ser valores reales, nunca textos de ejemplo (en CAR_FIX quedo `DEFAULT 'Marca del vehiculo'` en `MarcaModelo.Marca`).

### 5.5 Orden de columnas

1. PK
2. FKs principales (padre directo)
3. Datos de negocio
4. Montos / totales
5. Estado (`Estado*ID`) y banderas (`Es*`, `Activo`)
6. Auditoria (`FechaCreacion`, etc.)

---

## 6. Constraints e Indices — Nomenclatura

Toda constraint se crea **con nombre explicito** (nunca dejar que SQL Server genere `PK__Clientes__3A8...`).

| Objeto | Formato | Ejemplo |
|---|---|---|
| Llave primaria | `PK_<Tabla>` | `PK_TokenRefresco` |
| Llave foranea | `FK_<TablaHija>_<TablaPadre>` | `FK_TokenRefresco_Usuarios`, `FK_Facturas_Vehiculos` |
| Default | `DF_<Tabla>_<Columna>` | `DF_TokenRefresco_FechaCreacion` |
| Unique | `UQ_<Tabla>_<Columna>` | `UQ_TokenRefresco_TokenHash` |
| Indice | `IX_<Tabla>_<Columna(s)>` | `IX_TokenRefresco_UsuarioID` |
| Check | `CK_<Tabla>_<Regla>` | `CK_TipoReparacion_DuracionMinima` |

**Indices obligatorios:**
- Un indice `IX_` por cada columna FK (SQL Server no los crea automaticamente).
- `UNIQUE` sobre llaves naturales de negocio: `Usuarios.NombreUsuario`, `TokenRefresco.TokenHash`.
- Indices adicionales para los filtros de las pantallas de listado (ej. `Vehiculos.Placa`).

**Checks recomendados** para reglas que no dependen de otras tablas: montos `>= 0`, duracion minima (`DuracionAproximadaHoras >= 1`), `FechaSalida >= FechaIngreso`.

---

## 7. Integridad Referencial — Politica de ON DELETE / ON UPDATE

| Tipo de relacion | ON DELETE | ON UPDATE | Ejemplo CAR_FIX |
|---|---|---|---|
| **Composicion** (el hijo no existe sin el padre: lineas de detalle) | `CASCADE` | `CASCADE` | `Facturas → Reparacion`, `Facturas → Repuesto`, `Usuarios → TokenRefresco` |
| **Propiedad con eliminacion permitida** (el negocio acepta borrar en cascada) | `CASCADE` | `CASCADE` | `Clientes → Vehiculos` |
| **Referencia a catalogo** (estados, roles) | `NO ACTION` | `CASCADE` | `EstadoOrden → OrdenServicio`, `Roles → Usuarios` |
| **Referencia entre documentos** (factura, orden) | `NO ACTION` | `NO ACTION` | `Facturas → OrdenServicio`, `Vehiculos → Facturas` |

**Reglas:**
- `NO ACTION` es la proteccion de ultimo recurso; las reglas de negocio de eliminacion ("no se puede eliminar un cliente con facturas") se validan **tambien en la capa de Aplicacion** y devuelven un mensaje claro (422), no un error de FK (500).
- Cuando dos entidades se crean juntas (Orden + Factura), el orden de insercion y eliminacion lo dicta la FK obligatoria: se inserta primero el padre (`Factura`) y se elimina primero el dependiente (`OrdenServicio`), todo en una sola transaccion (patron Unidad de Trabajo).
- Evitar ciclos de `CASCADE` (SQL Server los rechaza con "may cause cycles or multiple cascade paths").

---

## 8. Patrones de Diseno Reutilizables

### 8.1 Catalogo de estados

```sql
CREATE TABLE Catalogo.EstadosFactura (
    EstadoFacturaID  INT          NOT NULL,
    Descripcion      VARCHAR(100) NOT NULL,
    CONSTRAINT PK_EstadosFactura PRIMARY KEY (EstadoFacturaID)
);

INSERT INTO Catalogo.EstadosFactura (EstadoFacturaID, Descripcion) VALUES
    (1, 'Cotización'), (2, 'Pendiente'), (3, 'Pagada');
```

- IDs fijos y documentados en la especificacion funcional; el codigo los referencia como constantes.
- Columna de texto siempre `Descripcion`.
- Las transiciones validas entre estados y los estados "bloqueados" (ej. Pagada no se modifica) se validan en la aplicacion.

### 8.2 Snapshot desnormalizado (historico)

Cuando un documento debe conservar el dato **tal como era** al momento de emitirse, se copia el valor en vez de solo referenciarlo:
- `Facturas.NombreCliente` — nombre del cliente al momento de facturar.
- `HistoricoRespuesto.Marca/Modelo/Annio` — datos del vehiculo en texto, sin FK, para que el historico sobreviva a cambios o eliminaciones.

Documentar siempre la columna como "snapshot" para que nadie la "corrija" convirtiendola en FK.

### 8.3 Totales calculados persistidos

Los totales de un documento (`SubTotal`, `ImpuestoVentas`, `Total`, `Pendiente`) se **guardan** en la tabla (para listados y reportes rapidos) pero se **calculan solo en un lugar** de la aplicacion (en CAR_FIX: `RecalculadorTotalesFactura`), que se invoca cada vez que cambia cualquier dato de entrada. Nunca son editables por el usuario.

### 8.4 Parametros clave/valor

```sql
CREATE TABLE Catalogo.Parametros (
    ParametroID INT IDENTITY(1,1) NOT NULL,
    Nombre      VARCHAR(200)      NOT NULL,   -- clave: ImpuestoVentas, HoraApertura...
    Valor       VARCHAR(MAX)      NOT NULL,   -- valor en texto
    CONSTRAINT PK_Parametros PRIMARY KEY (ParametroID),
    CONSTRAINT UQ_Parametros_Nombre UNIQUE (Nombre)
);
```

- Para tasas, horarios y valores configurables que no justifican una tabla propia.
- La aplicacion valida el formato al leer y **falla explicitamente** si un parametro critico falta o es invalido (no inventar un valor por defecto silencioso cuando el resultado afecta dinero o fechas).
- Agregar `UNIQUE` sobre `Nombre`.

### 8.5 Tabla de configuracion mono-registro

Datos unicos del negocio (nombre, direccion, telefonos, email — en CAR_FIX `Catalogo.Taller`). La aplicacion garantiza que exista exactamente una fila y valida su existencia antes de usarla (ej. antes de generar un PDF). Debe tener pantalla de mantenimiento desde el inicio, no quedar como pendiente.

### 8.6 Seguridad

| Tabla | Reglas |
|---|---|
| `Roles` | Catalogo con IDs fijos; `Nombre` sin espacios ni tildes porque viaja en el JWT (`Administrador`, `JefeMecanicos`, `Mecanico`) |
| `Usuarios` | `NombreUsuario` `UNIQUE`; `PasswordHash` BCrypt (`varchar(500)`); baja logica con `Activo bit DEFAULT 1` en vez de `DELETE` |
| `TokenRefresco` | Se guarda solo el **hash** del token, nunca el token en claro; `FechaExpiracion`, `Revocado`, `FechaRevocado`; `CASCADE` desde `Usuarios` |

### 8.7 Datos transitorios

Archivos procesados por IA (fotos de documentos, facturas escaneadas) y PDFs generados **no se persisten** en la BD; solo se guardan los datos extraidos y validados por el usuario.

---

## 9. Scripts y Cambios de Esquema

- Todo cambio de esquema se hace con un **script SQL versionado** en el repositorio: `Backend/Scripts/sql/AAAA-MM-DD_descripcion_del_cambio.sql` (ej. `2026-07-23_crear_tabla_token_refresco.sql`).
- El script empieza con un comentario que explica el **por que** del cambio y la seccion de la especificacion relacionada.
- Separar lotes con `GO`; crear constraints e indices con nombre explicito dentro del mismo script.
- Preferir scripts **idempotentes** (`IF NOT EXISTS (...)`, `IF COL_LENGTH(...) IS NULL`) para poder re-ejecutarlos en otros ambientes (local, Azure).
- Datos semilla (estados, roles, parametros) en scripts propios, revisados contra ortografia y espacios al final.
- Despues de cada cambio:
  1. Ejecutar el script en local.
  2. Regenerar entidades con `dotnet ef dbcontext scaffold ... --force` (nunca editar los archivos generados; extender con clases parciales).
  3. Regenerar la documentacion del esquema (`/sync-db-docs`).
  4. Aplicar el mismo script en los demas ambientes (`/sincronizar-esquema-azure`).

---

## 10. Documentacion Obligatoria

### 10.1 Documentacion dentro de la BD (`MS_Description`)

**Toda tabla y toda columna se documenta en la propia base de datos** mediante la propiedad extendida `MS_Description`. Es la fuente primaria de la documentacion: viaja con la BD, se ve en SSMS/Azure Data Studio y alimenta la generacion automatica de `DATABASE_DOCUMENTATION.md`.

```sql
-- Descripcion de la tabla
EXEC sys.sp_addextendedproperty
    @name = N'MS_Description', @value = N'Almacena el catalogo de clientes',
    @level0type = N'SCHEMA', @level0name = N'Catalogo',
    @level1type = N'TABLE',  @level1name = N'Clientes';

-- Descripcion de una columna
EXEC sys.sp_addextendedproperty
    @name = N'MS_Description', @value = N'Identificador unico del cliente',
    @level0type = N'SCHEMA', @level0name = N'Catalogo',
    @level1type = N'TABLE',  @level1name = N'Clientes',
    @level2type = N'COLUMN', @level2name = N'ClienteID';

-- Para corregir una descripcion existente usar sys.sp_updateextendedproperty con los mismos parametros
```

**Reglas:**
- La descripcion de la **tabla** explica su proposito de negocio ("Almacena el catalogo de clientes").
- La descripcion de cada **columna** explica que significa el dato, no repite el nombre: unidades (`horas`, `colones con IVA`), significado de los valores de un `bit` (`1 = empresa, 0 = persona natural`), si es snapshot, si es calculada por la aplicacion, y a que tabla apunta si es FK.
- Las descripciones se crean **en el mismo script** que crea la tabla o agrega la columna; un objeto sin `MS_Description` se considera incompleto.
- Usar literales `N'...'` para que las tildes de las descripciones se guarden correctamente (las descripciones si pueden llevar tildes; los nombres de objetos no).
- Consulta para detectar columnas sin documentar (debe devolver 0 filas):

```sql
SELECT s.name AS Esquema, t.name AS Tabla, c.name AS Columna
FROM sys.columns c
JOIN sys.tables  t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name <> 'sysdiagrams'
  AND NOT EXISTS (SELECT 1 FROM sys.extended_properties ep
                  WHERE ep.class = 1 AND ep.major_id = c.object_id
                    AND ep.minor_id = c.column_id AND ep.name = 'MS_Description')
ORDER BY 1, 2, c.column_id;
```

### 10.2 Documento del esquema (`DATABASE_DOCUMENTATION.md`)

Cada sistema mantiene un `DATABASE_DOCUMENTATION.md` generado desde la BD real (tomando las descripciones de `MS_Description`) con:

1. Encabezado: servidor, BD y fecha de analisis.
2. Diagrama de relaciones (ERD en texto) agrupado por esquema.
3. Tabla de relaciones: hija, FK, padre, PK, ON DELETE, ON UPDATE.
4. Una seccion por tabla: proposito, columnas (tipo, nulo, PK, identity, default, descripcion), relaciones y observaciones.
5. Valores actuales de todos los catalogos de estados y parametros.
6. Lista de observaciones / deuda tecnica del esquema.

Los pendientes de BD (typos, renombrados, columnas faltantes) se registran en un archivo de pendientes con checkbox y fecha de confirmacion.

---

## 11. Lecciones Aprendidas en CAR_FIX (no repetir)

| Problema encontrado | Consecuencia | Regla del estandar |
|---|---|---|
| Typos en nombres: `EstadoFactura.Descipcion`, `HistoricoRespuesto.RepuestoDecripcion`, tabla `HojaInpeccionDetalle`, tabla `HistoricoRespuesto` | El typo se propago a entidades, DTOs y frontend; corregirlo es costoso | Revisar ortografia de cada objeto antes de crearlo (seccion 2) |
| Tildes en columnas: `Taller.UbicaciónGPS`, `Clientes.Dirección` | Propiedades C# con tilde, inconsistencia con la convencion | Sin tildes ni ñ en nombres de objetos (seccion 2) |
| `Vehiculos.Annio` creado como `tinyint` | No soportaba anios > 255; requirio migracion | `smallint`/`int` para anios (seccion 5.3) |
| `EstadoFactura.Descipcion` como `nchar(10)` | Textos truncados y rellenos con espacios | `varchar` con largo suficiente (seccion 5.3) |
| PK `PK_OrdenReparacion` en la tabla `OrdenServicio` tras renombrarla | Nombre de constraint engannoso | Renombrar constraints junto con la tabla (seccion 4.1) |
| `DEFAULT 'Marca del vehiculo'` en `MarcaModelo.Marca` | Valor basura si se omite la columna | Defaults solo con valores reales (seccion 5.4) |
| `DEFAULT 0` en una FK cuyo catalogo empieza en 1 (`HojaInspeccionDetalleEstadoID`) | Un INSERT que omite la columna falla por la FK | Sin defaults en FK salvo valor existente (seccion 5.2) |
| Datos semilla con typos y espacios al final (`suspeción`, `elétrico`, `'J. Sistema elétrico '`) | Datos sucios visibles al usuario | Revisar scripts de semilla (seccion 9) |
| Mezcla de tablas en plural y singular | Nombres impredecibles | Plural siempre (seccion 4.1) |
| `TipoReparacion` sin IDENTITY mientras todas las demas lo tienen | Excepcion que hay que recordar en el codigo | IDENTITY en todas, salvo catalogos de estados con IDs fijos (seccion 5.1) |
| Catalogos de estados con IDENTITY | Riesgo de IDs distintos entre ambientes | IDs explicitos en catalogos de estados (seccion 8.1) |
| Tabla `Taller` sin pantalla de mantenimiento | Se tuvo que insertar un registro a mano | Toda tabla de configuracion nace con su pantalla (seccion 8.5) |
| Columna de negocio requerida (`HistoricoRespuesto.Motor`) no creada | Funcionalidad incompleta | Validar el diseno contra la especificacion funcional antes de crear la tabla |
| Tablas y columnas agregadas despues (`Usuarios`, `Roles`, `TokenRefresco`, `Reparacion.DuracionAproximadaHoras`) quedaron sin `MS_Description` (22 columnas sin documentar; corregido el 2026-10-21 con `2026-10-21_agregar_descripciones_faltantes.sql`) | Documentacion incompleta en la BD | Las descripciones van en el mismo script de creacion (seccion 10.1) |
| Tablas creadas para una funcionalidad aun no disenada (Hoja de Inspeccion) | Esquema "huerfano" sin uso | Crear tablas cuando la especificacion este cerrada |

---

## 12. Plantilla de Script para una Tabla Nueva

```sql
-- =============================================================================
-- AAAA-MM-DD — Crear tabla Sistema.<Tabla>
-- Motivo: <por que se necesita / seccion de la especificacion>
-- =============================================================================

IF OBJECT_ID('Sistema.<Tabla>', 'U') IS NULL
BEGIN
    CREATE TABLE Sistema.<Tabla> (
        <Entidad>ID      INT IDENTITY(1,1) NOT NULL,
        <Padre>ID        INT               NOT NULL,
        Descripcion      VARCHAR(250)      NOT NULL,
        Monto            MONEY             NOT NULL CONSTRAINT DF_<Tabla>_Monto  DEFAULT ((0)),
        EsActivo         BIT               NOT NULL CONSTRAINT DF_<Tabla>_EsActivo DEFAULT ((1)),
        FechaCreacion    DATETIME          NOT NULL CONSTRAINT DF_<Tabla>_FechaCreacion DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_<Tabla> PRIMARY KEY (<Entidad>ID),
        CONSTRAINT FK_<Tabla>_<TablaPadre> FOREIGN KEY (<Padre>ID)
            REFERENCES Catalogo.<TablaPadre> (<Padre>ID)
            ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_<Tabla>_MontoNoNegativo CHECK (Monto >= 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_<Tabla>_<Padre>ID')
    CREATE INDEX IX_<Tabla>_<Padre>ID ON Sistema.<Tabla> (<Padre>ID);
GO

-- Documentacion (obligatoria: tabla + cada columna)
EXEC sys.sp_addextendedproperty N'MS_Description', N'<Proposito de negocio de la tabla>',
     N'SCHEMA', N'Sistema', N'TABLE', N'<Tabla>';
EXEC sys.sp_addextendedproperty N'MS_Description', N'Identificador único del registro',
     N'SCHEMA', N'Sistema', N'TABLE', N'<Tabla>', N'COLUMN', N'<Entidad>ID';
EXEC sys.sp_addextendedproperty N'MS_Description', N'FK a Catalogo.<TablaPadre>',
     N'SCHEMA', N'Sistema', N'TABLE', N'<Tabla>', N'COLUMN', N'<Padre>ID';
-- ... una por cada columna restante
GO
```

---

## 13. Checklist antes de Crear o Modificar una Tabla

- [ ] Nombres en espanol, PascalCase, sin tildes ni ñ, **ortografia revisada**
- [ ] Tabla en plural y en el esquema correcto (`Catalogo` / `Sistema`)
- [ ] PK `int IDENTITY` llamada `<Entidad>ID` (o ID fijo si es catalogo de estados)
- [ ] FKs con el mismo nombre que la PK referenciada, como constraint real, con indice `IX_`
- [ ] Politica ON DELETE / ON UPDATE elegida segun la seccion 7
- [ ] Tipos correctos: `money` para dinero, `bit` + default para booleanos, `smallint`/`int` para anios, sin `nchar`
- [ ] Nulabilidad coincide con la especificacion funcional
- [ ] Defaults y constraints con nombre explicito (`PK_`, `FK_`, `DF_`, `UQ_`, `IX_`, `CK_`)
- [ ] `UNIQUE` en llaves naturales de negocio
- [ ] `MS_Description` en la tabla y en **cada** columna (consulta de la seccion 10.1 devuelve 0 filas)
- [ ] Script versionado con fecha en `Backend/Scripts/sql/`, idempotente
- [ ] Datos semilla revisados (ortografia, sin espacios al final)
- [ ] Entidades regeneradas con `scaffold` y documentacion regenerada (`/sync-db-docs`)
- [ ] Script aplicado en todos los ambientes

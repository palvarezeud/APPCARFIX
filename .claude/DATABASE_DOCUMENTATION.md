# Documentación de Base de Datos — CAR_FIX

> **Servidor:** `localhost\SQL2022` | **Base de datos:** `CAR_FIX` | **Fecha de análisis:** 2026-07-18

Sistema de gestión de reparación de vehículos. Permite registrar clientes, sus vehículos, órdenes de reparación, reparaciones individuales, repuestos utilizados y la facturación correspondiente.

---

## Diagrama de Relaciones (ERD)

```
[Esquema Catalogo]                         [Esquema Sistema]

Clientes (1) ─────────────────── (N) Vehiculos
              FK CASCADE/CASCADE       │
                                       ├──→ (N) OrdenServicio ──── EstadoOrden (1)
                                       │          │  FK NO_ACTION/CASCADE
                                       │          └──→ (1) Facturas ──── EstadoFactura (1)
                                       │               FK NO_ACTION/NO_ACTION  FK NO_ACTION/CASCADE
                                       │
                              (N) Facturas
     FK_Facturas_Vehiculos:        │
     NO_ACTION/NO_ACTION           ├── (N) Reparacion
                                   │        FK CASCADE/CASCADE
                                   └── (N) Repuesto
                                            FK CASCADE/CASCADE

Roles (1) ──────────── (N) Usuarios        (autenticacion — sin FK a tablas de negocio)
           FK NO_ACTION/CASCADE

TipoReparacion     (independiente — catalogo de consulta, sin FK a Reparacion)
Taller             (independiente — configuración del sistema)
HistoricoRespuesto (independiente — historial de precios de repuestos por vehiculo)
MarcaModelo        (independiente — catalogo de Marca/Modelo/Annio, sin FK)
Parametros         (independiente — catalogo generico clave/valor, tabla nueva)
```

### Relaciones principales

| Tabla hija               | FK Column          | Tabla padre                | PK Column          | ON DELETE  | ON UPDATE  |
|--------------------------|--------------------|----------------------------|--------------------|------------|------------|
| `Sistema.Facturas`       | `EstadoFacturaID`  | `Catalogo.EstadoFactura`   | `EstadoFacturaID`  | NO ACTION  | CASCADE    |
| `Sistema.Facturas`       | `VehiculoID`       | `Catalogo.Vehiculos`       | `VehiculoID`       | NO ACTION  | NO ACTION  |
| `Sistema.OrdenServicio`  | `EstadoOrdenID`    | `Catalogo.EstadoOrden`     | `EstadoOrdenID`    | NO ACTION  | CASCADE    |
| `Sistema.OrdenServicio`  | `FacturaID`        | `Sistema.Facturas`         | `FacturaID`        | NO ACTION  | NO ACTION  |
| `Sistema.OrdenServicio`  | `VehiculoID`       | `Catalogo.Vehiculos`       | `VehiculoID`       | CASCADE    | CASCADE    |
| `Sistema.Reparacion`     | `FacturaID`        | `Sistema.Facturas`         | `FacturaID`        | CASCADE    | CASCADE    |
| `Sistema.Repuesto`       | `FacturaID`        | `Sistema.Facturas`         | `FacturaID`        | CASCADE    | CASCADE    |
| `Catalogo.Vehiculos`     | `ClienteID`        | `Catalogo.Clientes`        | `ClienteID`        | CASCADE    | CASCADE    |
| `Catalogo.Usuarios`      | `RolID`            | `Catalogo.Roles`           | `RolID`            | NO ACTION  | CASCADE    |

> Todas las relaciones tienen FK constraint enforced en la BD. No existen FK lógicas.
> Las tablas están distribuidas en dos esquemas: `Catalogo` (maestros y configuración) y `Sistema` (transaccionales).
> La relación `Reparacion → TipoReparacion` fue eliminada en la BD. `TipoReparacion` sigue usándose como catálogo de consulta desde la aplicación, pero sin FK constraint.

---

## Tablas

### `Catalogo.Clientes`

Almacena los clientes del taller, tanto personas naturales como empresas.

| Columna         | Tipo           | Nulo | PK | Identity  | Default | Descripción                          |
|-----------------|----------------|------|----|-----------|---------|--------------------------------------|
| `ClienteID`     | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único del cliente      |
| `NombreCliente` | `varchar(250)` | NO   |    |           |         | Nombre completo o razón social       |
| `Telefono1`     | `varchar(50)`  | NO   |    |           |         | Teléfono principal                   |
| `Telefono2`     | `varchar(50)`  | YES  |    |           |         | Teléfono secundario (opcional)       |
| `Email`         | `varchar(250)` | YES  |    |           |         | Correo electrónico                   |
| `Direccion`     | `varchar(MAX)` | YES  |    |           |         | Dirección física                     |
| `Localizacion`  | `geography`    | YES  |    |           |         | Coordenadas geográficas (GPS)        |
| `EsEmpresa`     | `bit`          | NO   |    |           | `((0))` | `1` = empresa, `0` = persona natural |

**Relaciones:** Un cliente puede tener múltiples vehículos (`Catalogo.Vehiculos.ClienteID` FK CASCADE/CASCADE).

---

### `Catalogo.Vehiculos`

Registro de vehículos asociados a los clientes.

| Columna              | Tipo           | Nulo | PK | Identity  | Descripción                              |
|----------------------|----------------|------|----|-----------|------------------------------------------|
| `VehiculoID`         | `int`          | NO   | ✓  | ✓ (1,1)  | Identificador único del vehículo         |
| `Placa`              | `varchar(50)`  | YES  |    |           | Número de placa                          |
| `Marca`              | `varchar(200)` | NO   |    |           | Marca del vehículo (Toyota, Ford, etc.)  |
| `Modelo`             | `varchar(200)` | YES  |    |           | Modelo específico                        |
| `VIN`                | `varchar(MAX)` | YES  |    |           | Número de identificación vehicular       |
| `Annio`              | `smallint`     | YES  |    |           | Año del vehículo                         |
| `Motor`              | `varchar(50)`  | YES  |    |           | Tipo/código de motor                     |
| `EsAutomatico`       | `bit`          | NO   |    |           | `1` = automático, `0` = manual           |
| `DetallesCarroceria` | `varchar(200)` | YES  |    |           | Descripción de la carrocería (opcional)  |
| `ClienteID`          | `int`          | NO   |    |           | FK → `Catalogo.Clientes.ClienteID`       |

**Relaciones:** FK a `Catalogo.Clientes` (CASCADE delete/update). Un vehículo puede tener múltiples `Sistema.OrdenServicio` y `Sistema.Facturas`.

---

### `Sistema.OrdenServicio`

Documento central del sistema. Registra el ingreso de un vehículo al taller para ser atendido.

| Columna           | Tipo           | Nulo | PK | Identity  | Default | Descripción                               |
|-------------------|----------------|------|----|-----------|---------|-------------------------------------------|
| `OrdenServicioID` | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único de la orden           |
| `VehiculoID`      | `int`          | NO   |    |           |         | FK → `Catalogo.Vehiculos.VehiculoID`      |
| `FechaIngreso`    | `datetime`     | NO   |    |           |         | Fecha y hora de ingreso del vehículo      |
| `FechaSalida`     | `datetime`     | NO   |    |           |         | Fecha y hora de salida estimada/real      |
| `ProblemaGeneral` | `varchar(MAX)` | NO   |    |           |         | Descripción del problema reportado        |
| `EstadoOrdenID`   | `int`          | NO   |    |           |         | FK → `Catalogo.EstadoOrden.EstadoOrdenID` |
| `EsGarantia`      | `bit`          | NO   |    |           | `((0))` | `1` = orden bajo garantía                |
| `FacturaID`       | `int`          | NO   |    |           |         | FK → `Sistema.Facturas.FacturaID`         |

**Relaciones:** FK a `Catalogo.Vehiculos` (CASCADE delete/update), a `Catalogo.EstadoOrden` (NO ACTION delete, CASCADE update) y a `Sistema.Facturas` (NO ACTION delete/update).

> **Observación:** `FacturaID` es NOT NULL, lo que exige que exista una `Factura` antes de crear la `OrdenServicio`. El orden de inserción debe crear la Factura primero y luego la Orden. La restricción PK se llama `PK_OrdenReparacion` — nombre heredado de un renombrado anterior de la tabla.

---

### `Sistema.Reparacion`

Detalle de cada reparación o trabajo realizado. Agrupa bajo una `Factura`.

| Columna                   | Tipo           | Nulo | PK | Identity  | Default | Descripción                                        |
|---------------------------|----------------|------|----|-----------|---------|-----------------------------------------------------|
| `ReparacionID`            | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único de la reparación               |
| `FacturaID`               | `int`          | NO   |    |           |         | FK → `Sistema.Facturas.FacturaID`                  |
| `Listo`                   | `bit`          | NO   |    |           | `((0))` | `1` = reparación completada, `0` = pendiente       |
| `DescripcionReparacion`   | `varchar(MAX)` | NO   |    |           |         | Descripción detallada del trabajo                  |
| `DuracionAproximadaHoras` | `int`          | YES  |    |           | `((1))` | Duración estimada en horas                         |
| `Costo`                   | `money`        | NO   |    |           | `((0))` | Costo de esta reparación específica                |

**Relaciones:** FK a `Sistema.Facturas` (CASCADE delete/update). Sin FK a `TipoReparacion` — la relación fue eliminada; `TipoReparacion` se usa como catálogo de consulta desde la aplicación.

---

### `Sistema.Repuesto`

Registro de repuestos o piezas utilizadas. Agrupa bajo una `Factura`.

| Columna          | Tipo           | Nulo | PK | Identity  | Default | Descripción                              |
|------------------|----------------|------|----|-----------|---------|-------------------------------------------|
| `RepuestoID`     | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único del repuesto         |
| `FacturaID`      | `int`          | NO   |    |           |         | FK → `Sistema.Facturas.FacturaID`        |
| `NombreRepuesto` | `varchar(200)` | NO   |    |           |         | Nombre del repuesto o pieza              |
| `Costo`          | `money`        | NO   |    |           |         | Costo del repuesto                       |
| `Fecha`          | `datetime`     | YES  |    |           |         | Fecha de adquisición o uso (opcional)     |
| `Repuestera`     | `varchar(200)` | YES  |    |           |         | Proveedor o repuestera donde se adquirió (opcional) |
| `Factura`        | `varchar(100)` | YES  |    |           |         | Número de factura del proveedor (opcional) |
| `Incluido`       | `bit`          | NO   |    |           | `((0))` | Indica si el mecánico ya colocó/utilizó el repuesto en el vehículo |

**Relaciones:** FK a `Sistema.Facturas` (CASCADE delete/update).

> **Observación:** La columna `Incluido` se implementó en la aplicación — se actualiza vía `PATCH /api/repuestos/{id}/incluido` desde la lista de Repuestos del detalle de la orden. Mismo propósito que `Sistema.Reparacion.Listo`: permite al mecánico marcar los repuestos en los que va trabajando. Ver `CLAUDE.md` sección 5.8.

---

### `Sistema.Facturas`

Documento de cobro al cliente. Hub central de reparaciones y repuestos.

| Columna              | Tipo           | Nulo | PK | Identity  | Default | Descripción                                       |
|----------------------|----------------|------|----|-----------|---------|---------------------------------------------------|
| `FacturaID`          | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único de la factura                 |
| `VehiculoID`         | `int`          | NO   |    |           |         | FK → `Catalogo.Vehiculos.VehiculoID`              |
| `Fecha`              | `datetime`     | NO   |    |           |         | Fecha de emisión de la factura                    |
| `NombreCliente`      | `varchar(200)` | NO   |    |           |         | Nombre del cliente (snapshot histórico)           |
| `DescripcionGeneral` | `varchar(MAX)` | NO   |    |           |         | Descripción general de los trabajos               |
| `TotalRepuestos`     | `money`        | NO   |    |           |         | Subtotal de repuestos                             |
| `TotalReparaciones`  | `money`        | NO   |    |           |         | Subtotal de reparaciones                          |
| `SubTotal`           | `money`        | NO   |    |           | `((0))` | Suma de reparaciones + repuestos antes de descuento/impuesto |
| `Total`              | `money`        | NO   |    |           |         | Total general                                     |
| `Descuento`          | `money`        | NO   |    |           |         | Monto de descuento aplicado                       |
| `Adelanto`           | `money`        | NO   |    |           | `((0))` | Monto de adelanto o depósito recibido             |
| `Pendiente`          | `money`        | NO   |    |           | `((0))` | Saldo pendiente de cobro (Total - Adelanto)        |
| `ImpuestoVentas`     | `money`        | NO   |    |           | `((0))` | Impuesto de ventas aplicado                       |
| `EstadoFacturaID`    | `int`          | NO   |    |           |         | FK → `Catalogo.EstadoFactura.EstadoFacturaID`     |

**Relaciones:** FK a `Catalogo.Vehiculos` (NO ACTION delete/update) y a `Catalogo.EstadoFactura` (NO ACTION delete, CASCADE update). Tiene N `Sistema.Reparacion` y N `Sistema.Repuesto`. `Sistema.OrdenServicio.FacturaID` la referencia con FK enforced.

> **Observación:** La tabla no tiene `ClienteID` ni `OrdenServicioID`. El vínculo con el cliente es indirecto vía `VehiculoID → Catalogo.Vehiculos.ClienteID`. No existe FK directa hacia `OrdenServicio`; la trazabilidad va en sentido contrario (`OrdenServicio.FacturaID`). `NombreCliente` es un snapshot desnormalizado del nombre del cliente al momento de la factura.
>
> **`SubTotal` y `Pendiente`:** columnas calculadas por la aplicación (`RecalculadorTotalesFactura` en `CarFix.Aplicacion/Comun/`), no editables por el usuario. `SubTotal = TotalRepuestos + TotalReparaciones - Descuento`; `ImpuestoVentas` (IVA) se calcula como `SubTotal * tasa` leyendo la tasa desde `Catalogo.Parametros` (fila `ImpuestoVentas`); `Total = SubTotal + ImpuestoVentas`; `Pendiente = Total - Adelanto`. Ver `CLAUDE.md` sección 5.6.

---

### `Catalogo.Taller`

Datos del taller mecánico (nombre, dirección, teléfonos, email). Su contenido es la fuente del encabezado de la factura.

| Columna                | Tipo           | Nulo | PK | Identity  | Descripción                         |
|------------------------|----------------|------|----|-----------|-------------------------------------|
| `TallerID`             | `int`          | NO   | ✓  | ✓ (1,1)  | Identificador (tabla mono-registro) |
| `Nombre`               | `varchar(MAX)` | NO   |    |           | Nombre del taller                   |
| `UbicacionDescripcion` | `varchar(MAX)` | NO   |    |           | Dirección / descripción física      |
| `Telefonos`            | `varchar(200)` | NO   |    |           | Teléfonos de contacto               |
| `Email`                | `varchar(200)` | NO   |    |           | Correo electrónico del taller       |
| `UbicaciónGPS`         | `geography`    | YES  |    |           | Coordenadas geográficas (GPS)       |

**Relaciones:** Ninguna. Tabla independiente sin FKs entrantes ni salientes.

> **Observaciones:** Al ser tabla de configuración global (mono-registro), el `TallerID` Identity no aporta valor; la lógica de negocio debe asegurar una sola fila. La columna `UbicaciónGPS` contiene tilde — inconsistente con la convención del proyecto; considerar renombrar a `UbicacionGPS`.

---

### `Catalogo.TipoReparacion`

Catálogo de tipos de reparación con costos base y duración estimada.

| Columna                   | Tipo           | Nulo | PK | Identity | Descripción                          |
|---------------------------|----------------|------|----|----------|--------------------------------------|
| `TipoReparacionID`        | `int`          | NO   | ✓  |          | Identificador del tipo de reparación |
| `DescripcionReparacion`   | `varchar(MAX)` | NO   |    |          | Descripción del tipo de trabajo      |
| `DuracionAproximadaHoras` | `int`          | NO   |    |          | Duración estimada en horas           |
| `CostoBase`               | `money`        | NO   |    |          | Costo referencial base del servicio  |

**Relaciones:** Ninguna FK constraint hacia esta tabla. `TipoReparacion` se usa como catálogo de consulta desde la aplicación (búsqueda para pre-rellenar reparaciones), pero sin FK enforced en BD desde que se eliminó la relación con `Sistema.Reparacion`.

> **Observación:** A diferencia de todas las demás tablas, `TipoReparacionID` no tiene columna IDENTITY — el ID debe asignarse manualmente al insertar registros.

---

### `Catalogo.HistoricoRespuesto`

Historial de repuestos comprados, con precio y proveedor, usado como referencia de precios por vehículo.

| Columna               | Tipo           | Nulo | PK | Identity  | Descripción                                   |
|-----------------------|----------------|------|----|-----------|-----------------------------------------------|
| `RespuestoHistoricoID`| `int`          | NO   | ✓  | ✓ (1,1)  | Identificador único del registro              |
| `Marca`               | `varchar(200)` | NO   |    |           | Marca del vehículo asociado al repuesto       |
| `Modelo`              | `varchar(200)` | NO   |    |           | Modelo del vehículo asociado al repuesto      |
| `Annio`               | `int`          | NO   |    |           | Año del vehículo (usa `int`, correcto)        |
| `RepuestoDecripcion`  | `varchar(MAX)` | NO   |    |           | Descripción del repuesto                      |
| `Precio`              | `money`        | NO   |    |           | Precio pagado                                 |
| `Repuestera`          | `varchar(MAX)` | NO   |    |           | Proveedor donde se compró                     |
| `FechaCompra`         | `datetime`     | NO   |    |           | Fecha de la compra                            |

**Relaciones:** Ninguna. Tabla independiente sin FKs entrantes ni salientes. No referencia a `Catalogo.Vehiculos` — el vehículo se almacena desnormalizado como texto.

> **Observaciones:** Typo en `RepuestoDecripcion` (falta la `s`; debería ser `RepuestoDescripcion`). El vehículo se almacena como Marca/Modelo/Annio en texto en lugar de una FK a `Vehiculos` — esto permite registros huérfanos si el vehículo es eliminado o nunca existió en el sistema.

---

### `Catalogo.MarcaModelo`

Catálogo de combinaciones Marca/Modelo/Año. Poblada con 752 registros: 47 combinaciones Marca/Modelo de vehículos comunes (Toyota, Hyundai, Honda, Nissan, Kia, Suzuki, Chevrolet, Ford, Mazda, Mitsubishi, Volkswagen, Isuzu, Mercedes-Benz, BMW, Subaru, Renault) x años de fabricación 2010-2025.

| Columna         | Tipo           | Nulo | PK | Identity | Default                   | Descripción                        |
|-----------------|----------------|------|----|----------|----------------------------|-------------------------------------|
| `MarcaModeloID` | `int`          | NO   | ✓  | ✓ (1,1)  |                            | Identificador único del registro    |
| `Marca`         | `varchar(200)` | YES  |    |          | `('Marca del vehiculo')`  | Marca del vehículo                  |
| `Modelo`        | `varchar(200)` | YES  |    |          |                            | Modelo del vehículo                 |
| `Annio`         | `int`          | YES  |    |          |                            | Año del vehículo                    |

**Relaciones:** Ninguna. Tabla independiente sin FKs entrantes ni salientes. No referencia a `Catalogo.Vehiculos`.

> **Observaciones:** Ya está ubicada en el esquema `Catalogo`, siguiendo la convención del resto del modelo. Todas sus columnas son nullable, lo cual es inusual para un catálogo (`Marca`, `Modelo` y `Annio` deberían ser probablemente `NOT NULL`). El default `'Marca del vehiculo'` en la columna `Marca` parece un valor de placeholder/documentación dejado por error en lugar de un default real. La tabla no se referencia desde ninguna otra tabla ni está documentada en las pantallas funcionales (sección 5 de `CLAUDE.md`) — su propósito debe confirmarse con el equipo.

---

### `Catalogo.EstadoOrden`

Catálogo de estados posibles de una orden de servicio.

| Columna         | Tipo           | Nulo | PK | Identity  | Descripción                   |
|-----------------|----------------|------|----|-----------|-------------------------------|
| `EstadoOrdenID` | `int`          | NO   | ✓  | ✓ (1,1)  | Identificador del estado      |
| `Descripcion`   | `varchar(150)` | NO   |    |           | Nombre/descripción del estado |

**Valores actuales:**

| ID | Descripcion   |
|----|---------------|
| 1  | Cotización    |
| 2  | Recibido      |
| 3  | Reparando     |
| 4  | Finalizado    |
| 5  | Entregado     |

---

### `Catalogo.EstadoFactura`

Catálogo de estados posibles de una factura.

| Columna           | Tipo        | Nulo | PK | Identity  | Descripción              |
|-------------------|-------------|------|----|-----------|--------------------------|
| `EstadoFacturaID` | `int`       | NO   | ✓  | ✓ (1,1)  | Identificador del estado |
| `Descipcion`      | `nchar(25)` | NO   |    |           | Descripción del estado   |

**Valores actuales:**

| ID | Descipcion |
|----|------------|
| 1  | Cotización |
| 2  | Pendiente  |
| 3  | Pagada     |

> **Observaciones:** Typo en `Descipcion` (falta la `r`). Aunque `nchar(25)` permite más texto que antes, sigue siendo limitado para descripciones como "Pendiente de pago".

---

### `Catalogo.Roles`

Catálogo de roles del sistema para control de acceso.

| Columna       | Tipo           | Nulo | PK | Identity  | Descripción                 |
|---------------|----------------|------|----|-----------|----------------------------|
| `RolID`       | `int`          | NO   | ✓  | ✓ (1,1)  | Identificador único del rol |
| `Nombre`      | `varchar(50)`  | NO   |    |           | Nombre del rol              |
| `Descripcion` | `varchar(200)` | YES  |    |           | Descripción del rol         |

**Valores actuales:**

| ID | Nombre        | Descripcion                                      |
|----|---------------|--------------------------------------------------|
| 1  | Administrador | Acceso total al sistema                          |
| 2  | JefeMecanicos | Gestion de ordenes, facturas y catalogo          |
| 3  | Mecanico      | Operacion diaria de ordenes y reparaciones       |

**Relaciones:** `Catalogo.Usuarios.RolID` referencia esta tabla (NO ACTION delete, CASCADE update).

---

### `Catalogo.Usuarios`

Usuarios del sistema con credenciales de acceso y rol asignado.

| Columna          | Tipo           | Nulo | PK | Identity  | Default | Descripción                             |
|------------------|----------------|------|----|-----------|---------|------------------------------------------|
| `UsuarioID`      | `int`          | NO   | ✓  | ✓ (1,1)  |         | Identificador único del usuario          |
| `NombreUsuario`  | `varchar(100)` | NO   |    |           |         | Login único en el sistema                |
| `PasswordHash`   | `varchar(500)` | NO   |    |           |         | Hash BCrypt de la contraseña             |
| `NombreCompleto` | `varchar(250)` | NO   |    |           |         | Nombre para mostrar en la interfaz       |
| `Email`          | `varchar(250)` | YES  |    |           |         | Correo electrónico del usuario           |
| `Activo`         | `bit`          | NO   |    |           | `((1))` | `1` = activo, `0` = deshabilitado        |
| `RolID`          | `int`          | NO   |    |           |         | FK → `Catalogo.Roles.RolID`              |

**Relaciones:** FK a `Catalogo.Roles` (NO ACTION delete, CASCADE update). Sin relaciones directas a tablas de negocio — el vínculo usuario-acción se gestiona en la capa de aplicación.

> **Observación:** `PasswordHash` almacena el hash generado por BCrypt (incluye salt embebido). Nunca almacenar contraseñas en texto plano. Usar `IServicioContrasenna.Hashear()` al crear o cambiar contraseñas.

---

### `Catalogo.Parametros`

Catálogo genérico clave/valor para parámetros de configuración del sistema (tasa de IVA, horario laboral del taller, etc.).

| Columna       | Tipo           | Nulo | PK | Identity  | Descripción                          |
|---------------|----------------|------|----|-----------|----------------------------------------|
| `ParametroID` | `int`          | NO   | ✓  | ✓ (1,1)  | Identificador único del parámetro     |
| `Nombre`      | `varchar(200)` | NO   |    |           | Nombre/clave del parámetro            |
| `Valor`       | `varchar(MAX)` | NO   |    |           | Valor del parámetro (texto libre)     |

**Valores actuales:**

| ID | Nombre         | Valor | Uso                                                                 |
|----|----------------|-------|----------------------------------------------------------------------|
| 1  | ImpuestoVentas | 13%   | Tasa de IVA calculada sobre `SubTotal` de Facturas (ver sección 5.6 de `CLAUDE.md`) |
| 5  | HoraApertura   | 8:00  | Hora de apertura del taller; usada para calcular `FechaSalida` (ver sección 5.5) |
| 6  | HoraCierre     | 18:00 | Hora de cierre del taller; usada para calcular `FechaSalida` (ver sección 5.5) |

**Relaciones:** Ninguna. Tabla independiente sin FKs entrantes ni salientes.

> **Observación:** Ya alimenta el cálculo de IVA (Facturas) y de `FechaSalida` (Órdenes de Servicio) desde el backend; falta la pantalla "Configuración del sistema" (ver `CLAUDE.md` sección 1, marcada como pendiente) para administrarla desde la UI en vez de por SQL directo. Estructura genérica de texto libre — no hay validación de tipo por parámetro a nivel de BD (ej. `HoraApertura >= HoraCierre` solo se valida en la aplicación al usarlo, no al guardarlo).

---

## Flujo de Negocio

```
1. REGISTRO
   Cliente  →  Vehículo

2. INGRESO AL TALLER
   Vehículo  →  OrdenReparacion (FechaIngreso, ProblemaGeneral, EstadoOrden)

3. EJECUCIÓN DE TRABAJOS
   OrdenReparacion  →  Reparacion  (qué se hizo, cuánto costó)
   OrdenReparacion  →  Repuesto    (qué piezas se usaron, de dónde, a qué costo)

4. FACTURACIÓN
   Cliente + Vehículo  →  Factura  (totales de reparaciones + repuestos - descuento)

5. ESTADOS
   OrdenReparacion.EstadoOrdenID   →  EstadoOrden
   Factura.EstadoFacturaID         →  EstadoFactura
```

---

## Observaciones y Oportunidades de Mejora

| # | Área | Descripción |
|---|------|-------------|
| 1 | **FK faltantes en `Facturas`** | Ninguna columna de `Facturas` tiene FK constraint: `ClienteID`, `VehiculoID`, `EstadoFacturaID`, `RepuestoID` y `ReparacionID` son todas referencias lógicas. El motor no garantiza integridad referencial. |
| 2 | **FK faltante en `Reparacion`** | `Reparacion.TipoReparacionID` no tiene FK constraint hacia `TipoReparacion`. |
| 3 | **Vínculo Factura ↔ Orden** | No existe FK ni columna directa entre `Facturas` y `OrdenReparacion`. La trazabilidad orden→factura requiere join por `VehiculoID`+`ClienteID`. |
| 4 | **Dato desnormalizado** | `Facturas.NombreCliente` duplica `Clientes.NombreCliente`. Puede ser intencional como snapshot histórico; si no, eliminar. |
| 5 | **Typo en columna** | `EstadoFactura.Descipcion` debería ser `Descripcion`. |
| 6 | **`EstadoFactura.Descipcion` demasiado corto** | `nchar(10)` no alcanza para descripciones como "Pendiente de pago". |
| 7 | **Naming inconsistente** | `TipoReparacion.CatalogoCostoID` debería llamarse `TipoReparacionID` para seguir la convención del resto del esquema. |
| 8 | **Tipo de dato `Annio`** | `tinyint` solo soporta 0–255. Para años de vehículos (ej. 1990–2030) se requiere `smallint` o `int`. |
| 9 | **Sin cantidad en `Repuesto`** | La tabla no tiene campo de cantidad. Si se usan múltiples unidades del mismo repuesto se requiere una fila por unidad o agregar columna `Cantidad`. |
| 10 | **`Taller` mono-registro con Identity** | Al ser una tabla de configuración global, el PK identity no aporta valor. La lógica de negocio debe asegurar que nunca exista más de una fila. |
| 11 | **`sysdiagrams` en tablas de usuario** | SQL Server creó esta tabla al usar el diseñador de diagramas. No es parte del esquema de la aplicación; puede ignorarse. |

---

## Resumen de Tablas

| Tabla             | Tipo           | Identity |
|-------------------|----------------|----------|
| `Clientes`        | Transaccional  | ✓        |
| `Vehiculos`       | Transaccional  | ✓        |
| `OrdenReparacion` | Transaccional  | ✓        |
| `Reparacion`      | Transaccional  | ✓        |
| `Repuesto`        | Transaccional  | ✓        |
| `Facturas`        | Transaccional  | ✓        |
| `Taller`          | Configuración  | ✓        |
| `TipoReparacion`      | Catálogo       | ✗        |
| `HistoricoRespuesto`  | Catálogo       | ✓        |
| `EstadoOrden`         | Catálogo       | ✓        |
| `EstadoFactura`       | Catálogo       | ✓        |
| `Roles`               | Seguridad      | ✓        |
| `Usuarios`            | Seguridad      | ✓        |

---

*Documentación generada automáticamente mediante análisis del esquema SQL Server — CAR_FIX*

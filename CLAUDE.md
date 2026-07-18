# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# CAR_FIX — Sistema de Reparación de Vehículos

## 0. Convencion de Nomenclatura (aplica a TODO el codigo)

**Todo el codigo se escribe en español, sin caracteres especiales.**
- Sin tildes: `numero` no `número`, `pagina` no `página`
- Sin ñ: reemplazar siguiendo el patron del esquema de BD — `annio` en lugar de `año`
- Sufijos tecnicos reconocidos pueden quedar en ingles: `Repository`, `Handler`, `Validator`, `Command`, `Query`, `Dto`, `Configuration`, `Exception`

## 1. Estado del Proyecto

El repositorio está en **fase de desarrollo activo**. La base de datos está creada y poblada con datos de prueba. Los pendientes de BD y de desarrollo están en [`PENDIENTES.md`](.claude/PENDIENTES.md).

### Pantallas implementadas

| Pantalla | Backend | Frontend | Notas |
|---|---|---|---|
| Login / Autenticacion | ✓ | ✓ | JWT con roles; interceptor cierra sesion automaticamente en 401 |
| Clientes | ✓ | ✓ | CRUD completo + modos ver/crear/editar |
| Vehiculos | ✓ | ✓ | CRUD completo + modos ver/crear/editar |
| Ordenes de Servicio | ✓ | ✓ | CRUD + modos ver/crear/editar/estado |
| Facturas | ✓ | ✓ | CRUD + modos ver/crear/editar/estado + tabs Reparaciones/Repuestos |
| Catalogo Reparaciones | ✓ | ✓ | CRUD completo + modos ver/crear/editar |
| Catalogo Repuestos (Historico) | ✓ | ✓ | CRUD completo + modos ver/crear/editar |
| Usuarios | ✓ | ✓ | CRUD completo |
| Configuracion del Sistema | ✗ | ✗ | Pendiente |

## 2. Descripción del Proyecto

Aplicación web para administrar un pequeño taller mecánico. Flujo principal:

1. **Registro** — El mecánico valida si el cliente existe; si no, captura nombre, teléfono y email. Para el vehículo, fotografía el permiso de circulación y la IA extrae marca, año, modelo y VIN automáticamente.
2. **Orden de servicio** — Se crea asociada al cliente y vehículo. Estado inicial: **Recibido**, total: **$0**. El cliente describe el motivo de ingreso.
3. **Diagnóstico** — El mecánico asocia reparaciones del catálogo a la orden desde su celular. Cada reparación tiene precio base ajustable y suma al total.
4. **Repuestos** — El mecánico escanea una factura de repuesto y la IA extrae fecha, lugar de compra, costo y descripción. Cada repuesto suma al total de la orden.
5. **Cotización** — Se genera un PDF con el resumen de la orden y se envía al correo del cliente.
6. **Aprobación** — El cliente aprueba verbalmente; el mecánico cambia el estado a **Reparando**.
7. **Cierre** — Estado cambia a **Finalizado**. Se genera factura con reparaciones y repuestos; el mecánico puede ajustar montos antes de imprimir o enviar como PDF.

## 3. Opciones de Menú

| Opción de Menú              | Descripción / Acción                                                      |
|-----------------------------|---------------------------------------------------------------------------|
| **Clientes**                | Mantenimiento de clientes (crear, editar, buscar)                         |
| **Vehículos**               | Mantenimiento de vehículos asociados a clientes                           |
| **Órdenes de servicio**     | Lista de órdenes; crear, modificar y cambiar estado                       |
| **Facturas**                | Lista de facturas; crear y cambiar estado                                 |
| **Repuestos**               | Asociar un repuesto a una factura                                         |
| **Reparaciones**            | Asociar una reparación a una factura                                      |
| **Catálogo de reparaciones**| Catálogo de tipos de reparación con costos base *(Jefe / Admin)*          |
| **Catálogo de repuestos**   | Catálogo genérico de repuestos con precios de referencia *(Jefe / Admin)* |
| **Usuarios y perfiles**     | Administración de usuarios y roles del sistema *(Jefe / Admin)*           |
| **Configuración del sistema**| Nombre del taller, teléfonos, correo electrónico *(Solo Admin)*          |

## 4. Perfiles del Sistema

**Administrador** — acceso total a todas las opciones del menú.

**Jefe de Mecánicos** — puede crear y modificar órdenes de servicio y facturas; registrar repuestos y reparaciones en facturas; cambiar estados de órdenes y facturas; administrar el catálogo de reparaciones, catálogo de repuestos y usuarios.

**Mecánico** — puede acceder a órdenes de servicio, crear facturas, registrar repuestos y reparaciones, y cambiar estados de órdenes y facturas.

### Matriz de Permisos por Opción de Menú

| Opción de Menú               | Descripción / Acción                                                   | Administrador | Jefe de Mecánicos | Mecánico |
|------------------------------|------------------------------------------------------------------------|:---:|:---:|:---:|
| Clientes                     | Abre el mantenimiento de clientes.                                     | ✓   | ✓   | ✓   |
| Vehículos                    | Abre el mantenimiento de vehículos.                                    | ✓   | ✓   | ✓   |
| Órdenes de servicio          | Abre la lista, crea/modifica órdenes y cambia su estado.               | ✓   | ✓   | ✓   |
| Facturas                     | Abre la lista, crea facturas y cambia su estado.                       | ✓   | ✓   | ✓   |
| Repuestos                    | Asocia un repuesto a una factura.                                      | ✓   | ✓   | ✓   |
| Reparaciones                 | Asocia una reparación a una factura.                                   | ✓   | ✓   | ✓   |
| Catálogo de reparaciones     | Accede al catálogo y registra costos/precios base.                     | ✓   | ✓   |     |
| Catálogo de repuestos        | Accede al catálogo genérico con costos base.                           | ✓   | ✓   |     |
| Usuarios y perfiles          | Administra los usuarios y sus perfiles en el sistema.                  | ✓   | ✓   |     |
| Configuración del sistema    | Administra parámetros (nombre del taller, teléfonos, correo).          | ✓   |     |     |

> Los roles se almacenan en `Catalogo.Roles` (valores: `Administrador`, `JefeMecanicos`, `Mecanico`) y los usuarios en `Catalogo.Usuarios` con hash BCrypt. La autenticación usa JWT generado desde el backend.

## 5. Especificaciones Funcionales

### 5.1 Estados de las Órdenes de Servicio

Tabla `Catalogo.EstadoOrden`. Los estados posibles son:

| ID | Estado        |
|----|---------------|
| 1  | Cotización    |
| 2  | Recibido      |
| 3  | Reparando     |
| 4  | Finalizado    |
| 5  | Entregado     |

### 5.2 Estados de las Facturas

Tabla `Catalogo.EstadoFactura`. Los estados posibles son:

| ID | Estado     |
|----|------------|
| 1  | Cotización |
| 2  | Pendiente  |
| 3  | Pagada     |


### 5.3 Mantenimiento de Clientes

Pantalla donde mecánicos y jefe de mecánicos gestionan clientes nuevos y existentes.

**Campos** (según tabla `Catalogo.Clientes`):

| Campo          | Obligatorio | Notas                                                                                     |
|----------------|:-----------:|-------------------------------------------------------------------------------------------|
| NombreCliente  | ✓           |                                                                                           |
| Telefono1      | ✓           |                                                                                           |
| Telefono2      |             |                                                                                           |
| Email          |             | Sin email no se pueden enviar facturas/proformas; solo impresión física o PDF por WhatsApp |
| Direccion      |             |                                                                                           |
| Localizacion   |             |                                                                                           |
| EsEmpresa      | ✓           | Default: `false`                                                                          |

**Reglas de negocio:**
- Un cliente puede tener múltiples vehículos asociados.
- **No se puede eliminar** un cliente que ya tenga facturas asociadas.
- Si no tiene facturas, se puede eliminar y se eliminan también sus vehículos asociados (CASCADE).
- La acción de eliminar la pueden realizar el **Jefe de Mecánicos** y el **Mecánico**.

**Pantalla — requisitos de UI:**
- Botones: Agregar, Modificar, Eliminar.
- Lista de clientes con filtro por nombre.
- Paginación activada cuando la lista supera 25 registros.
- **En celular** (ancho ≤768px): el listado solo muestra las columnas `#`, `NombreCliente` y `Telefono1` (Email y Tipo se ocultan). En vez de paginación, carga 15 registros iniciales y agrega 15 más automáticamente al hacer scroll hacia el final de la lista (scroll infinito).

---

### 5.4 Vehículos

Pantalla donde todos los perfiles gestionan vehículos asociados a clientes. También es el punto de partida para crear una orden de servicio.

**Campos** (según tabla `Catalogo.Vehiculos`):

| Campo              | Obligatorio | Notas                                                      |
|--------------------|:-----------:|------------------------------------------------------------|
| ClienteID          | ✓           | FK al cliente propietario del vehículo                     |
| Placa              |             | Número de placa del vehículo                               |
| Marca              | ✓           |                                                            |
| Modelo             | ✓           |                                                            |
| VIN                |             | Identificador de carrocería                                |
| Annio              | ✓           | Año de fabricación                                         |
| Motor              |             | Características del motor                                  |
| EsAutomatico       | ✓           | Default: `false` (manual)                                  |
| DetallesCarroceria | ✓           | Desperfectos, rayones o golpes observados al ingreso       |

**Reglas de negocio:**
- Un vehículo siempre debe estar asociado a un cliente.
- **No se puede eliminar** un vehículo que tenga órdenes de servicio asociadas.
- Desde esta pantalla se puede iniciar la creación de una orden de servicio para el vehículo seleccionado.

**Pantalla — requisitos de UI:**
- Botones: Agregar, Modificar, Eliminar, Generar Orden de Servicio.
- Lista con filtro por placa o nombre del cliente.
- Paginación activada cuando la lista supera 25 registros.
- Al seleccionar, mostrar detalle de campos con botones Aceptar y Cancelar.
- Los campos `Marca` y `Modelo` del formulario tienen autocompletado (`<datalist>`) basado en el catálogo `Catalogo.MarcaModelo`; `Modelo` solo sugiere valores una vez que `Marca` coincide con una marca conocida del catálogo. El campo `Annio` es una caja de texto (no un combo) que sugiere únicamente los años disponibles para la combinación `Marca`+`Modelo` ingresada. Estas sugerencias no restringen el valor final: `Marca`/`Modelo`/`Annio` siguen siendo de texto libre.
- **En celular** (ancho ≤768px): el listado solo muestra las columnas `#`, `Placa`, `Marca` y `NombreCliente` (Modelo, Año y Transmisión se ocultan). En vez de paginación, carga 15 registros iniciales y agrega 15 más automáticamente al hacer scroll hacia el final de la lista (scroll infinito).

**Característica especial — escaneo IA (tarjeta de circulación):**
- El mecánico fotografía la tarjeta de circulación del vehículo.
- La IA extrae automáticamente: `Marca`, `Modelo`, `Annio`, `VIN`, `Placa` y `Motor`.
- Los datos pre-rellenan el formulario; el mecánico valida y corrige antes de guardar.
- Disponible solo en modo **Crear** (no en Editar ni en modo Ver).
- Implementación: backend llama a la API de Claude (Anthropic) — modelo `claude-sonnet-5` por defecto (configurable vía `Anthropic:Modelo`), enviando la foto como imagen y forzando salida JSON estructurada (`output_config.format`). La foto **no se persiste**; se usa solo de forma transitoria para la extracción. Requiere una API key de Anthropic (`Anthropic:ApiKey`, vía `dotnet user-secrets` en desarrollo o variable de entorno `Anthropic__ApiKey` en producción) — **facturación separada por token, no cubierta por una suscripción Claude Pro**.

---

### 5.5 Órdenes de Servicio

Pantalla con el listado de órdenes de servicio activas (vehículos en el taller en proceso de reparación o cotización).

**Campos** (según tabla `Sistema.OrdenServicio`):

| Campo           | Obligatorio | Notas                                                       |
|-----------------|:-----------:|-------------------------------------------------------------|
| VehiculoID      | ✓           | FK al vehículo ingresado                                    |
| FechaIngreso    | ✓           | Fecha y hora de ingreso al taller                           |
| FechaSalida     | ✓           | Fecha y hora estimada o real de salida                      |
| ProblemaGeneral | ✓           | Descripción del problema indicada por el dueño del vehículo |
| EstadoOrdenID   | ✓           | Estado actual; ver sección 5.1                              |
| EsGarantia      | ✓           | Default: `false`; indica que la reparación es bajo garantía |
| FacturaID       | ✓           | FK a la factura asociada a esta orden                       |

**Reglas de negocio:**
- Estado inicial al crear: **Cotización** (ID 1).
- Al crear una orden se crea **atomicamente** una factura en estado Cotizacion (un solo commit via patron Unidad de Trabajo).
- Al eliminar una orden, si tiene factura en estado Cotizacion o Pendiente, se elimina tambien la factura atomicamente. Si la factura esta en estado **Pagada**, no se puede eliminar la orden.
- **Ordenes en estado Finalizado (4) o Entregado (5) no se pueden eliminar ni modificar** (el backend valida y retorna error).
- **Calculo automatico de FechaSalida:** cada vez que se agrega, modifica o elimina una reparacion asociada a la factura de la orden, el backend recalcula `FechaSalida` simulando el avance del reloj real dentro del horario laboral del taller — parametros `HoraApertura` y `HoraCierre` en `Catalogo.Parametros` (valores actuales: `8:00` y `18:00`, formato `H:mm`/`HH:mm`) — sumando las horas de las reparaciones con duracion conocida y saltando por completo los **sabados y domingos** (el taller no labora esos dias). Si la `FechaIngreso` cae fuera de ese horario (antes de abrir, despues de cerrar, o en fin de semana), el conteo arranca en el siguiente momento laboral valido. Si ninguna reparacion tiene horas registradas, `FechaSalida` no se modifica. Si `HoraApertura`/`HoraCierre` faltan, no se pueden interpretar, o `HoraApertura >= HoraCierre`, la operacion falla explicitamente (`422 Regla de negocio violada`) en vez de calcular con un horario invalido. El calculo es un estimado; el campo sigue siendo editable manualmente en la orden y se sobrescribe en el siguiente cambio a las reparaciones. Implementacion: `CalculadoraFechaSalida` y `RecalculadorFechaSalidaFactura` en `CarFix.Aplicacion/Comun/`.

**Pantalla — requisitos de UI:**
- Botones, en este orden: **Agregar, Modificar, Eliminar**.
- Filtros: número de orden (`OrdenServicioID`) o placa del vehículo, y por estado (pastillas: Todos, Cotización, Recibido, Reparando, Finalizado, Entregado).
- Paginación activada cuando la lista supera 15 registros; los controles de paginación se muestran centrados.
- Para ver el detalle: un clic sobre la fila la selecciona/marca (habilita Modificar/Eliminar); **doble clic** sobre la fila abre el detalle directamente.
- Al seleccionar, mostrar detalle de campos con botones Aceptar y Cancelar.
- El detalle muestra unicamente las listas de **Reparaciones** (Descripcion + Listo) y **Repuestos** (Repuesto + Incluido); no se muestran los datos de la factura asociada.
- **En celular** (ancho ≤768px): el listado solo muestra las columnas `#`, `Placa`, `Vehículo` y `Cliente` (Ingreso, Salida, Estado, Garantía y Factura se ocultan). Los filtros de estado se acomodan en filas de 3 pastillas para no desbordar el ancho de pantalla.
- Las listas de reparaciones y repuestos muestran fondo amarillo cuando estan vacias.

**Comportamiento del formulario segun estado de la orden:**

| Estado | Campos | Selector de Estado | Checkboxes Reparaciones | Botones |
|---|---|---|---|---|
| `ver` (cualquier estado) | Solo lectura (CSS) | Solo lectura | Deshabilitados | Solo **Cerrar** |
| `editar`, estado 1-3 | Editables | Editable | Interactivos | Cancelar + **Aceptar** |
| `editar`, estado **Finalizado (4)** | Deshabilitados (`[disabled]`) | **Editable** (solo para pasar a Entregado) | Deshabilitados | Cancelar + **Aceptar** (llama `PATCH /estado`, no al update general) |
| `editar`, estado **Entregado (5)** | Solo lectura (CSS) | Solo lectura | Deshabilitados | Solo **Cerrar** |

**Comportamiento del campo Listo en reparaciones:**
- El campo `Listo` se muestra como checkbox interactivo (no texto estatico).
- Al marcar el ultimo check (todas las reparaciones listas), el sistema pregunta si desea pasar la orden a **Finalizado**.
  - Si acepta: llama `PATCH /api/ordenes/{id}/estado` con `nuevoEstadoId: 4`, recarga la lista y actualiza el badge.
  - Si cancela: no se modifica el estado; los checks quedan marcados.
- Los checkboxes se deshabilitan cuando la orden esta en estado Finalizado (4) o Entregado (5), o cuando el formulario esta en modo `ver`.

**Comportamiento del campo Incluido en repuestos:**
- Igual proposito que `Listo` en reparaciones: le permite al mecanico ir marcando, desde el detalle de la orden, los repuestos que ya fue colocando/utilizando en el vehiculo a medida que avanza el trabajo.
- Se muestra como checkbox interactivo en la lista de repuestos del detalle de la orden (ver seccion 5.8).
- Se actualiza via `PATCH /api/repuestos/{id}/incluido` de forma independiente (no requiere editar el repuesto completo); el backend rechaza el cambio si la factura asociada esta en estado **Pagada**.
- A diferencia de `Listo`, marcar todos los repuestos como `Incluido` **no** dispara ningun cambio automatico de estado de la orden.
- Los checkboxes se deshabilitan bajo las mismas condiciones que `Listo`: orden en estado Finalizado (4) o Entregado (5), o formulario en modo `ver`.

---

### 5.6 Facturas

Pantalla con el listado de facturas activas y proformas (cotizaciones pendientes de aprobación).

**Campos** (según tabla `Sistema.Facturas`):

| Campo              | Obligatorio | Notas                                                    |
|--------------------|:-----------:|----------------------------------------------------------|
| VehiculoID         | ✓           | FK al vehículo                                           |
| Fecha              | ✓           | Fecha de la factura                                      |
| NombreCliente      | ✓           | Snapshot del nombre del cliente al momento de la factura |
| DescripcionGeneral |             | Información adicional que se desee incluir               |
| TotalRepuestos     | ✓           | Suma acumulada de los repuestos asociados                |
| TotalReparaciones  | ✓           | Suma acumulada de las reparaciones asociadas             |
| Descuento          | ✓           | Descuento aplicado; editable                             |
| SubTotal           | ✓           | Calculado: `TotalRepuestos + TotalReparaciones - Descuento`. No editable. |
| ImpuestoVentas     | ✓           | Calculado (IVA): `SubTotal * tasa`, donde `tasa` viene de `Catalogo.Parametros` (fila `ImpuestoVentas`, ej. `13%`). Ya no es editable ni un porcentaje visible; se muestra como monto (₡). |
| Total              | ✓           | Calculado: `SubTotal + ImpuestoVentas`. No editable.      |
| Adelanto           | ✓           | Default: `0`; dinero recibido del cliente como adelanto; editable |
| Pendiente          | ✓           | Calculado: `Total - Adelanto`. No editable.               |
| EstadoFacturaID    | ✓           | Estado actual; ver sección 5.2                           |

**Reglas de negocio:**
- Estado inicial al crear: **Cotización** (ID 1).
- **Solo se puede eliminar** una factura en estado **Cotización**.
- **Facturas en estado Pagada (3) no se pueden eliminar ni modificar** (el backend valida y retorna error).
- `TotalRepuestos`/`TotalReparaciones` se acumulan agregando/editando/eliminando registros de `Reparacion`/`Repuesto`. `SubTotal`, `ImpuestoVentas` (IVA), `Total` y `Pendiente` se recalculan automáticamente cada vez que cambian `TotalRepuestos`, `TotalReparaciones`, `Descuento` o `Adelanto` — implementación centralizada en `RecalculadorTotalesFactura` (`CarFix.Aplicacion/Comun/`), que lee la tasa de IVA desde `Catalogo.Parametros` (si el parámetro falta o no es numérico, la tasa cae a `0` sin bloquear la operación).
- Al cambiar el estado a **Pagada**, el sistema copia automáticamente los repuestos de esa factura a `Catalogo.HistoricoRespuesto`.
- **Envío de factura por correo:** el botón "Enviar factura" genera un PDF con el diseño de la sección 6 (encabezado del `Taller`, datos de cliente/vehículo, desglose de reparaciones/repuestos y totales) y lo envía por correo al `Email` del cliente asociado (`Factura.VehiculoID → Vehiculo.ClienteID → Cliente.Email`).
  - Requiere que el cliente tenga `Email` registrado; si no, el backend rechaza la solicitud con `400` y el mensaje "El cliente no tiene correo electronico registrado...". El botón se deshabilita en el frontend cuando la factura seleccionada no tiene `emailCliente`.
  - Requiere que exista un registro en `Catalogo.Taller` (tabla mono-registro); si está vacía, el backend rechaza la solicitud.
  - Disponible en cualquier estado de la factura (Cotización, Pendiente o Pagada) — aplica tanto para enviar la proforma como la factura final.
  - Endpoint: `POST /api/facturas/{id}/enviar`. No hay reintentos automáticos ni cola; si el envío falla (ej. SMTP no configurado o caído), el usuario ve el error y puede reintentar manualmente.

**Pantalla — requisitos de UI:**
- Botones en la lista: Agregar, Modificar, Eliminar.
- Filtros: placa del vehículo, número de orden de servicio, nombre del cliente.
- Paginación activada cuando la lista supera 25 registros.
- Para ver el detalle: un clic sobre la fila la selecciona/marca (habilita Modificar/Eliminar); **doble clic** sobre la fila abre el detalle directamente.
- Al seleccionar, mostrar detalle de campos con botones Aceptar y Cancelar.
- En el detalle (modos ver y editar), arriba a la derecha del panel se muestran los botones **Abrir PDF** y **Enviar factura**, habilitados sin importar el estado de la factura. No aparecen en el listado.
- **En celular** (ancho ≤768px): el listado solo muestra las columnas `#`, `Placa`, `Vehículo` y `Cliente` (Fecha, Repuestos, Reparaciones, Total y Estado se ocultan).
- **Recálculo en vivo (modo editar):** al escribir `Descuento` o `Adelanto`, `SubTotal`/`IVA`/`Total General`/`Pendiente` se recalculan en el navegador sin esperar a guardar (`computed()` en `facturas.component.ts` que reproduce la misma fórmula de `RecalculadorTotalesFactura`). La tasa de IVA se deriva de `impuestoVentas/subTotal` de la factura ya cargada — evita llamar a `GET /api/parametros`, que está restringido a rol Administrador. El backend sigue siendo la fuente de verdad al guardar.

---

### 5.7 Reparaciones (asociadas a Factura)

Sub-pantalla dentro de la Factura para gestionar las reparaciones o servicios realizados al vehículo.

**Campos** (según tabla `Sistema.Reparacion`):

| Campo                 | Obligatorio | Notas                                                           |
|-----------------------|:-----------:|-----------------------------------------------------------------|
| FacturaID             | ✓           | FK a la factura contenedora                                     |
| TipoReparacionID      |             | FK al catálogo; se completa solo al seleccionar del catálogo    |
| DescripcionReparacion | ✓           | Descripción del trabajo o servicio                              |
| Costo                 | ✓           | Default: `0`; editable aunque venga pre-relleno del catálogo   |
| DuracionAproximadaHoras |           | Opcional; horas estimadas del trabajo. Se pre-rellena con el valor del catálogo al usar `TipoReparacion`, pero es editable; en registro manual tambien puede indicarse. Alimenta el calculo automatico de `FechaSalida` (ver sección 5.5) |
| Listo                 | ✓           | Default: `false`; indica si el trabajo fue completado           |

**Reglas de negocio:**
- Se pueden registrar de dos formas:
  1. **Desde el catálogo**: buscar en `TipoReparacion`, seleccionar un tipo y copiar los datos a la reparación (descripción, costo y duración son editables).
  2. **Manual**: digitar descripción y costo directamente sin usar el catálogo; la duración es opcional.
- Acciones disponibles (Agregar, Modificar, Eliminar): **Mecánico** y **JefeMecanicos** mientras la factura **no** esté en estado **Pagada**.
- El campo `Listo` se actualiza via `PATCH /api/reparaciones/{id}/listo` de forma independiente (no requiere editar la reparacion completa).
- Al agregar, modificar o eliminar una reparación se recalcula automáticamente la `FechaSalida` de la orden asociada (ver sección 5.5).

**Pantalla — requisitos de UI:**
- Lista, Agregar, Modificar, Eliminar dentro del contexto de la Factura.
- Buscador en catálogo `TipoReparacion` con pre-relleno de descripción y costo. Busca automaticamente mientras se escribe (debounce ~300ms), sin boton "Buscar".
- El campo `Listo` se muestra como checkbox interactivo en la lista; se deshabilita si la factura esta en estado **Pagada** o la orden en estado **Finalizado/Entregado**.

---

### 5.8 Repuestos (asociados a Factura)

Sub-pantalla dentro de la Factura para gestionar los repuestos o piezas utilizadas en la reparación.

**Campos** (según tabla `Sistema.Repuesto`):

| Campo          | Obligatorio | Notas                                               |
|----------------|:-----------:|-----------------------------------------------------|
| FacturaID      | ✓           | FK a la factura contenedora                         |
| Incluido       | ✓           | Default: `false`; indica si el mecanico ya coloco/utilizo el repuesto en el vehiculo |
| NombreRepuesto | ✓           | Descripción del repuesto o pieza                    |
| Costo          | ✓           | Costo del repuesto incluido impuesto de ventas      |
| Fecha          | ✓           | Fecha de la factura del proveedor                   |
| Repuestera     | ✓           | Nombre de la tienda o proveedor donde se compró     |
| Factura        |             | Número de factura de compra emitida por el proveedor|

**Reglas de negocio:**
- Se pueden registrar de dos formas:
  1. **Desde el histórico**: buscar en `HistoricoRespuesto` por marca, modelo y año; si existe, copiar los datos (precio editable).
  2. **Manual**: digitar todos los campos a partir de una cotización real del proveedor.
- Acciones disponibles (Agregar, Modificar, Eliminar): **Mecánico** y **JefeMecanicos** mientras la factura **no** esté en estado **Pagada**.
- El campo `Incluido` se actualiza via `PATCH /api/repuestos/{id}/incluido` de forma independiente (no requiere editar el repuesto completo). Mismo proposito que `Listo` en Reparaciones (sección 5.7): permite al mecánico ir marcando, desde el detalle de la orden, los repuestos y reparaciones en los que va trabajando.

**Pantalla — requisitos de UI:**
- Lista, Agregar, Modificar, Eliminar dentro del contexto de la Factura.
- Buscador en `HistoricoRespuesto` filtrando por marca, modelo y año. Busca automaticamente mientras se escribe (debounce ~300ms), sin boton "Buscar".
- El campo `Incluido` se muestra como checkbox interactivo en la lista; se deshabilita si la factura esta en estado **Pagada** o la orden en estado **Finalizado/Entregado**.

**Característica especial — escaneo IA (factura del proveedor):**
- El mecánico fotografía la factura del proveedor de repuestos, desde el boton "Escanear factura de repuestos" disponible tanto en el mini-formulario de **Agregar** como en el de **Modificar** repuesto (dentro del detalle de la Factura).
- La IA extrae: la lista de repuestos de la factura (nombre/descripcion de cada linea), el monto **total** de la factura, la fecha, el nombre de la tienda (Repuestera) y el numero de factura.
- Si la factura tiene varios repuestos, **no se crean varios registros**: los nombres se concatenan con coma en un solo campo `NombreRepuesto`, y `Costo` se llena con el monto total de la factura (no por item). Esta concatenacion la hace el backend (`EscanearFacturaRepuestoHandler`), no el frontend.
- Los datos pre-rellenan el formulario (solo los campos que la IA pudo leer, sin borrar lo que el mecánico ya haya escrito a mano); el mecánico valida y corrige antes de guardar. Mismo principio de confianza que el escaneo de tarjeta de circulación (sección 5.4).
- Disponible por igual en escritorio y celular (mismo patron que el escaneo de vehiculo: `capture="environment"` en el input de archivo abre la camara en celular y el selector de archivos en escritorio).
- Implementación: backend expone `POST /api/repuestos/escanear-factura` (`IFormFile`, limite 10MB, JPEG/PNG/WEBP), que delega en `IServicioVisionFacturaRepuesto` (Dominio) / `ServicioVisionFacturaRepuestoAnthropic` (Infraestructura) — mismo patron que `IServicioVisionVehiculo`/`ServicioVisionAnthropic` de la sección 5.4 (modelo `Anthropic:Modelo`, salida JSON forzada, foto no persistida). Frontend: `RepuestosService.escanearFacturaRepuesto` en `facturas.component.ts`.

---

### 5.9 Catálogo de Tipos de Reparación

Pantalla donde el **JefeMecanicos** y el **Administrador** administran los tipos de reparación y servicios que ofrece el taller. Sirve como catálogo de búsqueda en la pantalla de Reparaciones (sección 5.7).

**Campos** (según tabla `Catalogo.TipoReparacion`):

| Campo                   | Obligatorio | Notas                                              |
|-------------------------|:-----------:|----------------------------------------------------|
| TipoReparacionID        | ✓           | ID manual — la tabla no tiene IDENTITY             |
| DescripcionReparacion   | ✓           | Descripción del servicio o tipo de reparación      |
| DuracionAproximadaHoras | ✓           | Duración estimada en horas; mínimo 1 hora          |
| CostoBase               | ✓           | Costo promedio de referencia del servicio          |

**Reglas de negocio:**
- La duración mínima es de **1 hora**.
- `TipoReparacionID` no tiene IDENTITY — el ID se asigna manualmente al insertar.
- El **Mecánico** usa este catálogo como búsqueda de solo lectura desde la pantalla de Reparaciones; no tiene acceso a su mantenimiento directo.

**Pantalla — requisitos de UI:**
- Botones: Agregar, Modificar, Eliminar.
- Filtro por `DescripcionReparacion`.
- Paginación activada cuando la lista supera 25 registros.
- Al seleccionar, mostrar detalle de campos con botones Aceptar y Cancelar.

---

### 5.10 Histórico de Repuestos

Catálogo histórico de repuestos adquiridos en reparaciones reales. Sirve como referencia de precios pasados al cotizar nuevas facturas proforma.

**Campos** (según tabla `Catalogo.HistoricoRespuesto`):

| Campo              | Obligatorio | Notas                                                             |
|--------------------|:-----------:|-------------------------------------------------------------------|
| Marca              | ✓           | Marca del vehículo                                                |
| Modelo             | ✓           | Modelo del vehículo                                               |
| Annio              | ✓           | Año de fabricación del vehículo                                   |
| Motor              | ✓           | Características del motor — **columna pendiente de agregar en BD**|
| RepuestoDecripcion | ✓           | Descripción del repuesto                                          |
| Precio             | ✓           | Precio pagado por el repuesto                                     |
| Repuestera         | ✓           | Tienda o proveedor donde se compró                                |
| FechaCompra        | ✓           | Fecha de adquisición                                              |

**Reglas de negocio:**
- Se alimenta **automáticamente** al pasar una `Factura` a estado **Pagada**: los repuestos reales de esa factura se copian a este histórico.
- No tiene FK con ninguna otra tabla — los datos del vehículo se almacenan desnormalizados para preservar el histórico independientemente de cambios posteriores.
- **Mecánico** puede consultar el histórico como buscador desde la pantalla de Repuestos (sección 5.8), pero no gestiona su mantenimiento directo.
- Acciones de Agregar, Modificar y Eliminar disponibles solo para **JefeMecanicos** y **Administrador**.

**Pantalla — requisitos de UI:**
- Botones: Agregar, Modificar, Eliminar (solo JefeMecanicos y Administrador).
- Filtros: Marca, Modelo, Año, descripción del repuesto.
- Paginación activada cuando la lista supera 25 registros.
- Al seleccionar, mostrar detalle de campos con botones Aceptar y Cancelar.
- También se presenta como panel de búsqueda dentro de la pantalla de Repuestos (sección 5.8).

> **Pendiente BD:** Agregar columna `Motor varchar(50) NOT NULL` a `Catalogo.HistoricoRespuesto`.
> **Typo BD:** La columna `RepuestoDecripcion` debería llamarse `RepuestoDescripcion` (falta la `s`).

---

## 6. Estructura de la Factura

**Encabezado**
```
                    [NOMBRE DEL TALLER]                    Factura #: 0001
                    [Dirección del taller]                 Fecha: DD/MM/AAAA
                    [Teléfonos] | [Email]
```

**Datos del cliente y vehículo** (izquierda)
```
Cliente: [Nombre del cliente]
Placa: [XXX-000]   Marca: [Toyota]   Modelo: [Corolla]   Año: [2020]
```

**Desglose** (Reparaciones primero, luego Repuestos, con un espacio de separacion entre ambos bloques)

| Reparación (descripción) | Monto | &nbsp; | Repuesto (descripción) | Precio |
|---------------------------|-------|--------|-------------------------|--------|
| ...                       | ₡x.xx |        | ...                     | ₡x.xx  |

**Pie de factura**
```
_______________________________        Total Repuestos:    ₡x.xx
Recibido de conformidad                Total Reparaciones: ₡x.xx
                                       Descuento:          ₡x.xx
                                       SubTotal:           ₡x.xx
                                       IVA:                ₡x.xx
                                       TOTAL GENERAL:      ₡x.xx
                                       Adelanto:           ₡x.xx
                                       Pendiente:          ₡x.xx
```

Los datos del encabezado de la factura (nombre, dirección, teléfonos, email) se leen de la tabla `Taller` en base de datos.

**Implementación — generación y envío del PDF:**
- El PDF se genera en el backend con **QuestPDF** (licencia Community — gratuita para empresas con ingresos anuales menores a $1M USD; requiere `QuestPDF.Settings.License = LicenseType.Community` en `Program.cs`). Servicio: `IServicioGeneradorFacturaPdf` (interfaz en `CarFix.Dominio.Interfaces`, implementación `ServicioGeneradorFacturaPdfQuestPdf` en `CarFix.Infraestructura/Pdf/`).
- El correo se envía con **MailKit** vía SMTP. Servicio: `IServicioEnvioCorreo` / `ServicioEnvioCorreoSmtp` en `CarFix.Infraestructura/Correo/`.
- Configuración SMTP en `appsettings.json` bajo la clave `Smtp` (`Host`, `Puerto`, `UsuarioRemitente`, `NombreRemitente`, `Contrasenna`, `UsarSsl`) — igual que `Anthropic:ApiKey`, la contraseña **no** se guarda en el repositorio: usar `dotnet user-secrets set "Smtp:Contrasenna" "..."` en desarrollo o la variable de entorno `Smtp__Contrasenna` en producción (también `Smtp__Host`, `Smtp__UsuarioRemitente`, etc. si difieren del placeholder).
- El PDF **no se persiste en disco ni en BD** — se genera en memoria y se adjunta directamente al correo.
- El comando `EnviarFacturaCommand` (`Features/Facturas/Commands/EnviarFactura/`) carga la factura con `Vehiculo→Cliente`, `Reparaciones` y `Repuestos`, valida que el cliente tenga `Email` y que exista un registro en `Catalogo.Taller`, genera el PDF y lo envía. Endpoint: `POST /api/facturas/{id}/enviar`.

## 7. Base de Datos

- **Servidor:** `localhost\SQL2022`
- **Base de datos:** `CAR_FIX`
- **Conexión:** `Server=localhost\SQL2022;Database=CAR_FIX;Integrated Security=True;TrustServerCertificate=True`
- **Esquema completo:** [`DATABASE_DOCUMENTATION.md`](.claude/DATABASE_DOCUMENTATION.md)

### Tablas principales

| Tabla            | Tipo       | Descripción                                      |
|------------------|------------|--------------------------------------------------|
| `Clientes`       | Transaccional | Clientes del taller (personas o empresas)     |
| `Vehiculos`      | Transaccional | Vehículos asociados a clientes                |
| `OrdenServicio`  | Transaccional | Documento central; registra ingreso al taller |
| `Reparacion`     | Transaccional | Trabajos ejecutados dentro de una orden       |
| `Repuesto`       | Transaccional | Piezas utilizadas en una orden                |
| `Facturas`       | Transaccional | Documento de cobro al cliente                 |
| `Taller`              | Configuración | Nombre, dirección, teléfonos y email del taller; fuente del encabezado de factura |
| `TipoReparacion`      | Catálogo   | Catálogo de tipos de trabajo con costo base; fuente de búsqueda en Reparaciones |
| `HistoricoRespuesto`  | Catálogo   | Historial de repuestos comprados; referencia de precios para cotizaciones        |
| `MarcaModelo`         | Catálogo   | Combinaciones Marca/Modelo/Annio de vehículos comunes; fuente de autocompletado en Vehículos |
| `EstadoOrden`         | Catálogo   | Estados de la orden (Cotización → Recibido → Reparando → Finalizado → Entregado) |
| `EstadoFactura`       | Catálogo   | Estados de la factura (Cotización, Pendiente, Pagada)                            |
| `Roles`          | Seguridad  | Roles del sistema (Administrador, JefeMecanicos, Mecánico) |
| `Usuarios`       | Seguridad  | Usuarios con credenciales BCrypt y rol asignado  |

### Problemas conocidos del esquema (ver también `PENDIENTES.md`)

- `EstadoFactura.Descipcion` tiene typo (falta la `r`).
- `Taller.UbicaciónGPS` tiene tilde en el nombre de columna — inconsistente con la convención del proyecto. (`Clientes.Direccion` y `Clientes.Localizacion` ya fueron corregidas en la BD).

## 8. Comandos del Proyecto

| Comando | Descripción |
|---------|-------------|
| `/sync-db-docs` | Conecta a `CAR_FIX` y regenera `.claude/DATABASE_DOCUMENTATION.md` con el esquema actual. Usar después de cualquier cambio en la BD. |
| `/arrancar-app` | Detiene instancias previas y arranca backend (puerto 5151) y frontend (puerto 4200) usando la IP Wi-Fi actual de la máquina. |

## 9. Frontend Angular

El frontend se desarrollará con **Angular 21**. Al escribir cualquier código Angular, seguir obligatoriamente las convenciones de esta guía:

@.claude/angular_buenas_practicas.md

## 10. Backend ASP.NET Core

El backend se desarrollará con **ASP.NET Core Web API, .NET 10, C#** siguiendo Arquitectura Limpia (Clean Architecture) con 5 proyectos: `Dominio`, `Aplicacion`, `Infraestructura`, `WebApi` y `Especificaciones`. Al escribir cualquier código backend, seguir obligatoriamente los patrones de esta guía:

@.claude/arquitectura_limpia_backend.md

## 11. Patron de Pantallas — Convenciones de Implementacion

Todas las pantallas de mantenimiento siguen el mismo patron. Al implementar una nueva pantalla o modificar una existente, respetar estas convenciones exactamente.

### 12.1 Modo de la Pantalla

Cada componente maneja un `signal<Modo>` en lugar de booleanos separados:

```typescript
// Pantallas basicas (Clientes, Vehiculos, Catalogo)
type Modo = 'ver' | 'crear' | 'editar';

// Pantallas con cambio de estado (Ordenes, Facturas)
type Modo = 'ver' | 'crear' | 'editar' | 'estado';
```

**Nunca** usar `modoEdicion = signal(false)` ni flags booleanos separados para controlar el formulario.

### 12.2 Estructura de Signals Obligatoria

```typescript
// ── Lista ──────────────────────────────────────────────────────────
items         = signal<ItemDto[]>([]);
cargando      = signal(false);
textoBusqueda = signal('');        // SIEMPRE signal, nunca string plano
seleccionado  = signal<ItemDto | null>(null);

itemsFiltrados = computed(() => {   // filtrado reactivo en cliente
  const texto = this.textoBusqueda().toLowerCase().trim();
  return !texto ? this.items()
    : this.items().filter(i => i.campo.toLowerCase().includes(texto));
});

// ── Formulario ─────────────────────────────────────────────────────
modo              = signal<Modo>('ver');
mostrarFormulario = signal(false);
guardando         = signal(false);
errorForm         = signal('');

// ── Campos del formulario (uno por campo) ──────────────────────────
campoUno = signal('');
campoDos = signal(0);
```

### 12.3 Comportamiento de Seleccion en la Lista

```typescript
seleccionar(item: ItemDto) {
  // Clic en fila ya seleccionada con formulario abierto → cerrar
  if (this.seleccionado()?.itemId === item.itemId && this.mostrarFormulario()) {
    this.cerrarFormulario();
    return;
  }
  // Clic en fila nueva → abrir modo 'ver'
  this.seleccionado.set(item);
  this.modo.set('ver');
  this.mostrarFormulario.set(true);
  this.errorForm.set('');
  // Precargar datos del formulario si aplica
}
```

### 12.4 Comportamiento de los Botones del Encabezado

```typescript
abrirFormulario(m: Exclude<Modo, 'ver'>) {
  if (m !== 'crear' && !this.seleccionado()) return;
  this.errorForm.set('');
  this.guardando.set(false);

  if (m === 'crear') {
    this.seleccionado.set(null);
    // Inicializar campos a valores por defecto
  } else if (m === 'editar') {
    // Cargar datos del seleccionado en los signals de campo
    const s = this.seleccionado()!;
    this.campoUno.set(s.campoUno);
  } else if (m === 'estado') {
    this.nuevoEstadoId.set(this.seleccionado()!.estadoId);
  }

  this.modo.set(m);
  this.mostrarFormulario.set(true);
}

cancelar() {
  if (this.modo() === 'crear') {
    this.cerrarFormulario();   // 'crear' cierra todo
    return;
  }
  this.modo.set('ver');        // 'editar'/'estado' vuelve a 'ver'
  this.errorForm.set('');
}

private cerrarFormulario() {
  this.mostrarFormulario.set(false);
  this.seleccionado.set(null);
  this.errorForm.set('');
}
```

### 12.5 Patron del Template

```html
<div class="pantalla-contenedor">

  <!-- Encabezado con botones de accion -->
  <div class="pantalla-encabezado">
    <h2 class="pantalla-titulo">Titulo</h2>
    <div class="acciones-barra">
      <button class="btn-accion" (click)="abrirFormulario('crear')">Agregar</button>
      <button class="btn-accion" (click)="abrirFormulario('editar')" [disabled]="!seleccionado()">Modificar</button>
      <button class="btn-accion btn-peligro" (click)="confirmarEliminar()" [disabled]="!seleccionado()">Eliminar</button>
    </div>
  </div>

  <!-- Lista: oculta mientras el formulario esta abierto -->
  @if (!mostrarFormulario()) {
    <div class="filtros-barra">
      <input class="filtro-input" type="text" placeholder="Buscar..."
             [value]="textoBusqueda()"
             (input)="textoBusqueda.set($any($event.target).value)" />
    </div>
    <div class="tabla-contenedor">
      <table class="tabla-datos">
        <thead>...</thead>
        <tbody>
          @for (item of itemsFiltrados(); track item.itemId) {
            <tr [class.fila-seleccionada]="seleccionado()?.itemId === item.itemId"
                (click)="seleccionar(item)">
              ...
            </tr>
          }
        </tbody>
      </table>
    </div>
  }

  <!-- Formulario: oculto cuando la lista esta visible -->
  @if (mostrarFormulario()) {
    <div class="formulario-panel">

      @if (modo() === 'ver') {
        <h3 class="formulario-titulo">Detalle de ...</h3>
        <div class="banner-solo-lectura">🔒 Solo lectura — presione Modificar para editar</div>
        <div class="form-grid solo-lectura">
          <!-- campos readonly -->
        </div>
        <div class="form-acciones">
          <button class="btn-secundario" (click)="cancelar()">Cerrar</button>
          <button class="btn-accion" (click)="abrirFormulario('editar')">Modificar</button>
        </div>
      }

      @if (modo() === 'crear' || modo() === 'editar') {
        <h3 class="formulario-titulo">{{ modo() === 'crear' ? 'Nuevo ...' : 'Modificar ...' }}</h3>
        <div class="form-grid">
          <!-- campos editables con (input) no [(ngModel)] -->
          <div class="form-grupo">
            <label>Campo *</label>
            <input type="text" [value]="campoUno()"
                   (input)="campoUno.set($any($event.target).value)" />
          </div>
        </div>
        @if (errorForm()) { <p class="error-form">{{ errorForm() }}</p> }
        <div class="form-acciones">
          <button class="btn-secundario" (click)="cancelar()">Cancelar</button>
          <button class="btn-accion" (click)="guardar()" [disabled]="guardando()">
            {{ guardando() ? 'Guardando...' : 'Aceptar' }}
          </button>
        </div>
      }

      @if (modo() === 'estado') {
        <!-- panel de cambio de estado -->
        <div class="form-acciones">
          <button class="btn-secundario" (click)="cancelar()">Cancelar</button>
          <button class="btn-accion" (click)="guardarEstado()" [disabled]="guardando()">Aceptar</button>
        </div>
      }

    </div>
  }

</div>
```

### 12.6 Inputs de Formulario — Regla Critica

**Nunca usar `[(ngModel)]` en campos de formulario que actualicen signals.** Usar siempre el evento `(input)` con `$any($event.target).value`:

```html
<!-- CORRECTO -->
<input type="text" [value]="nombre()"
       (input)="nombre.set($any($event.target).value)" />

<input type="number" [value]="costo()"
       (input)="costo.set(+$any($event.target).value)" />

<select [value]="estadoId()"
        (change)="estadoId.set(+$any($event.target).value)">

<!-- INCORRECTO — rompe la reactividad con Signals -->
<input type="text" [(ngModel)]="nombreString" />
```

`[(ngModel)]` solo se puede usar con variables de clase normales (string, number, boolean), nunca con signals.

### 12.7 Modo Solo Lectura

El modo `'ver'` desactiva toda interaccion con el formulario mediante CSS:

```css
/* styles.css — ya definido globalmente */
.form-grid.solo-lectura { pointer-events: none; }
.form-grid.solo-lectura input,
.form-grid.solo-lectura textarea,
.form-grid.solo-lectura select {
  background: var(--color-fondo);
  color: var(--color-texto-suave);
  cursor: default;
}
```

El banner amarillo de solo lectura:
```css
.banner-solo-lectura { /* ya definido en styles.css */ }
```

**No agregar `[attr.readonly]` a cada campo individualmente** — el `pointer-events: none` en el contenedor es suficiente y mas limpio.

### 12.8 Manejo de Errores del Backend

El backend devuelve `ProblemDetails` (JSON) en errores 4xx/5xx. Usar siempre este helper en todos los componentes:

```typescript
private extraerError(err: { error?: unknown }): string {
  const e = err.error;
  if (typeof e === 'string') return e;
  return (e as any)?.detail ?? (e as any)?.title ?? 'Error al procesar la solicitud.';
}
```

**Nunca** hacer `this.errorForm.set(err.error)` directamente — genera `[object Object]` cuando el backend devuelve JSON.

### 12.9 CSS Classes del Sistema de Diseno

Todas estas clases estan definidas en `styles.css` y deben usarse en toda pantalla nueva.

> **ADVERTENCIA — Pantallas antiguas usan nombres distintos.** `Clientes`, `Vehiculos`, `CatalogoReparaciones`, `CatalogoRepuestos`, `Ordenes` fueron escritas antes de que se estandarizara este patron y usan clases legacy (`.pagina-header`, `.btn.btn-primario`, `.filtro-bar`, `table` sin clase, `.seleccionada`). **No tomar esas pantallas como referencia de CSS.** La pantalla `Facturas` es la referencia correcta del patron actual.

| Clase nueva (usar) | Clase legacy equivalente (NO usar en pantallas nuevas) |
|---|---|
| `.pantalla-encabezado` | `.pagina-header` |
| `.acciones-barra` | `.acciones` |
| `.btn-accion` | `.btn.btn-primario` |
| `.btn-accion.btn-peligro` | `.btn.btn-peligro` |
| `.btn-secundario` | `.btn.btn-secundario` |
| `.filtros-barra` | `.filtro-bar` |
| `.filtro-input` | `input[type="text"]` dentro de `.filtro-bar` |
| `.tabla-datos` | `table` (sin clase) |
| `.fila-seleccionada` | `.seleccionada` |
| `.texto-vacio` / `.texto-cargando` | `.celda-vacia` / `.cargando` |
| `.formulario-titulo` | `.formulario-panel h3` |
| `.form-grupo.form-grupo-ancho` | (no existia) |
| `.error-form` | (no existia) |

**Clases completas disponibles:**

| Clase | Uso |
|---|---|
| `.pantalla-contenedor` | Wrapper raiz de cada pantalla |
| `.pantalla-encabezado` | Fila con titulo + botones |
| `.pantalla-titulo` | `<h2>` del nombre de la pantalla |
| `.acciones-barra` | Contenedor de botones de accion |
| `.btn-accion` | Boton de accion principal (azul) |
| `.btn-accion.btn-peligro` | Boton destructivo (rojo) |
| `.btn-secundario` | Boton cancelar/cerrar (gris) |
| `.filtros-barra` | Contenedor de filtros de busqueda |
| `.filtro-input` | Input de busqueda por texto |
| `.tabla-contenedor` | Wrapper de la tabla (da sombra y bordes) |
| `.tabla-datos` | Tabla de listado (dentro de `.tabla-contenedor`) |
| `.fila-seleccionada` | Fila activa en la tabla |
| `.texto-vacio` | Mensaje cuando la lista esta vacia |
| `.texto-cargando` | Mensaje mientras carga |
| `.formulario-panel` | Panel del formulario (reemplaza la lista) |
| `.formulario-titulo` | `<h3>` dentro del panel |
| `.form-grid` | Grid de campos del formulario |
| `.form-grid.solo-lectura` | Grid en modo ver (no interactivo) |
| `.form-grupo` | Contenedor de label + campo |
| `.form-grupo.form-grupo-ancho` | Campo que ocupa todo el ancho del grid |
| `.form-acciones` | Fila de botones Cancelar/Aceptar |
| `.banner-solo-lectura` | Banner amarillo de modo ver |
| `.error-form` | Mensaje de error debajo del form |
| `.badge` | Etiqueta de estado |
| `.badge-1` a `.badge-5` | Estados de Ordenes de Servicio |
| `.badge-f-1` a `.badge-f-3` | Estados de Facturas (f = factura) |

### 12.10 Badges de Estado

**Ordenes de Servicio** (`badge-N` donde N = EstadoOrdenId):
- `badge-1` amarillo — Cotizacion
- `badge-2` azul — Recibido
- `badge-3` rojo — En reparacion
- `badge-4` verde — Finalizado
- `badge-5` gris — Entregado

**Facturas** (`badge-f-N` donde N = EstadoFacturaId):
- `badge-f-1` amarillo — Cotizacion
- `badge-f-2` rojo — Pendiente
- `badge-f-3` verde — Pagada

```html
<!-- Ordenes -->
<span [class]="'badge badge-' + orden.estadoOrdenId">{{ orden.estadoOrdenDescripcion }}</span>

<!-- Facturas -->
<span [class]="'badge badge-f-' + factura.estadoFacturaId">{{ factura.estadoFacturaDescripcion }}</span>
```

### 12.11 Pantallas con Tabs de Detalle (Patron Facturas)

Cuando una pantalla tiene sub-entidades que se muestran en la vista de detalle (modo `'ver'`), se usan tabs dentro del `formulario-panel`. El patron es:

```typescript
// Signals adicionales para tabs
tabActivo    = signal<'tabA' | 'tabB'>('tabA');
itemsTabA    = signal<ItemADto[]>([]);
itemsTabB    = signal<ItemBDto[]>([]);
cargandoTab  = signal(false);

mostrarFormSubA = signal(false);

cambiarTab(tab: 'tabA' | 'tabB') {
  this.tabActivo.set(tab);
  this.mostrarFormSubA.set(false);
  this.cargarTab(tab);
}
```

Los tabs solo aparecen en modo `'ver'`. Al cambiar a modo `'editar'` o `'estado'`, los tabs desaparecen.

Las sub-entidades tienen sus propios signals de formulario inline (prefijo `sub`):
```typescript
subCampoUno = signal('');
subCampoDos = signal(0);
```

Al guardar una sub-entidad (agregar reparacion, repuesto, etc.) se llama `recargarSeleccionado(id)` para refrescar los totales de la entidad padre en el panel sin cerrar el formulario.

### 12.12 Responsive Movil — Columnas Reducidas y Paginacion Movil

La aplicacion debe verse correctamente en celulares reales (referencia: 360x800px). El breakpoint de "movil" es `max-width: 768px` (el mismo que ya usa el sidebar para convertirse en menu hamburguesa). Convenciones aplicadas en Clientes y Vehiculos, reutilizables para cualquier pantalla de listado nueva:

**Columnas reducidas en tablas legacy:**
- Se agrega la clase `col-oculta-movil` a los `<th>`/`<td>` que deben ocultarse en celular (definida en `styles.css`, `display:none` bajo el breakpoint de 768px).
- Solo se muestran las columnas mas identificativas de cada fila (ej. Clientes: `#`, Nombre, Telefono; Vehiculos: `#`, Placa, Marca, Cliente).
- Si la tabla queda con pocas columnas visibles, se anula el ancho minimo forzado de la tabla (agregando una clase especifica, ej. `.tabla-clientes`, `.tabla-vehiculos`) para que no aparezca scroll horizontal innecesario.

**Paginacion con tamanio reducido en movil (no scroll infinito):**

El scroll infinito se descarto: el re-render disparado por el listener de scroll durante la carga de mas filas interfiere con el `(click)` de seleccion de fila (el usuario debe tocar dos veces para que abra el detalle). En su lugar, movil usa el mismo mecanismo de paginacion que escritorio, solo que con paginas mas pequenias.

```typescript
private readonly mediaMovil = window.matchMedia('(max-width: 768px)');
esMovil      = signal(this.mediaMovil.matches);
paginaActual = signal(0);

private readonly onCambioMedia = (e: MediaQueryListEvent) => {
  this.esMovil.set(e.matches);
  this.paginaActual.set(0);
};

tamanoPagina   = computed(() => this.esMovil() ? 15 : 25); // TAMANO_PAGINA_MOVIL / POR_PAGINA
paginasTotales = computed(() => Math.ceil(this.itemsFiltrados().length / this.tamanoPagina()));

itemsMostrados = computed(() => {
  const inicio = this.paginaActual() * this.tamanoPagina();
  return this.itemsFiltrados().slice(inicio, inicio + this.tamanoPagina());
});

reiniciarPaginacion(): void {
  this.paginaActual.set(0);
}
```

```html
@if (itemsFiltrados().length > tamanoPagina()) {
  <div class="paginacion">
    <button class="btn btn-outline" [disabled]="paginaActual() === 0"
            (click)="paginaActual.update(p => p - 1)">‹</button>
    <span>{{ paginaActual() + 1 }} / {{ paginasTotales() }}</span>
    <button class="btn btn-outline" [disabled]="paginaActual() >= paginasTotales() - 1"
            (click)="paginaActual.update(p => p + 1)">›</button>
  </div>
}
```
- En escritorio (>768px): paginacion de 25 registros con botones anterior/siguiente.
- En movil (≤768px): mismo mecanismo, pero de 15 registros por pagina. Los controles de paginacion se muestran igual en ambos casos.
- Al cambiar el texto de busqueda o los filtros, `paginaActual` se reinicia a 0 (metodo `reiniciarPaginacion()`).
- La deteccion de movil se actualiza automaticamente si el usuario rota la pantalla o redimensiona la ventana (listener `change` sobre el `MediaQueryList`), reiniciando tambien `paginaActual` a 0 para evitar quedar en una pagina fuera de rango al cambiar el tamanio de pagina.

### 12.13 Asistente de Voz Global

Boton flotante (FAB) visible en todas las pantallas autenticadas (`ShellComponent`, junto a `<app-install-prompt />`) que permite hablarle a la app para navegar y para crear registros de Cliente/Vehiculo en lenguaje natural.

**Flujo:** el navegador transcribe la voz a texto (Web Speech API, gratis, sin backend) → el texto se envia a `POST /api/asistente-voz/interpretar` junto con la ruta actual → Claude (mismo patron de IA que el escaneo de fotos, ver seccion 5.4) devuelve un intent estructurado (`navegar`, `buscar`, `crear_cliente`, `crear_vehiculo` o `desconocido`) → el frontend navega y/o prellena el formulario, **el mecanico siempre revisa y corrige antes de guardar** (mismo principio de confianza que el escaneo de fotos).

**Backend:**
- `IServicioAsistenteVoz` (Dominio) / `ServicioAsistenteVozAnthropic` (Infraestructura) — solo texto, sin imagen, `OutputConfig.Effort = Effort.Low` (tarea simple, evita el modo de razonamiento extendido).
- `IServicioLlamadaAnthropicJson` (Infraestructura) — helper compartido para llamadas a Claude con salida JSON forzada (usado por el asistente de voz; `ServicioVisionAnthropic` del escaneo de fotos aun no lo usa, queda como mejora futura).
- Un unico JSON Schema combinado cubre navegacion + busqueda + extraccion de campos de Cliente/Vehiculo en una sola llamada. **Importante:** la API de Claude rechaza combinar `"enum"` con `"type": [..., "null"]` en el mismo campo (error `output_config.format.schema`) — los campos nullable con lista de valores validos (ej. `pantallaDestino`) se dejan como `"type": ["string","null"]` sin `enum`, y el whitelist se valida en C# (`NormalizarPantalla` en `ServicioAsistenteVozAnthropic.cs`).
- Endpoint `POST /api/asistente-voz/interpretar`, `RequireAuthorization()` (cualquier rol autenticado).

**Frontend:**
- `ReconocimientoVozService` (`core/voz/`) — envuelve `SpeechRecognition`/`webkitSpeechRecognition` (`es-CR`); expone `soportado` (signal). TypeScript no trae estos tipos en `lib.dom.d.ts` — declarados en `src/types/speech-recognition.d.ts`.
- `AsistenteVozService` (`core/voz/`, `providedIn:'root'`) — orquestador global: signals `estado`/`mensajeUsuario`, mapa de permisos por pantalla (usa los computed `esAdmin`/`esJefe` de `AuthService` — es solo UX amigable, **la guardia de ruta real sigue siendo la unica fuente de verdad de seguridad**), y un signal de "accion pendiente" (`tomarAccionPendiente(pantalla)`) que sobrevive a la navegacion porque vive en un servicio root, no en el componente de ruta.
- Cada pantalla que participa (Clientes, Vehiculos, Ordenes) llama `asistenteVoz.tomarAccionPendiente('<pantalla>')` en su `ngOnInit` y aplica el resultado: `abrir-crear` (abre el formulario), `prellenar-cliente`/`prellenar-vehiculo` (sobreescribe solo los campos no-null, mismo principio que `aplicarDatosEscaneados` del escaneo de fotos), o `buscar` (setea `textoBusqueda`).
- El boton de microfono se muestra en escritorio (>768px); si `!soportado()` aparece deshabilitado con `title` explicando el motivo (soporte real de Web Speech API varia por navegador — confiable en Chrome/Android, inconsistente en Safari/iOS, sin soporte estable en Firefox desktop). **En movil (≤768px) el FAB se oculta** (`ShellComponent` inyecta `EsMovilService`) — el flujo por voz en movil pasa por la pantalla de Chat de la seccion 12.14.

### 12.14 Chat de Voz Movil — Flujo Encadenado

En movil (≤768px), la pantalla de aterrizaje tras el login (ruta `/chat`, decidida por `InicioComponent` usando `EsMovilService`) es un chat con microfono grande centrado (`ChatTallerComponent`/`ChatTallerService`, `features/chat-taller/`) que, a diferencia del FAB de la seccion 12.13, **ejecuta las acciones reales contra el backend** en vez de solo prellenar formularios — el mecanico crea un cliente, su vehiculo, la orden de servicio (con su factura Cotizacion atomica) y la envia por correo, cada accion dictada por separado como un comando de voz independiente (no un solo dictado largo).

**Maquina de estados (`ChatTallerService`):** `pasoActual` (`'cliente'|'vehiculo'|'orden'|'factura'`) y `fase` (`'inicio'|'confirmando'|'ejecutando'|'terminado'`). Cada comando exige confirmacion hablada antes de ejecutar: la IA resume los datos entendidos en el chat y espera que el usuario diga "si"/"confirmar" (clasificado localmente por `interprete-confirmacion.util.ts`, sin llamar a Claude para una decision binaria) antes de invocar `ClientesService.crear`/`VehiculosService.crear`/`OrdenesService.crear`/`FacturasService.enviar`. Decir "no" cancela el paso (o, en el paso de factura, solo omite el envio); una respuesta larga durante la confirmacion se trata como correccion de datos y se reinterpreta con `intentEnProgreso` como pista (`InterpretarComandoVozCommand.IntentEnProgreso`), sin adivinar un intent nuevo.

**Continuidad entre acciones separadas:** tras confirmar cada paso, el chat vuelve a esperar un nuevo comando de voz (no reintenta ni adivina el siguiente paso automaticamente). La continuidad entre acciones se da porque `ChatTallerService` recuerda en signals el `clienteIdCreado`/`vehiculoIdCreado`/`facturaIdCreada` de la sesion: si el mecanico dice "ahora crea un vehiculo..." sin mencionar el cliente, se usa automaticamente el ultimo cliente creado en la conversacion (mismo principio para vehiculo→orden). Si el mecanico menciona un cliente/vehiculo existente por nombre o placa (`nombreClienteBuscado`/`placaBuscada`) en vez de continuar la sesion, se resuelve por busqueda en vez de usar el ultimo creado.

**Intents nuevos en el backend** (`ServicioAsistenteVozAnthropic.cs`): `crear_orden` (objeto `orden`: `problemaGeneral`, `esGarantia`, `placaBuscada` opcional para referirse a un vehiculo ya existente por placa) y `enviar_factura` (sin campos propios, opera sobre la ultima factura creada en la conversacion). `CrearOrdenCommand` ahora retorna `CrearOrdenResponseDto(OrdenServicioId, FacturaId)` en vez de solo el `int` de la orden, para poder encadenar el envio de correo sin una consulta adicional.

**Flujo corto:** si el usuario menciona una placa existente (`orden.placaBuscada` o `vehiculo.nombreClienteBuscado`) en vez de dictar datos nuevos, el chat resuelve el cliente/vehiculo contra `ClientesService.obtener()`/`VehiculosService.obtener()` (mismo patron de busqueda por texto que ya usaba el prellenado de Vehiculos) y salta directo al paso correspondiente.

### 12.15 Instalacion como PWA — Banner Automatico y Opcion de Menu

Dos caminos para instalar CAR FIX como PWA, ambos respaldados por el mismo servicio central `InstalacionPwaService` (`core/pwa/`, `providedIn:'root'`) — evita que el evento `beforeinstallprompt` (el navegador lo dispara una sola vez) quede "atrapado" en un solo componente y no lo pueda usar otro.

- `InstalacionPwaService` centraliza: captura de `beforeinstallprompt`/`appinstalled`, deteccion de iOS (`navigator.userAgent` con fallback `maxTouchPoints` para iPadOS moderno, que oculta "iPad" del user-agent) y deteccion de instalacion previa (`matchMedia('(display-mode: standalone)')` o `navigator.standalone`). Expone `mostrarOpcionInstalar` (computed: `!yaInstalado() && (puedeInstalarAndroid() || esIOS())`) y el metodo `manejarClicMenu()` que bifurca por plataforma.
- **Banner automatico** (`InstallPromptComponent`, montado en `ShellComponent` junto al FAB de voz): aparece solo si el navegador dispara `beforeinstallprompt` (Chrome/Android/Edge) y el usuario no lo cerro antes en esta sesion (`sessionStorage['install-prompt-cerrado']`). En iOS ese evento nunca se dispara, asi que este banner nunca aparece ahi por si solo.
- **Opcion de menu** ("📲 Instalar aplicacion", `NavComponent`): visible para **todos los roles autenticados** (no solo Admin — a proposito no se puso en la pantalla de Configuracion, que es Admin-only), y solo cuando `mostrarOpcionInstalar()` es `true`. En Android/Chrome dispara el prompt nativo de instalacion (`instalarAndroid()`); en iOS no existe una API programatica, asi que abre un panel de instrucciones manuales ("Toca el icono Compartir en Safari y selecciona 'Agregar a pantalla de inicio'", reutilizando las clases CSS del banner automatico). El item desaparece del menu apenas la app queda instalada (evento `appinstalled` o `userChoice` aceptado).

## 12. Estructura del Proyecto

```
AppCarFix/
├── CLAUDE.md                            # Este archivo (raiz del repositorio)
├── .claude/
│   ├── settings.json
│   ├── PENDIENTES.md                    # Lista de pendientes (BD + desarrollo)
│   ├── DATABASE_DOCUMENTATION.md        # Esquema completo de BD (generado por /sync-db-docs)
│   ├── angular_buenas_practicas.md      # Guia de buenas practicas Angular 21
│   ├── arquitectura_limpia_backend.md   # Guia de Arquitectura Limpia ASP.NET Core
│   └── skills/
│       └── sync-db-docs/
│           └── SKILL.md
│
├── Backend/                             # Solucion .NET (CarFix.slnx)
│   ├── Dominio/CarFix.Dominio/
│   │   ├── Entidades/                   # Entidades generadas por EF scaffold
│   │   ├── Interfaces/                  # ICarFixDbContext, IRepositorio*, IServicio*
│   │   └── Excepciones/
│   ├── Aplicacion/CarFix.Aplicacion/
│   │   ├── Comun/                       # Resultado<T>, ComportamientoValidacion
│   │   └── Features/
│   │       ├── Autenticacion/
│   │       ├── Clientes/
│   │       ├── Vehiculos/
│   │       ├── OrdenesServicio/
│   │       ├── Facturas/
│   │       │   ├── Commands/
│   │       │   │   ├── CrearFactura/
│   │       │   │   ├── ActualizarFactura/
│   │       │   │   ├── EliminarFactura/
│   │       │   │   └── CambiarEstadoFactura/
│   │       │   ├── Queries/ObtenerFacturas/
│   │       │   └── Dtos/FacturaDto.cs
│   │       ├── Reparaciones/
│   │       ├── Repuestos/
│   │       ├── TiposReparacion/
│   │       ├── HistoricoRepuestos/
│   │       ├── Usuarios/
│   │       └── AsistenteVoz/             # InterpretarComandoVoz (navegacion/formularios por voz)
│   ├── Infraestructura/CarFix.Infraestructura/
│   │   ├── Persistencia/
│   │   │   ├── CarFixDbContext.cs       # Contexto EF (generado)
│   │   │   ├── Repositorios/
│   │   │   └── UnidadTrabajo.cs
│   │   ├── Seguridad/                   # ServicioToken, ServicioContrasenna
│   │   ├── IA/                          # ServicioVisionAnthropic (fotos), ServicioAsistenteVozAnthropic (voz), ServicioLlamadaAnthropicJson (helper compartido)
│   │   ├── Pdf/                         # ServicioGeneradorFacturaPdfQuestPdf
│   │   └── Correo/                      # ServicioEnvioCorreoSmtp
│   └── WebApi/CarFix.WebApi/
│       ├── Endpoints/                   # Un archivo por entidad (MapearXxx)
│       └── Excepciones/ManejadorExcepciones.cs
│
├── Frontend/car-fix-app/src/types/
│   └── speech-recognition.d.ts          # Tipos ambientales del Web Speech API (no estan en lib.dom.d.ts)
├── Frontend/car-fix-app/src/app/
│   ├── core/auth/                       # AuthService, authInterceptor, guards
│   ├── core/pwa/                        # InstalacionPwaService, InstallPromptComponent (seccion 12.15)
│   ├── core/voz/                        # ReconocimientoVozService, AsistenteVozService, AsistenteVozFabComponent
│   ├── core/comun/                      # EsMovilService (deteccion compartida de movil via matchMedia)
│   ├── models/                          # Interfaces DTO (*.model.ts)
│   ├── services/                        # Servicios HTTP (*.service.ts)
│   └── features/
│       ├── login/
│       ├── inicio/                      # InicioComponent — redirige a /chat o /ordenes segun EsMovilService
│       ├── chat-taller/                 # ChatTallerComponent/Service — chat de voz movil que ejecuta acciones (seccion 12.14)
│       ├── clientes/
│       ├── vehiculos/
│       ├── ordenes/
│       ├── facturas/
│       ├── catalogo-reparaciones/
│       ├── catalogo-repuestos/
│       ├── usuarios/
│       ├── configuracion/
│       └── sin-acceso/
│
└── PROMPTS/                             # Bitacora de sesiones de trabajo
```

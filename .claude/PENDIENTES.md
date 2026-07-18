# Pendientes — CAR_FIX

## Base de Datos

- [x] Agregar tabla `Usuarios` y tabla `Roles` — creadas en `Catalogo.Roles` y `Catalogo.Usuarios` con FK enforced y seed de 3 roles
- [ ] Corregir typo `EstadoFactura.Descipcion` → `Descripcion`
- [x] Cambiar `Vehiculos.Annio` de `tinyint` a `smallint` — confirmado en BD via `/sync-db-docs` (2026-07-02)
- [x] Renombrar `Clientes.Dirección` → `Direccion` y `Clientes.Localización` → `Localizacion` — confirmado en BD via `/sync-db-docs` (2026-07-05)
- [ ] Renombrar columna con tilde: `Taller.UbicaciónGPS` → `UbicacionGPS`
- [ ] Agregar columna `Motor varchar(50) NOT NULL` a `Catalogo.HistoricoRespuesto` (requerida por negocio — ver sección 5.10)
- [ ] Corregir typo `HistoricoRespuesto.RepuestoDecripcion` → `RepuestoDescripcion`

## Desarrollo

- [ ] Implementar pantalla "Configuración del sistema" (Solo Admin) para editar `Catalogo.Taller` desde la UI — hoy es tabla mono-registro sin mantenimiento. Se insertó un registro placeholder manualmente en BD (2026-07-03) para poder probar el envío de facturas por correo (sección 5.6/6 de `CLAUDE.md`); falta que el admin lo revise/actualice con los datos reales del taller.
- [ ] Configurar credenciales SMTP reales (`Smtp:Host`, `Smtp:UsuarioRemitente`, `Smtp:Contrasenna` vía `dotnet user-secrets` o variables de entorno `Smtp__*`) para habilitar el envío de facturas por correo — la infraestructura ya está implementada (QuestPDF + MailKit) pero sin credenciales el envío falla con "El servidor de correo no esta configurado".
- [x] Bug en `DELETE /api/ordenes/{id}` (`EliminarOrdenHandler.cs`): al eliminar una orden en estado Cotización/Recibido/Reparando, EF Core lanzaba `InvalidOperationException` ("The association between entity types 'Factura' and 'OrdenServicio' has been severed...") y el endpoint respondía `500`. Causa: el handler eliminaba primero la `Factura` (principal) y luego la `OrdenServicio` (dependiente) en la relación 1 a 1 con FK obligatoria `OrdenServicio.FacturaId`; EF Core no permite ese orden. Corregido invirtiendo el orden de los `Remove()` (orden primero, factura despues) — confirmado con prueba end-to-end (2026-07-18): `DELETE` devuelve `204` y ambas filas se eliminan correctamente.


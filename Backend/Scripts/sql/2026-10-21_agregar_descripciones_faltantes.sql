-- =============================================================================
-- 2026-10-21 — Agregar descripciones (MS_Description) faltantes
-- Motivo: el estandar de BD (Documentacion/ESTANDAR_BASE_DATOS.md, seccion 10.1)
-- exige que toda tabla y toda columna este documentada en la propia BD. Estas
-- tablas/columnas se crearon despues de la documentacion inicial y quedaron sin
-- descripcion.
--
-- Idempotente: si la descripcion ya existe la actualiza, si no la crea.
-- Archivo en UTF-8: ejecutar con sqlcmd -f 65001 para conservar las tildes.
-- =============================================================================

SET NOCOUNT ON;

DECLARE @Descripciones TABLE (
    Esquema     SYSNAME        NOT NULL,
    Tabla       SYSNAME        NOT NULL,
    Columna     SYSNAME        NULL,      -- NULL = descripcion de la tabla
    Descripcion NVARCHAR(500)  NOT NULL
);

INSERT INTO @Descripciones (Esquema, Tabla, Columna, Descripcion) VALUES
    -- Catalogo.MarcaModelo
    (N'Catalogo', N'MarcaModelo', N'MarcaModeloID', N'Identificador único de la combinación Marca/Modelo/Año'),
    (N'Catalogo', N'MarcaModelo', N'Marca',         N'Marca del vehículo (Toyota, Hyundai, etc.); fuente del autocompletado de Marca en la pantalla de Vehículos'),

    -- Catalogo.Parametros
    (N'Catalogo', N'Parametros', N'ParametroID', N'Identificador único del parámetro'),

    -- Catalogo.Roles
    (N'Catalogo', N'Roles', NULL,          N'Catálogo de roles del sistema para el control de acceso por perfil'),
    (N'Catalogo', N'Roles', N'RolID',      N'Identificador único del rol'),
    (N'Catalogo', N'Roles', N'Nombre',     N'Nombre técnico del rol, sin espacios ni tildes (Administrador, JefeMecanicos, Mecanico); viaja como claim de rol en el token JWT'),
    (N'Catalogo', N'Roles', N'Descripcion', N'Descripción legible de los permisos del rol'),

    -- Catalogo.Usuarios
    (N'Catalogo', N'Usuarios', NULL,             N'Usuarios que pueden iniciar sesión en el sistema, con su credencial y rol asignado'),
    (N'Catalogo', N'Usuarios', N'UsuarioID',     N'Identificador único del usuario'),
    (N'Catalogo', N'Usuarios', N'NombreUsuario', N'Nombre de inicio de sesión; único en el sistema'),
    (N'Catalogo', N'Usuarios', N'PasswordHash',  N'Hash BCrypt de la contraseña (incluye el salt); la contraseña nunca se guarda en texto plano'),
    (N'Catalogo', N'Usuarios', N'NombreCompleto', N'Nombre completo del usuario que se muestra en la interfaz'),
    (N'Catalogo', N'Usuarios', N'Email',         N'Correo electrónico del usuario (opcional)'),
    (N'Catalogo', N'Usuarios', N'Activo',        N'1 = puede iniciar sesión, 0 = deshabilitado (baja lógica en lugar de eliminar)'),
    (N'Catalogo', N'Usuarios', N'RolID',         N'Rol asignado al usuario; FK a Catalogo.Roles'),

    -- Sistema.Reparacion
    (N'Sistema', N'Reparacion', N'DuracionAproximadaHoras', N'Horas estimadas del trabajo (opcional); se pre-rellena desde Catalogo.TipoReparacion y alimenta el cálculo automático de FechaSalida de la orden'),

    -- Sistema.TokenRefresco
    (N'Sistema', N'TokenRefresco', NULL,                       N'Tokens de refresco de sesión, uno por dispositivo; permiten el re-login silencioso y el desbloqueo biométrico sin volver a digitar la contraseña'),
    (N'Sistema', N'TokenRefresco', N'TokenRefrescoID',          N'Identificador único del token de refresco'),
    (N'Sistema', N'TokenRefresco', N'UsuarioID',                N'Usuario dueño del token; FK a Catalogo.Usuarios'),
    (N'Sistema', N'TokenRefresco', N'TokenHash',                N'Hash del token de refresco; el token en claro solo lo conoce el dispositivo y nunca se guarda'),
    (N'Sistema', N'TokenRefresco', N'IdentificadorDispositivo', N'Identificador del dispositivo o navegador que solicitó el token (opcional)'),
    (N'Sistema', N'TokenRefresco', N'FechaCreacion',            N'Fecha y hora UTC en que se emitió el token'),
    (N'Sistema', N'TokenRefresco', N'FechaExpiracion',          N'Fecha y hora UTC a partir de la cual el token deja de ser válido'),
    (N'Sistema', N'TokenRefresco', N'Revocado',                 N'1 = el token fue invalidado antes de expirar (cierre de sesión o revocación), 0 = vigente'),
    (N'Sistema', N'TokenRefresco', N'FechaRevocado',            N'Fecha y hora UTC en que se revocó el token; NULL si no ha sido revocado');

DECLARE @Esquema SYSNAME, @Tabla SYSNAME, @Columna SYSNAME, @Descripcion NVARCHAR(500);

DECLARE cursorDescripciones CURSOR LOCAL FAST_FORWARD FOR
    SELECT Esquema, Tabla, Columna, Descripcion FROM @Descripciones;

OPEN cursorDescripciones;
FETCH NEXT FROM cursorDescripciones INTO @Esquema, @Tabla, @Columna, @Descripcion;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Nivel2Tipo VARCHAR(128) = CASE WHEN @Columna IS NULL THEN NULL ELSE 'COLUMN' END;

    IF EXISTS (SELECT 1 FROM sys.fn_listextendedproperty(N'MS_Description',
                   'SCHEMA', @Esquema, 'TABLE', @Tabla, @Nivel2Tipo, @Columna))
        EXEC sys.sp_updateextendedproperty
             @name = N'MS_Description', @value = @Descripcion,
             @level0type = N'SCHEMA', @level0name = @Esquema,
             @level1type = N'TABLE',  @level1name = @Tabla,
             @level2type = @Nivel2Tipo, @level2name = @Columna;
    ELSE
        EXEC sys.sp_addextendedproperty
             @name = N'MS_Description', @value = @Descripcion,
             @level0type = N'SCHEMA', @level0name = @Esquema,
             @level1type = N'TABLE',  @level1name = @Tabla,
             @level2type = @Nivel2Tipo, @level2name = @Columna;

    FETCH NEXT FROM cursorDescripciones INTO @Esquema, @Tabla, @Columna, @Descripcion;
END

CLOSE cursorDescripciones;
DEALLOCATE cursorDescripciones;
GO

-- Verificacion: debe devolver 0 filas
SELECT s.name AS Esquema, t.name AS Tabla, c.name AS Columna
FROM sys.columns c
JOIN sys.tables  t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name <> 'sysdiagrams'
  AND NOT EXISTS (SELECT 1 FROM sys.extended_properties ep
                  WHERE ep.class = 1 AND ep.major_id = c.object_id
                    AND ep.minor_id = c.column_id AND ep.name = 'MS_Description')
ORDER BY 1, 2, c.column_id;
GO

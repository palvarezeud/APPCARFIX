-- Tabla para refresh tokens (desbloqueo biometrico local + re-login silencioso)
-- Ver CLAUDE.md seccion 12.16 / plan de login biometrico

CREATE TABLE Sistema.TokenRefresco (
    TokenRefrescoID           INT IDENTITY(1,1) NOT NULL,
    UsuarioID                 INT NOT NULL,
    TokenHash                 VARCHAR(100) NOT NULL,
    IdentificadorDispositivo  VARCHAR(200) NULL,
    FechaCreacion             DATETIME NOT NULL CONSTRAINT DF_TokenRefresco_FechaCreacion DEFAULT (GETUTCDATE()),
    FechaExpiracion           DATETIME NOT NULL,
    Revocado                  BIT NOT NULL CONSTRAINT DF_TokenRefresco_Revocado DEFAULT ((0)),
    FechaRevocado             DATETIME NULL,
    CONSTRAINT PK_TokenRefresco PRIMARY KEY (TokenRefrescoID),
    CONSTRAINT FK_TokenRefresco_Usuarios FOREIGN KEY (UsuarioID)
        REFERENCES Catalogo.Usuarios (UsuarioID)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

CREATE UNIQUE INDEX UQ_TokenRefresco_TokenHash ON Sistema.TokenRefresco (TokenHash);
GO

CREATE INDEX IX_TokenRefresco_UsuarioID ON Sistema.TokenRefresco (UsuarioID);
GO

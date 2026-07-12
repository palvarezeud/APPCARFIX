namespace CarFix.Aplicacion.Features.Usuarios.Dtos;

public record UsuarioDto(
    int     UsuarioId,
    string  NombreUsuario,
    string  NombreCompleto,
    string? Email,
    bool    Activo,
    int     RolId,
    string  NombreRol
);

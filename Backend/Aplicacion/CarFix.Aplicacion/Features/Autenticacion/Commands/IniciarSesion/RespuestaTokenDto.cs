namespace CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;

public record RespuestaTokenDto(
    string   Token,
    DateTime Expiracion,
    string   TokenRefresco,
    DateTime ExpiracionTokenRefresco
);

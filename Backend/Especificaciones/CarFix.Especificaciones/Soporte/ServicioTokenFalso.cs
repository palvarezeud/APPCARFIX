using CarFix.Dominio.Interfaces;

namespace CarFix.Especificaciones.Soporte;

public class ServicioTokenFalso : IServicioToken
{
    public (string Token, DateTime Expiracion) GenerarToken(int usuarioId, string nombreUsuario, string rol)
        => ($"token-falso-{usuarioId}-{rol}", DateTime.UtcNow.AddHours(8));
}

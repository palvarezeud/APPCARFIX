using CarFix.Dominio.Interfaces;

namespace CarFix.Especificaciones.Soporte;

public class ServicioTokenFalso : IServicioToken
{
    public string GenerarToken(int usuarioId, string nombreUsuario, string rol)
        => $"token-falso-{usuarioId}-{rol}";
}

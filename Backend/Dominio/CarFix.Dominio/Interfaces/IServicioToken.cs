namespace CarFix.Dominio.Interfaces;

public interface IServicioToken
{
    (string Token, DateTime Expiracion) GenerarToken(int usuarioId, string nombreUsuario, string rol);
}

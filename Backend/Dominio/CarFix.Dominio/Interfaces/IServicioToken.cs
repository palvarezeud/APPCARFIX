namespace CarFix.Dominio.Interfaces;

public interface IServicioToken
{
    string GenerarToken(int usuarioId, string nombreUsuario, string rol);
}

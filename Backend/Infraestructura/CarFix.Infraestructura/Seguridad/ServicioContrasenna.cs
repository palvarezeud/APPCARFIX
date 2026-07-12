using CarFix.Dominio.Interfaces;

namespace CarFix.Infraestructura.Seguridad;

public class ServicioContrasenna : IServicioContrasenna
{
    public bool   Verificar(string contrasenna, string hash) => BCrypt.Net.BCrypt.Verify(contrasenna, hash);
    public string Hashear(string contrasenna)                => BCrypt.Net.BCrypt.HashPassword(contrasenna);
}

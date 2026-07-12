namespace CarFix.Dominio.Interfaces;

public interface IServicioContrasenna
{
    bool   Verificar(string contrasenna, string hash);
    string Hashear(string contrasenna);
}

using System.Security.Cryptography;
using System.Text;
using CarFix.Dominio.Interfaces;

namespace CarFix.Infraestructura.Seguridad;

public class ServicioHashToken : IServicioHashToken
{
    public string Hashear(string valor)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}

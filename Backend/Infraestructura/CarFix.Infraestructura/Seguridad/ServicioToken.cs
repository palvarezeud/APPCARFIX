using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarFix.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CarFix.Infraestructura.Seguridad;

public class ServicioToken : IServicioToken
{
    private readonly IConfiguration _config;

    public ServicioToken(IConfiguration config) => _config = config;

    public (string Token, DateTime Expiracion) GenerarToken(int usuarioId, string nombreUsuario, string rol)
    {
        var llave        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Llave"]!));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
        var expiracion   = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiracionMinutos"]!));

        var reclamaciones = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Name,            nombreUsuario),
            new Claim(ClaimTypes.Role,            rol)
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Emisor"],
            audience:           _config["Jwt:Audiencia"],
            claims:             reclamaciones,
            expires:            expiracion,
            signingCredentials: credenciales
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}

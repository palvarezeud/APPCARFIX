using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;

public class IniciarSesionHandler : IRequestHandler<IniciarSesionCommand, Resultado<RespuestaTokenDto>>
{
    private readonly ICarFixDbContext      _contexto;
    private readonly IServicioToken        _servicioToken;
    private readonly IServicioContrasenna  _servicioContrasenna;

    public IniciarSesionHandler(
        ICarFixDbContext      contexto,
        IServicioToken        servicioToken,
        IServicioContrasenna  servicioContrasenna)
    {
        _contexto            = contexto;
        _servicioToken       = servicioToken;
        _servicioContrasenna = servicioContrasenna;
    }

    public async Task<Resultado<RespuestaTokenDto>> Handle(IniciarSesionCommand cmd, CancellationToken ct)
    {
        var usuario = await _contexto.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == cmd.NombreUsuario && u.Activo, ct);

        if (usuario is null || !_servicioContrasenna.Verificar(cmd.Password, usuario.PasswordHash))
            return Resultado<RespuestaTokenDto>.Fallo("Credenciales invalidas.");

        var expiracion = DateTime.UtcNow.AddMinutes(60);
        var token      = _servicioToken.GenerarToken(usuario.UsuarioId, usuario.NombreUsuario, usuario.Rol!.Nombre);

        return Resultado<RespuestaTokenDto>.Exito(new RespuestaTokenDto(token, expiracion));
    }
}

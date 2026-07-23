using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;

public class IniciarSesionHandler : IRequestHandler<IniciarSesionCommand, Resultado<RespuestaTokenDto>>
{
    private readonly ICarFixDbContext         _contexto;
    private readonly IServicioToken           _servicioToken;
    private readonly IServicioContrasenna     _servicioContrasenna;
    private readonly IServicioHashToken       _servicioHashToken;
    private readonly IRepositorioTokenRefresco _repositorioTokenRefresco;
    private readonly IUnidadTrabajo           _unidadTrabajo;

    public IniciarSesionHandler(
        ICarFixDbContext          contexto,
        IServicioToken            servicioToken,
        IServicioContrasenna      servicioContrasenna,
        IServicioHashToken        servicioHashToken,
        IRepositorioTokenRefresco repositorioTokenRefresco,
        IUnidadTrabajo            unidadTrabajo)
    {
        _contexto                 = contexto;
        _servicioToken             = servicioToken;
        _servicioContrasenna       = servicioContrasenna;
        _servicioHashToken         = servicioHashToken;
        _repositorioTokenRefresco  = repositorioTokenRefresco;
        _unidadTrabajo             = unidadTrabajo;
    }

    public async Task<Resultado<RespuestaTokenDto>> Handle(IniciarSesionCommand cmd, CancellationToken ct)
    {
        var usuario = await _contexto.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == cmd.NombreUsuario && u.Activo, ct);

        if (usuario is null || !_servicioContrasenna.Verificar(cmd.Password, usuario.PasswordHash))
            return Resultado<RespuestaTokenDto>.Fallo("Credenciales invalidas.");

        var (token, expiracion) = _servicioToken.GenerarToken(usuario.UsuarioId, usuario.NombreUsuario, usuario.Rol!.Nombre);

        var tokenRefrescoPlano = GeneradorTokenRefresco.Generar();
        var expiracionRefresco = DateTime.UtcNow.AddDays(GeneradorTokenRefresco.DiasVigencia);

        await _repositorioTokenRefresco.AgregarAsync(new TokenRefresco
        {
            UsuarioId       = usuario.UsuarioId,
            TokenHash       = _servicioHashToken.Hashear(tokenRefrescoPlano),
            FechaExpiracion = expiracionRefresco,
            Revocado        = false
        }, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<RespuestaTokenDto>.Exito(
            new RespuestaTokenDto(token, expiracion, tokenRefrescoPlano, expiracionRefresco));
    }
}

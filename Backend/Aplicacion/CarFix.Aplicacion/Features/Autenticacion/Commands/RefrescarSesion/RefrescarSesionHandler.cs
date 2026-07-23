using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RefrescarSesion;

public class RefrescarSesionHandler : IRequestHandler<RefrescarSesionCommand, Resultado<RespuestaTokenDto>>
{
    private readonly IServicioToken            _servicioToken;
    private readonly IServicioHashToken        _servicioHashToken;
    private readonly IRepositorioTokenRefresco _repositorioTokenRefresco;
    private readonly IUnidadTrabajo            _unidadTrabajo;

    public RefrescarSesionHandler(
        IServicioToken            servicioToken,
        IServicioHashToken        servicioHashToken,
        IRepositorioTokenRefresco repositorioTokenRefresco,
        IUnidadTrabajo            unidadTrabajo)
    {
        _servicioToken            = servicioToken;
        _servicioHashToken        = servicioHashToken;
        _repositorioTokenRefresco = repositorioTokenRefresco;
        _unidadTrabajo            = unidadTrabajo;
    }

    public async Task<Resultado<RespuestaTokenDto>> Handle(RefrescarSesionCommand cmd, CancellationToken ct)
    {
        var hash   = _servicioHashToken.Hashear(cmd.TokenRefresco);
        var actual = await _repositorioTokenRefresco.ObtenerPorHashAsync(hash, ct);

        if (actual is null || actual.Revocado || actual.FechaExpiracion < DateTime.UtcNow)
            return Resultado<RespuestaTokenDto>.Fallo("Token de refresco invalido o vencido.");

        actual.Revocado      = true;
        actual.FechaRevocado = DateTime.UtcNow;

        var (token, expiracion) = _servicioToken.GenerarToken(
            actual.Usuario.UsuarioId, actual.Usuario.NombreUsuario, actual.Usuario.Rol!.Nombre);

        var tokenRefrescoPlano = GeneradorTokenRefresco.Generar();
        var expiracionRefresco = DateTime.UtcNow.AddDays(GeneradorTokenRefresco.DiasVigencia);

        await _repositorioTokenRefresco.AgregarAsync(new TokenRefresco
        {
            UsuarioId       = actual.UsuarioId,
            TokenHash       = _servicioHashToken.Hashear(tokenRefrescoPlano),
            FechaExpiracion = expiracionRefresco,
            Revocado        = false
        }, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<RespuestaTokenDto>.Exito(
            new RespuestaTokenDto(token, expiracion, tokenRefrescoPlano, expiracionRefresco));
    }
}

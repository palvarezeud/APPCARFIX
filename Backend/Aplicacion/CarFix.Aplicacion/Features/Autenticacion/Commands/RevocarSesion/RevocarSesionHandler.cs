using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RevocarSesion;

public class RevocarSesionHandler : IRequestHandler<RevocarSesionCommand, Resultado>
{
    private readonly IServicioHashToken        _servicioHashToken;
    private readonly IRepositorioTokenRefresco _repositorioTokenRefresco;
    private readonly IUnidadTrabajo            _unidadTrabajo;

    public RevocarSesionHandler(
        IServicioHashToken        servicioHashToken,
        IRepositorioTokenRefresco repositorioTokenRefresco,
        IUnidadTrabajo            unidadTrabajo)
    {
        _servicioHashToken        = servicioHashToken;
        _repositorioTokenRefresco = repositorioTokenRefresco;
        _unidadTrabajo            = unidadTrabajo;
    }

    public async Task<Resultado> Handle(RevocarSesionCommand cmd, CancellationToken ct)
    {
        var hash   = _servicioHashToken.Hashear(cmd.TokenRefresco);
        var actual = await _repositorioTokenRefresco.ObtenerPorHashAsync(hash, ct);

        if (actual is not null && !actual.Revocado)
        {
            actual.Revocado      = true;
            actual.FechaRevocado = DateTime.UtcNow;
            await _unidadTrabajo.GuardarCambiosAsync(ct);
        }

        return Resultado.Exito();
    }
}

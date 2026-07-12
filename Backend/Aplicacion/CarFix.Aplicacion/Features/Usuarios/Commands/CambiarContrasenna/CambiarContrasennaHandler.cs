using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CambiarContrasenna;

public class CambiarContrasennaHandler : IRequestHandler<CambiarContrasennaCommand, Resultado>
{
    private readonly IRepositorioUsuario  _repositorio;
    private readonly IUnidadTrabajo       _unidadTrabajo;
    private readonly IServicioContrasenna _servicioContrasenna;

    public CambiarContrasennaHandler(
        IRepositorioUsuario  repositorio,
        IUnidadTrabajo       unidadTrabajo,
        IServicioContrasenna servicioContrasenna)
    {
        _repositorio         = repositorio;
        _unidadTrabajo       = unidadTrabajo;
        _servicioContrasenna = servicioContrasenna;
    }

    public async Task<Resultado> Handle(CambiarContrasennaCommand cmd, CancellationToken ct)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Resultado.Fallo("Usuario no encontrado.");

        usuario.PasswordHash = _servicioContrasenna.Hashear(cmd.NuevoPassword);
        _repositorio.Actualizar(usuario);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}

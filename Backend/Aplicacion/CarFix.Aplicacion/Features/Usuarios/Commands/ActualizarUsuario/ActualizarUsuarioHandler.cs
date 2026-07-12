using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioHandler : IRequestHandler<ActualizarUsuarioCommand, Resultado>
{
    private readonly IRepositorioUsuario _repositorio;
    private readonly IUnidadTrabajo      _unidadTrabajo;

    public ActualizarUsuarioHandler(IRepositorioUsuario repositorio, IUnidadTrabajo unidadTrabajo)
    {
        _repositorio   = repositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Resultado.Fallo("Usuario no encontrado.");

        usuario.NombreCompleto = cmd.NombreCompleto;
        usuario.Email          = cmd.Email;
        usuario.RolId          = cmd.RolId;
        usuario.Activo         = cmd.Activo;

        _repositorio.Actualizar(usuario);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}

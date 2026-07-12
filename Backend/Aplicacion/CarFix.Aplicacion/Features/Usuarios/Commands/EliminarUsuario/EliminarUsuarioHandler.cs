using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.EliminarUsuario;

public class EliminarUsuarioHandler : IRequestHandler<EliminarUsuarioCommand, Resultado>
{
    private readonly IRepositorioUsuario _repositorio;
    private readonly IUnidadTrabajo      _unidadTrabajo;

    public EliminarUsuarioHandler(IRepositorioUsuario repositorio, IUnidadTrabajo unidadTrabajo)
    {
        _repositorio   = repositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarUsuarioCommand cmd, CancellationToken ct)
    {
        if (cmd.UsuarioId == cmd.UsuarioActualId)
            return Resultado.Fallo("No puedes eliminar tu propio usuario.");

        var usuario = await _repositorio.ObtenerPorIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Resultado.Fallo("Usuario no encontrado.");

        _repositorio.Eliminar(usuario);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}

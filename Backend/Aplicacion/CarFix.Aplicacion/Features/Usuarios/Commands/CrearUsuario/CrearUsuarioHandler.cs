using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioHandler : IRequestHandler<CrearUsuarioCommand, Resultado<int>>
{
    private readonly IRepositorioUsuario  _repositorio;
    private readonly ICarFixDbContext     _contexto;
    private readonly IUnidadTrabajo       _unidadTrabajo;
    private readonly IServicioContrasenna _servicioContrasenna;

    public CrearUsuarioHandler(
        IRepositorioUsuario  repositorio,
        ICarFixDbContext     contexto,
        IUnidadTrabajo       unidadTrabajo,
        IServicioContrasenna servicioContrasenna)
    {
        _repositorio         = repositorio;
        _contexto            = contexto;
        _unidadTrabajo       = unidadTrabajo;
        _servicioContrasenna = servicioContrasenna;
    }

    public async Task<Resultado<int>> Handle(CrearUsuarioCommand cmd, CancellationToken ct)
    {
        var existeNombre = await _contexto.Usuarios
            .AnyAsync(u => u.NombreUsuario == cmd.NombreUsuario, ct);

        if (existeNombre)
            return Resultado<int>.Fallo($"El nombre de usuario '{cmd.NombreUsuario}' ya esta en uso.");

        var usuario = new Usuario
        {
            NombreUsuario  = cmd.NombreUsuario,
            PasswordHash   = _servicioContrasenna.Hashear(cmd.Password),
            NombreCompleto = cmd.NombreCompleto,
            Email          = cmd.Email,
            RolId          = cmd.RolId,
            Activo         = cmd.Activo
        };

        await _repositorio.AgregarAsync(usuario, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(usuario.UsuarioId);
    }
}

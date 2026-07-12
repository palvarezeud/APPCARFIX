using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Usuarios.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Usuarios.Queries.ObtenerUsuarios;

public class ObtenerUsuariosHandler : IRequestHandler<ObtenerUsuariosQuery, Resultado<IEnumerable<UsuarioDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerUsuariosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<UsuarioDto>>> Handle(ObtenerUsuariosQuery query, CancellationToken ct)
    {
        var resultado = await _contexto.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioDto(
                u.UsuarioId,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Email,
                u.Activo,
                u.RolId,
                u.Rol.Nombre))
            .ToListAsync(ct);

        return Resultado<IEnumerable<UsuarioDto>>.Exito(resultado);
    }
}

using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Usuarios.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Queries.ObtenerUsuarios;

public record ObtenerUsuariosQuery : IRequest<Resultado<IEnumerable<UsuarioDto>>>;

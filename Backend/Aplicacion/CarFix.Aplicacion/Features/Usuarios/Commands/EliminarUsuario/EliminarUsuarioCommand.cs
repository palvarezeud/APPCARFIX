using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.EliminarUsuario;

public record EliminarUsuarioCommand(int UsuarioId, int UsuarioActualId) : IRequest<Resultado>;

using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CambiarContrasenna;

public record CambiarContrasennaCommand(int UsuarioId, string NuevoPassword) : IRequest<Resultado>;

using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Parametros.Commands.EliminarParametro;

public record EliminarParametroCommand(int ParametroId) : IRequest<Resultado>;

using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Parametros.Commands.CrearParametro;

public record CrearParametroCommand(
    string Nombre,
    string Valor
) : IRequest<Resultado<int>>;

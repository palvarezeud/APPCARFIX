using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Parametros.Commands.ActualizarParametro;

public record ActualizarParametroCommand(
    int    ParametroId,
    string Nombre,
    string Valor
) : IRequest<Resultado>;

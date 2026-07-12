using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.MarcarIncluidoRepuesto;

public record MarcarIncluidoRepuestoCommand(
    int  RepuestoId,
    bool Incluido
) : IRequest<Resultado>;

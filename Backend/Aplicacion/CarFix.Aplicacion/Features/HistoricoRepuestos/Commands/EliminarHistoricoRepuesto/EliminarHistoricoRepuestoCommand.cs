using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.EliminarHistoricoRepuesto;

public record EliminarHistoricoRepuestoCommand(int RespuestoHistoricoId) : IRequest<Resultado>;

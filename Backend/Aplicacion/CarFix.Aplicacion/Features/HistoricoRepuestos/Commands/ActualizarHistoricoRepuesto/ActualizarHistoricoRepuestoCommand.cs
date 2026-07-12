using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.ActualizarHistoricoRepuesto;

public record ActualizarHistoricoRepuestoCommand(
    int      RespuestoHistoricoId,
    string   Marca,
    string   Modelo,
    int      Annio,
    string   RepuestoDecripcion,
    decimal  Precio,
    string   Repuestera,
    DateTime FechaCompra
) : IRequest<Resultado>;

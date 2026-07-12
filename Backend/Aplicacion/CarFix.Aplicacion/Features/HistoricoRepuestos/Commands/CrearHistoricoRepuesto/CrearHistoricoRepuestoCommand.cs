using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.CrearHistoricoRepuesto;

public record CrearHistoricoRepuestoCommand(
    string   Marca,
    string   Modelo,
    int      Annio,
    string   RepuestoDecripcion,
    decimal  Precio,
    string   Repuestera,
    DateTime FechaCompra
) : IRequest<Resultado<int>>;

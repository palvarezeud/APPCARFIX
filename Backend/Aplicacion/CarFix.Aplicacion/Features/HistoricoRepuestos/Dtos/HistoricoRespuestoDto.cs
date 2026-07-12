namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Dtos;

public record HistoricoRespuestoDto(
    int      RespuestoHistoricoId,
    string   Marca,
    string   Modelo,
    int      Annio,
    string   RepuestoDecripcion,
    decimal  Precio,
    string   Repuestera,
    DateTime FechaCompra
);

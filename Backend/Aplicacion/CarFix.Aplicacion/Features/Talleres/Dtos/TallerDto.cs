namespace CarFix.Aplicacion.Features.Talleres.Dtos;

public record TallerDto(
    int      TallerId,
    string   Nombre,
    string   UbicacionDescripcion,
    string   Telefonos,
    string   Email,
    decimal? Latitud,
    decimal? Longitud
);

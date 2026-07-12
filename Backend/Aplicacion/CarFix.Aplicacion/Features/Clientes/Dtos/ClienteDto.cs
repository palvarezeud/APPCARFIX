namespace CarFix.Aplicacion.Features.Clientes.Dtos;

public record ClienteDto(
    int     ClienteId,
    string  NombreCliente,
    string  Telefono1,
    string? Telefono2,
    string? Email,
    bool    EsEmpresa
);

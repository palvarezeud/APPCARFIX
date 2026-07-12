using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Clientes.Commands.ActualizarCliente;

public record ActualizarClienteCommand(
    int     ClienteId,
    string  NombreCliente,
    string  Telefono1,
    string? Telefono2,
    string? Email,
    bool    EsEmpresa
) : IRequest<Resultado>;

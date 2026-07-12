using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Clientes.Commands.CrearCliente;

public record CrearClienteCommand(
    string  NombreCliente,
    string  Telefono1,
    string? Telefono2,
    string? Email,
    bool    EsEmpresa
) : IRequest<Resultado<int>>;

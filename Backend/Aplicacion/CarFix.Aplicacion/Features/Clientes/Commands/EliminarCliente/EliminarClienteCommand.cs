using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Clientes.Commands.EliminarCliente;

public record EliminarClienteCommand(int ClienteId) : IRequest<Resultado>;

using FluentValidation;

namespace CarFix.Aplicacion.Features.Clientes.Commands.CrearCliente;

public class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteValidator()
    {
        RuleFor(x => x.NombreCliente)
            .NotEmpty().WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(200);

        RuleFor(x => x.Telefono1)
            .NotEmpty().WithMessage("El telefono principal es requerido.")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene un formato valido.");
    }
}

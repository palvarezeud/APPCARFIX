using FluentValidation;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.AgregarRepuesto;

public class AgregarRepuestoValidator : AbstractValidator<AgregarRepuestoCommand>
{
    public AgregarRepuestoValidator()
    {
        RuleFor(x => x.FacturaId)
            .GreaterThan(0).WithMessage("La factura es requerida.");

        RuleFor(x => x.NombreRepuesto)
            .NotEmpty().WithMessage("El nombre del repuesto es requerido.")
            .MaximumLength(200);

        RuleFor(x => x.Costo)
            .GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.");

        RuleFor(x => x.Repuestera)
            .NotEmpty().WithMessage("El nombre de la repuestera es requerido.");
    }
}

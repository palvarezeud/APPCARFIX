using FluentValidation;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.AgregarReparacion;

public class AgregarReparacionValidator : AbstractValidator<AgregarReparacionCommand>
{
    public AgregarReparacionValidator()
    {
        RuleFor(x => x.FacturaId)
            .GreaterThan(0).WithMessage("La factura es requerida.");

        RuleFor(x => x.DescripcionReparacion)
            .NotEmpty().WithMessage("La descripcion de la reparacion es requerida.")
            .MaximumLength(500);

        RuleFor(x => x.Costo)
            .GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.");

        RuleFor(x => x.DuracionAproximadaHoras)
            .GreaterThan(0).When(x => x.DuracionAproximadaHoras.HasValue)
            .WithMessage("La duracion debe ser mayor a 0 horas.");
    }
}

using FluentValidation;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.CrearTipoReparacion;

public class CrearTipoReparacionValidator : AbstractValidator<CrearTipoReparacionCommand>
{
    public CrearTipoReparacionValidator()
    {
        RuleFor(x => x.DescripcionReparacion)
            .NotEmpty().WithMessage("La descripcion es requerida.")
            .MaximumLength(200);

        RuleFor(x => x.DuracionAproximadaHoras)
            .GreaterThanOrEqualTo(1).WithMessage("La duracion minima es 1 hora.");

        RuleFor(x => x.CostoBase)
            .GreaterThanOrEqualTo(0).WithMessage("El costo base no puede ser negativo.");
    }
}

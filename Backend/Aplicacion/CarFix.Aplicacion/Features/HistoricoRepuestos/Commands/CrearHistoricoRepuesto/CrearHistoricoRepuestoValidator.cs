using FluentValidation;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.CrearHistoricoRepuesto;

public class CrearHistoricoRepuestoValidator : AbstractValidator<CrearHistoricoRepuestoCommand>
{
    public CrearHistoricoRepuestoValidator()
    {
        RuleFor(x => x.Marca).NotEmpty().WithMessage("La marca es requerida.").MaximumLength(100);
        RuleFor(x => x.Modelo).NotEmpty().WithMessage("El modelo es requerido.").MaximumLength(100);
        RuleFor(x => x.Annio).GreaterThan(1900).WithMessage("El anno debe ser mayor a 1900.");
        RuleFor(x => x.RepuestoDecripcion).NotEmpty().WithMessage("La descripcion del repuesto es requerida.").MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
        RuleFor(x => x.Repuestera).NotEmpty().WithMessage("La repuestera es requerida.").MaximumLength(150);
    }
}

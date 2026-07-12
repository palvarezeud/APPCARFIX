using FluentValidation;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CambiarEstadoOrden;

public class CambiarEstadoOrdenValidator : AbstractValidator<CambiarEstadoOrdenCommand>
{
    public CambiarEstadoOrdenValidator()
    {
        RuleFor(x => x.OrdenServicioId)
            .GreaterThan(0).WithMessage("El identificador de la orden es requerido.");

        RuleFor(x => x.NuevoEstadoId)
            .InclusiveBetween(1, 5).WithMessage("Estado de orden no valido. Los valores permitidos son del 1 al 5.");
    }
}

using FluentValidation;

namespace CarFix.Aplicacion.Features.Facturas.Commands.CambiarEstadoFactura;

public class CambiarEstadoFacturaValidator : AbstractValidator<CambiarEstadoFacturaCommand>
{
    public CambiarEstadoFacturaValidator()
    {
        RuleFor(x => x.FacturaId)
            .GreaterThan(0).WithMessage("El identificador de la factura es requerido.");

        RuleFor(x => x.NuevoEstadoId)
            .InclusiveBetween(1, 3).WithMessage("Estado de factura no valido. Los valores permitidos son del 1 al 3.");
    }
}

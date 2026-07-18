using FluentValidation;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.EscanearFacturaRepuesto;

public class EscanearFacturaRepuestoValidator : AbstractValidator<EscanearFacturaRepuestoCommand>
{
    private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];
    private const long TamannoMaximoBytes = 10 * 1024 * 1024;

    public EscanearFacturaRepuestoValidator()
    {
        RuleFor(x => x.ImagenBytes)
            .NotEmpty().WithMessage("La imagen es requerida.")
            .Must(b => b.Length <= TamannoMaximoBytes).WithMessage("La imagen no debe superar 10 MB.");

        RuleFor(x => x.TipoContenido)
            .Must(t => TiposPermitidos.Contains(t.ToLowerInvariant()))
            .WithMessage("Formato de imagen no soportado. Use JPEG, PNG o WEBP.");
    }
}

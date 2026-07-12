using FluentValidation;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.EscanearTarjetaCirculacion;

public class EscanearTarjetaCirculacionValidator : AbstractValidator<EscanearTarjetaCirculacionCommand>
{
    private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];
    private const long TamannoMaximoBytes = 10 * 1024 * 1024;

    public EscanearTarjetaCirculacionValidator()
    {
        RuleFor(x => x.ImagenBytes)
            .NotEmpty().WithMessage("La imagen es requerida.")
            .Must(b => b.Length <= TamannoMaximoBytes).WithMessage("La imagen no debe superar 10 MB.");

        RuleFor(x => x.TipoContenido)
            .Must(t => TiposPermitidos.Contains(t.ToLowerInvariant()))
            .WithMessage("Formato de imagen no soportado. Use JPEG, PNG o WEBP.");
    }
}

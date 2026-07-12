using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.AsistenteVoz.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.AsistenteVoz.Commands.InterpretarComandoVoz;

public record InterpretarComandoVozCommand(
    string  Transcripcion,
    string? PantallaActual,
    string? IntentEnProgreso
) : IRequest<Resultado<InterpretacionVozDto>>;

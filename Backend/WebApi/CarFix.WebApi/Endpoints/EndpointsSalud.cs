using CarFix.Aplicacion.Features.Salud.Queries.VerificarSalud;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsSalud
{
    public static void MapearSalud(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/salud")
            .WithTags("Salud")
            .AllowAnonymous();

        grupo.MapGet("/",
            async Task<Ok<bool>>
            (ISender sender) =>
            {
                var resultado = await sender.Send(new VerificarSaludQuery());
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("VerificarSalud")
            .WithSummary("Toca la base de datos para activarla si esta pausada (Azure SQL Serverless); sin autenticacion, pensado para llamarse al cargar el frontend");
    }
}

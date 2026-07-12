using CarFix.Aplicacion.Features.MarcasModelos.Dtos;
using CarFix.Aplicacion.Features.MarcasModelos.Queries.ObtenerMarcasModelos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsMarcasModelos
{
    public static void MapearMarcasModelos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/marcas-modelos")
            .WithTags("Marcas y modelos")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<MarcaModeloDto>>> (ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerMarcasModelosQuery());
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerMarcasModelos")
            .WithSummary("Catalogo de Marca/Modelo/Annio para autocompletar el formulario de Vehiculos");
    }
}

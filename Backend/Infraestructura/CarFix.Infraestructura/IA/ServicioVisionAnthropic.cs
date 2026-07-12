using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using CarFix.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarFix.Infraestructura.IA;

public class ServicioVisionAnthropic : IServicioVisionVehiculo
{
    private readonly AnthropicClient _cliente;
    private readonly IConfiguration  _config;
    private readonly ILogger<ServicioVisionAnthropic> _logger;

    private const string InstruccionSistema =
        "Eres un asistente que extrae datos de tarjetas de circulacion vehicular " +
        "fotografiadas por un mecanico de taller. Devuelve SOLO los campos solicitados " +
        "en el formato indicado. Si un campo no es legible con confianza, usa null " +
        "en vez de inventar un valor." +
        "es posible que la imagen este borrosa, cortada o con reflejos. No inventes datos. " +
        "es posible que el modelo sea el año del vehículo, el modelo a veces es la línea del vehículo, y el año a veces es el año de fabricación. ";


    private static readonly Dictionary<string, JsonElement> EsquemaJson =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""
        {
          "type": "object",
          "properties": {
            "marca":  { "type": ["string", "null"] },
            "modelo": { "type": ["string", "null"] },
            "annio":  { "type": ["integer", "null"] },
            "vin":    { "type": ["string", "null"] },
            "placa":  { "type": ["string", "null"] },
            "motor":  { "type": ["string", "null"] }
          },
          "required": ["marca", "modelo", "annio", "vin", "placa", "motor"],
          "additionalProperties": false
        }
        """)!;

    public ServicioVisionAnthropic(
        AnthropicClient cliente, IConfiguration config, ILogger<ServicioVisionAnthropic> logger)
    {
        _cliente = cliente;
        _config  = config;
        _logger  = logger;
    }

    public async Task<ExtraccionVehiculoResultado> ExtraerDatosAsync(
        byte[] imagenBytes, string tipoContenido, CancellationToken ct = default)
    {
        try
        {
            var base64 = Convert.ToBase64String(imagenBytes);
            var modelo = _config["Anthropic:Modelo"] ?? "claude-sonnet-5";

            var parametros = new MessageCreateParams
            {
                Model     = modelo,
                MaxTokens = 1024,
                System    = InstruccionSistema,
                OutputConfig = new OutputConfig
                {
                    Format = new JsonOutputFormat { Schema = EsquemaJson }
                },
                Messages =
                [
                    new()
                    {
                        Role = Role.User,
                        Content = new List<ContentBlockParam>
                        {
                            new ImageBlockParam
                            {
                                Source = new Base64ImageSource
                                {
                                    MediaType = MapearMediaType(tipoContenido),
                                    Data      = base64
                                }
                            },
                            new TextBlockParam
                            {
                                Text = "Extrae marca, modelo, annio, vin, placa y motor " +
                                       "de esta tarjeta de circulacion vehicular."
                            }
                        }
                    }
                ]
            };

            var respuesta = await _cliente.Messages.Create(parametros, cancellationToken: ct);

            if (respuesta.StopReason == "refusal")
            {
                _logger.LogWarning("Anthropic rechazo la solicitud de extraccion de vehiculo.");
                return ExtraccionVehiculoResultado.Fallo(
                    "La IA no pudo procesar esta imagen. Complete los datos manualmente.");
            }

            var texto = respuesta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                _logger.LogWarning("Anthropic no devolvio texto en la extraccion de vehiculo.");
                return ExtraccionVehiculoResultado.Fallo(
                    "No se pudo leer la imagen. Complete los datos manualmente.");
            }

            using var doc  = JsonDocument.Parse(texto);
            var raiz = doc.RootElement;

            return ExtraccionVehiculoResultado.Exito(
                LeerString(raiz, "marca"),
                LeerString(raiz, "modelo"),
                LeerAnnio(raiz),
                LeerString(raiz, "vin"),
                LeerString(raiz, "placa"),
                LeerString(raiz, "motor"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar al servicio de vision de Anthropic.");
            return ExtraccionVehiculoResultado.Fallo(
                "El servicio de escaneo no esta disponible en este momento. " +
                "Complete los datos manualmente.");
        }
    }

    private static string? LeerString(JsonElement raiz, string propiedad) =>
        raiz.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static short? LeerAnnio(JsonElement raiz) =>
        raiz.TryGetProperty("annio", out var valor) && valor.ValueKind == JsonValueKind.Number
            ? valor.GetInt16()
            : null;

    private static MediaType MapearMediaType(string tipoContenido) => tipoContenido switch
    {
        "image/png"  => MediaType.ImagePng,
        "image/webp" => MediaType.ImageWebP,
        "image/gif"  => MediaType.ImageGif,
        _            => MediaType.ImageJpeg
    };
}

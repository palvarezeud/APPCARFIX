using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using CarFix.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarFix.Infraestructura.IA;

public class ServicioVisionFacturaRepuestoAnthropic : IServicioVisionFacturaRepuesto
{
    private readonly AnthropicClient _cliente;
    private readonly IConfiguration  _config;
    private readonly ILogger<ServicioVisionFacturaRepuestoAnthropic> _logger;

    private const string InstruccionSistema =
        "Eres un asistente que extrae datos de facturas de proveedores de repuestos " +
        "fotografiadas por un mecanico de taller. Devuelve SOLO los campos solicitados " +
        "en el formato indicado. Si un campo no es legible con confianza, usa null " +
        "en vez de inventar un valor. " +
        "es posible que la imagen este borrosa, cortada o con reflejos. No inventes datos. " +
        "'repuestos' debe ser la lista con el nombre/descripcion de cada repuesto o pieza " +
        "que aparece en la factura (uno por linea de detalle). 'montoTotal' es el monto TOTAL " +
        "de la factura (el total general a pagar), no la suma de un solo repuesto.";

    private static readonly Dictionary<string, JsonElement> EsquemaJson =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""
        {
          "type": "object",
          "properties": {
            "repuestos":     { "type": ["array", "null"], "items": { "type": "string" } },
            "montoTotal":    { "type": ["number", "null"] },
            "fecha":         { "type": ["string", "null"] },
            "repuestera":    { "type": ["string", "null"] },
            "numeroFactura": { "type": ["string", "null"] }
          },
          "required": ["repuestos", "montoTotal", "fecha", "repuestera", "numeroFactura"],
          "additionalProperties": false
        }
        """)!;

    public ServicioVisionFacturaRepuestoAnthropic(
        AnthropicClient cliente, IConfiguration config, ILogger<ServicioVisionFacturaRepuestoAnthropic> logger)
    {
        _cliente = cliente;
        _config  = config;
        _logger  = logger;
    }

    public async Task<ExtraccionFacturaRepuestoResultado> ExtraerDatosAsync(
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
                                Text = "Extrae los repuestos, el monto total, la fecha, la repuestera " +
                                       "(tienda o proveedor) y el numero de factura de esta factura de repuestos."
                            }
                        }
                    }
                ]
            };

            var respuesta = await _cliente.Messages.Create(parametros, cancellationToken: ct);

            if (respuesta.StopReason == "refusal")
            {
                _logger.LogWarning("Anthropic rechazo la solicitud de extraccion de factura de repuesto.");
                return ExtraccionFacturaRepuestoResultado.Fallo(
                    "La IA no pudo procesar esta imagen. Complete los datos manualmente.");
            }

            var texto = respuesta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                _logger.LogWarning("Anthropic no devolvio texto en la extraccion de factura de repuesto.");
                return ExtraccionFacturaRepuestoResultado.Fallo(
                    "No se pudo leer la imagen. Complete los datos manualmente.");
            }

            using var doc  = JsonDocument.Parse(texto);
            var raiz = doc.RootElement;

            return ExtraccionFacturaRepuestoResultado.Exito(
                LeerListaStrings(raiz, "repuestos"),
                LeerDecimal(raiz, "montoTotal"),
                LeerString(raiz, "fecha"),
                LeerString(raiz, "repuestera"),
                LeerString(raiz, "numeroFactura"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar al servicio de vision de Anthropic.");
            return ExtraccionFacturaRepuestoResultado.Fallo(
                "El servicio de escaneo no esta disponible en este momento. " +
                "Complete los datos manualmente.");
        }
    }

    private static string? LeerString(JsonElement raiz, string propiedad) =>
        raiz.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static decimal? LeerDecimal(JsonElement raiz, string propiedad) =>
        raiz.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.Number
            ? valor.GetDecimal()
            : null;

    private static List<string>? LeerListaStrings(JsonElement raiz, string propiedad)
    {
        if (!raiz.TryGetProperty(propiedad, out var valor) || valor.ValueKind != JsonValueKind.Array)
            return null;

        var lista = valor.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString()!)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return lista.Count > 0 ? lista : null;
    }

    private static MediaType MapearMediaType(string tipoContenido) => tipoContenido switch
    {
        "image/png"  => MediaType.ImagePng,
        "image/webp" => MediaType.ImageWebP,
        "image/gif"  => MediaType.ImageGif,
        _            => MediaType.ImageJpeg
    };
}

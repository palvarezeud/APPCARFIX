using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using CarFix.Aplicacion.Comun;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarFix.Infraestructura.IA;

public interface IServicioLlamadaAnthropicJson
{
    Task<Resultado<JsonElement>> LlamarAsync(
        string instruccionSistema,
        string contenidoUsuario,
        Dictionary<string, JsonElement> esquemaJson,
        int maxTokens,
        CancellationToken ct = default);
}

public class ServicioLlamadaAnthropicJson : IServicioLlamadaAnthropicJson
{
    private readonly AnthropicClient _cliente;
    private readonly IConfiguration  _config;
    private readonly ILogger<ServicioLlamadaAnthropicJson> _logger;

    public ServicioLlamadaAnthropicJson(
        AnthropicClient cliente, IConfiguration config, ILogger<ServicioLlamadaAnthropicJson> logger)
    {
        _cliente = cliente;
        _config  = config;
        _logger  = logger;
    }

    public async Task<Resultado<JsonElement>> LlamarAsync(
        string instruccionSistema,
        string contenidoUsuario,
        Dictionary<string, JsonElement> esquemaJson,
        int maxTokens,
        CancellationToken ct = default)
    {
        try
        {
            var modelo = _config["Anthropic:Modelo"] ?? "claude-sonnet-5";

            var parametros = new MessageCreateParams
            {
                Model     = modelo,
                MaxTokens = maxTokens,
                System    = instruccionSistema,
                OutputConfig = new OutputConfig
                {
                    Effort = Effort.Low,
                    Format = new JsonOutputFormat { Schema = esquemaJson }
                },
                Messages =
                [
                    new()
                    {
                        Role = Role.User,
                        Content = new List<ContentBlockParam>
                        {
                            new TextBlockParam { Text = contenidoUsuario }
                        }
                    }
                ]
            };

            var respuesta = await _cliente.Messages.Create(parametros, cancellationToken: ct);

            if (respuesta.StopReason == "refusal")
            {
                _logger.LogWarning("Anthropic rechazo la solicitud.");
                return Resultado<JsonElement>.Fallo("La IA no pudo procesar la solicitud.");
            }

            var texto = respuesta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                _logger.LogWarning("Anthropic no devolvio texto en la respuesta.");
                return Resultado<JsonElement>.Fallo("No se pudo interpretar la solicitud.");
            }

            using var doc = JsonDocument.Parse(texto);
            return Resultado<JsonElement>.Exito(doc.RootElement.Clone());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar al servicio de Anthropic.");
            return Resultado<JsonElement>.Fallo("El servicio de IA no esta disponible en este momento.");
        }
    }
}

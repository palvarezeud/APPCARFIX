namespace CarFix.Dominio.Interfaces;

public interface IServicioEnvioCorreo
{
    Task<ResultadoEnvioCorreo> EnviarAsync(
        string   destinatario,
        string   asunto,
        string   cuerpoHtml,
        byte[]   adjuntoBytes,
        string   nombreAdjunto,
        CancellationToken ct = default);
}

public record ResultadoEnvioCorreo(bool EsExitoso, string? MensajeError)
{
    public static ResultadoEnvioCorreo Exito() => new(true, null);
    public static ResultadoEnvioCorreo Fallo(string mensajeError) => new(false, mensajeError);
}

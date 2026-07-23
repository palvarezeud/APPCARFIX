using System.Security.Cryptography;

namespace CarFix.Aplicacion.Comun;

public static class GeneradorTokenRefresco
{
    public const int DiasVigencia = 60;

    public static string Generar()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

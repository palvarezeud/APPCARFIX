namespace CarFix.Aplicacion.Comun;

public class Resultado<T>
{
    public bool    EsExitoso { get; }
    public T?      Valor     { get; }
    public string? Error     { get; }

    private Resultado(T valor)      { EsExitoso = true;  Valor = valor; }
    private Resultado(string error) { EsExitoso = false; Error = error; }

    public static Resultado<T> Exito(T valor)      => new(valor);
    public static Resultado<T> Fallo(string error) => new(error);
}

public class Resultado
{
    public bool    EsExitoso { get; }
    public string? Error     { get; }

    private Resultado(bool esExitoso, string? error) { EsExitoso = esExitoso; Error = error; }

    public static Resultado Exito()                => new(true, null);
    public static Resultado Fallo(string error)    => new(false, error);
}

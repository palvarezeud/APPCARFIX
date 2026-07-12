namespace CarFix.Dominio.Entidades;

public partial class MarcaModelo
{
    public int MarcaModeloId { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public int? Annio { get; set; }
}

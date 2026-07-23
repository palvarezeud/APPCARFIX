using System;

namespace CarFix.Dominio.Entidades;

public partial class TokenRefresco
{
    public int TokenRefrescoId { get; set; }

    public int UsuarioId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string? IdentificadorDispositivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Revocado { get; set; }

    public DateTime? FechaRevocado { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}

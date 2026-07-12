using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string? Email { get; set; }

    public bool Activo { get; set; }

    public int RolId { get; set; }

    public virtual Role Rol { get; set; } = null!;
}


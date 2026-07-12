using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena el catalogo de clientes
/// </summary>
public partial class Cliente
{
    /// <summary>
    /// Identificador unico del cliente
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Nombre completo del cliente o empresa
    /// </summary>
    public string NombreCliente { get; set; } = null!;

    /// <summary>
    /// Telefono Principal del cliente
    /// </summary>
    public string Telefono1 { get; set; } = null!;

    /// <summary>
    /// Telefono alternativo del cliente
    /// </summary>
    public string? Telefono2 { get; set; }

    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Direccion del Cliente
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Indica si el cliente es una empresa
    /// </summary>
    public bool EsEmpresa { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}


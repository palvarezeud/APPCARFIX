using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena el catalogo de vehículos
/// </summary>
public partial class Vehiculo
{
    /// <summary>
    /// Identificador único del vehiculo en el sistema
    /// </summary>
    public int VehiculoId { get; set; }

    /// <summary>
    /// Placa del vehiculo
    /// </summary>
    public string? Placa { get; set; }

    /// <summary>
    /// Marca del Vehículo 
    /// </summary>
    public string Marca { get; set; } = null!;

    /// <summary>
    /// Modelo del Vehículo
    /// </summary>
    public string? Modelo { get; set; }

    /// <summary>
    /// VIN del vehiculo 
    /// </summary>
    public string? Vin { get; set; }

    /// <summary>
    /// Define el año de fabricación del vehiculo
    /// </summary>
    public short? Annio { get; set; }

    /// <summary>
    /// Describe el tipo de motor y cilindrada
    /// </summary>
    public string? Motor { get; set; }

    /// <summary>
    /// Indica si el carro es automático o manual
    /// </summary>
    public bool EsAutomatico { get; set; }

    /// <summary>
    /// Tiene algun detalle o golpe en la carrocería al ingresar al taller
    /// </summary>
    public string DetallesCarroceria { get; set; } = null!;

    /// <summary>
    /// Cliente asociado al vehiculo
    /// </summary>
    public int ClienteId { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual ICollection<OrdenServicio> OrdenServicios { get; set; } = new List<OrdenServicio>();
}


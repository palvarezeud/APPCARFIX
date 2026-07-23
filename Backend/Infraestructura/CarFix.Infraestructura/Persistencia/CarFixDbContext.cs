using System;
using System.Collections.Generic;
using CarFix.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia;

public partial class CarFixDbContext : DbContext
{
    public CarFixDbContext(DbContextOptions<CarFixDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<EstadoFactura> EstadoFacturas { get; set; }

    public virtual DbSet<EstadoOrden> EstadoOrdens { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<HistoricoRespuesto> HistoricoRespuestos { get; set; }

    public virtual DbSet<MarcaModelo> MarcaModelos { get; set; }

    public virtual DbSet<OrdenServicio> OrdenServicios { get; set; }

    public virtual DbSet<Parametro> Parametros { get; set; }

    public virtual DbSet<Reparacion> Reparacions { get; set; }

    public virtual DbSet<Repuesto> Repuestos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Taller> Tallers { get; set; }

    public virtual DbSet<TipoReparacion> TipoReparacions { get; set; }

    public virtual DbSet<TokenRefresco> TokenRefrescos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes", "Catalogo", tb => tb.HasComment("Almacena el catalogo de clientes"));

            entity.Property(e => e.ClienteId)
                .HasComment("Identificador unico del cliente")
                .HasColumnName("ClienteID");
            entity.Property(e => e.Direccion)
                .IsUnicode(false)
                .HasComment("Direccion del Cliente");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasComment("Correo electrónico del cliente");
            entity.Property(e => e.EsEmpresa).HasComment("Indica si el cliente es una empresa");
            entity.Property(e => e.NombreCliente)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasComment("Nombre completo del cliente o empresa");
            entity.Property(e => e.Telefono1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Telefono Principal del cliente");
            entity.Property(e => e.Telefono2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Telefono alternativo del cliente");
        });

        modelBuilder.Entity<EstadoFactura>(entity =>
        {
            entity.ToTable("EstadoFactura", "Catalogo", tb => tb.HasComment("Almacena los estados de las facturas(Cotizacion, Pendiente, Cancelada)"));

            entity.Property(e => e.EstadoFacturaId)
                .HasComment("Identificador único del estado de la factura")
                .HasColumnName("EstadoFacturaID");
            entity.Property(e => e.Descipcion)
                .HasMaxLength(25)
                .IsFixedLength()
                .HasComment("Almacena la descripción del estado");
        });

        modelBuilder.Entity<EstadoOrden>(entity =>
        {
            entity.ToTable("EstadoOrden", "Catalogo", tb => tb.HasComment("Almacena los estados de las ordenes de reparación(Cotización, Recibido, En reparacion, Finalizado, Entregado)"));

            entity.Property(e => e.EstadoOrdenId)
                .HasComment("Identificador del estado de la orden")
                .HasColumnName("EstadoOrdenID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasComment("Descripción del estado de una orden");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.ToTable("Facturas", "Sistema", tb => tb.HasComment("Almacena las facturas"));

            entity.Property(e => e.FacturaId)
                .HasComment("Identificador único de la factura")
                .HasColumnName("FacturaID");
            entity.Property(e => e.Adelanto)
                .HasComment("Si el cliente dio un adelanto de dinero ")
                .HasColumnType("money");
            entity.Property(e => e.DescripcionGeneral)
                .IsUnicode(false)
                .HasComment("Descripción general de las reparaciones");
            entity.Property(e => e.Descuento)
                .HasComment("Si aplica algún descuento")
                .HasColumnType("money");
            entity.Property(e => e.EstadoFacturaId)
                .HasComment("Estado de la factura")
                .HasColumnName("EstadoFacturaID");
            entity.Property(e => e.Fecha)
                .HasComment("Fecha de la factura")
                .HasColumnType("datetime");
            entity.Property(e => e.ImpuestoVentas)
                .HasComment("Almacena el impuesto de ventas de la factura")
                .HasColumnType("money");
            entity.Property(e => e.NombreCliente)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Nombre del cliente");
            entity.Property(e => e.Pendiente)
                .HasComment("Saldo pendiente de cobro (Total menos Adelanto)")
                .HasColumnType("money");
            entity.Property(e => e.SubTotal)
                .HasComment("Repuestos + reparaciones menos el descuento")
                .HasColumnType("money");
            entity.Property(e => e.Total)
                .HasComment("Total de la factura ")
                .HasColumnType("money");
            entity.Property(e => e.TotalReparaciones)
                .HasComment("Total de las reparaciones realizadas")
                .HasColumnType("money");
            entity.Property(e => e.TotalRepuestos)
                .HasComment("Total de los repuestos comprados")
                .HasColumnType("money");
            entity.Property(e => e.VehiculoId)
                .HasComment("Identificador del vehiculo asociado")
                .HasColumnName("VehiculoID");

            entity.HasOne(d => d.EstadoFactura).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.EstadoFacturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturas_EstadoFactura");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.VehiculoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturas_Vehiculos");
        });

        modelBuilder.Entity<HistoricoRespuesto>(entity =>
        {
            entity.HasKey(e => e.RespuestoHistoricoId).HasName("PK_TipoRespuesto");

            entity.ToTable("HistoricoRespuesto", "Catalogo", tb => tb.HasComment("Almacena un historico con repuestos comprados en facturas anteriores"));

            entity.Property(e => e.RespuestoHistoricoId)
                .HasComment("Identificador único de la tabla")
                .HasColumnName("RespuestoHistoricoID");
            entity.Property(e => e.Annio).HasComment("Año del vehículo");
            entity.Property(e => e.FechaCompra)
                .HasComment("Fecha que se compro")
                .HasColumnType("datetime");
            entity.Property(e => e.Marca)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Marca del Vehículo");
            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Modelo del vehículo");
            entity.Property(e => e.Precio)
                .HasComment("Precio del repuesto")
                .HasColumnType("money");
            entity.Property(e => e.Repuestera)
                .IsUnicode(false)
                .HasComment("Lugar donde se compró");
            entity.Property(e => e.RepuestoDecripcion)
                .IsUnicode(false)
                .HasComment("Descripción del repuesto");
        });

        modelBuilder.Entity<MarcaModelo>(entity =>
        {
            entity.HasKey(e => e.MarcaModeloId).HasName("PK_MarcaModelo");

            entity.ToTable("MarcaModelo", "Catalogo", tb => tb.HasComment("Catalogo de combinaciones Marca/Modelo/Annio para autocompletar Vehiculos"));

            entity.Property(e => e.MarcaModeloId)
                .HasComment("Identificador unico del registro")
                .HasColumnName("MarcaModeloID");
            entity.Property(e => e.Marca)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Marca del vehiculo");
            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Modelo del vehiculo");
            entity.Property(e => e.Annio)
                .HasComment("Annio de fabricacion del vehiculo");
        });

        modelBuilder.Entity<OrdenServicio>(entity =>
        {
            entity.HasKey(e => e.OrdenServicioId).HasName("PK_OrdenReparacion");

            entity.ToTable("OrdenServicio", "Sistema", tb => tb.HasComment("Almacena las ordenes de reparación de vehiculos"));

            entity.HasIndex(e => e.FacturaId, "UQ_OrdenReparacion_FacturaIAsociada").IsUnique();

            entity.Property(e => e.OrdenServicioId)
                .HasComment("Identificador único de orden de reparación")
                .HasColumnName("OrdenServicioID");
            entity.Property(e => e.EsGarantia).HasComment("Especifica si la orden es para cubrir una garantía de un trabajo anterior");
            entity.Property(e => e.EstadoOrdenId)
                .HasComment("Estado de la orden(0=Cotización,1=Recibido, 2=En revisión, 3= Finalizado, 4= Entregado")
                .HasColumnName("EstadoOrdenID");
            entity.Property(e => e.FacturaId)
                .HasComment("Asocia una factura a la orden de servicio")
                .HasColumnName("FacturaID");
            entity.Property(e => e.FechaIngreso)
                .HasComment("Fecha de ingreso del vehículo al taller")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaSalida)
                .HasComment("Fecha de salida del vehículo del taller")
                .HasColumnType("datetime");
            entity.Property(e => e.ProblemaGeneral)
                .IsUnicode(false)
                .HasComment("Describe el problema o revisión que se le debe reparar al vehículo");
            entity.Property(e => e.VehiculoId)
                .HasComment("Vehículo asociado a la orden de reparación")
                .HasColumnName("VehiculoID");

            entity.HasOne(d => d.EstadoOrden).WithMany(p => p.OrdenServicios)
                .HasForeignKey(d => d.EstadoOrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenReparacion_EstadoOrden");

            entity.HasOne(d => d.Factura).WithOne(p => p.OrdenServicio)
                .HasForeignKey<OrdenServicio>(d => d.FacturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenReparacion_Facturas");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.OrdenServicios)
                .HasForeignKey(d => d.VehiculoId)
                .HasConstraintName("FK_OrdenReparacion_Vehiculos1");
        });

        modelBuilder.Entity<Parametro>(entity =>
        {
            entity.ToTable("Parametros", "Catalogo", tb => tb.HasComment("Catalogo generico clave/valor para parametros de configuracion del sistema"));

            entity.Property(e => e.ParametroId)
                .HasComment("Identificador unico del parametro")
                .HasColumnName("ParametroID");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Nombre/clave del parametro");
            entity.Property(e => e.Valor)
                .IsUnicode(false)
                .HasComment("Valor del parametro (texto libre)");
        });

        modelBuilder.Entity<Reparacion>(entity =>
        {
            entity.HasKey(e => e.ReparacionId).HasName("PK_Reparaciones");

            entity.ToTable("Reparacion", "Sistema", tb => tb.HasComment("Almacena las reparaciones hechas a un vehículo"));

            entity.Property(e => e.ReparacionId)
                .HasComment("Identificador unico de la reparación")
                .HasColumnName("ReparacionID");
            entity.Property(e => e.Listo)
                .HasDefaultValue(false);
            entity.Property(e => e.Costo)
                .HasComment("Costo de la reparación")
                .HasColumnType("money");
            entity.Property(e => e.DescripcionReparacion)
                .IsUnicode(false)
                .HasComment("Descripción de la reparación hecha");
            entity.Property(e => e.FacturaId)
                .HasComment("Identificador de la orden asociada a la reparación")
                .HasColumnName("FacturaID");
            entity.Property(e => e.DuracionAproximadaHoras)
                .HasColumnName("DuracionAproximadaHoras")
                .HasDefaultValue(1);

            entity.HasOne(d => d.Factura).WithMany(p => p.Reparaciones)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK_Reparacion_Facturas");
        });

        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.HasKey(e => e.RepuestoId).HasName("PK_Repuestos");

            entity.ToTable("Repuesto", "Sistema", tb => tb.HasComment("Almacena los repuestos que se cotizan o aplican a la reparación"));

            entity.Property(e => e.RepuestoId)
                .HasComment("Identificador único del respuesto")
                .HasColumnName("RepuestoID");
            entity.Property(e => e.Costo)
                .HasComment("Costo del repuesto")
                .HasColumnType("money");
            entity.Property(e => e.Factura)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasComment("Número de factura asociada");
            entity.Property(e => e.FacturaId)
                .HasComment("Identificador de la orden asociada")
                .HasColumnName("FacturaID");
            entity.Property(e => e.Fecha)
                .HasComment("Fecha de compra del repuesto")
                .HasColumnType("datetime");
            entity.Property(e => e.Incluido)
                .HasDefaultValue(false);
            entity.Property(e => e.NombreRepuesto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Descripción del respuesto");
            entity.Property(e => e.Repuestera)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Nombre del lugar donde se compró el repuesto");

            entity.HasOne(d => d.FacturaNavigation).WithMany(p => p.Repuestos)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK_Repuesto_Facturas");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId);

            entity.ToTable("Roles", "Catalogo");

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Taller>(entity =>
        {
            entity.ToTable("Taller", "Catalogo", tb => tb.HasComment("Datos del taller"));

            entity.Property(e => e.TallerId)
                .HasComment("Identificador unico")
                .HasColumnName("TallerID");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Correo electronico del taller");
            entity.Property(e => e.Nombre)
                .IsUnicode(false)
                .HasComment("Nombre del taller");
            entity.Property(e => e.Telefonos)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Telefonos del taller");
            entity.Property(e => e.UbicacionDescripcion)
                .IsUnicode(false)
                .HasComment("Descripción de la ubicación");
            entity.Property(e => e.UbicacionGps)
                .HasColumnName("UbicaciónGPS")
                .HasColumnType("geography")
                .HasComment("Coordenadas geograficas del taller");
        });

        modelBuilder.Entity<TipoReparacion>(entity =>
        {
            entity.HasKey(e => e.TipoReparacionId).HasName("PK_TipoReparacionID");

            entity.ToTable("TipoReparacion", "Catalogo", tb => tb.HasComment("Almacena el catalogo con los costos vigentes de reparaciones comunes"));

            entity.Property(e => e.TipoReparacionId)
                .ValueGeneratedNever()
                .HasComment("Identificador único de la tabla")
                .HasColumnName("TipoReparacionID");
            entity.Property(e => e.CostoBase)
                .HasComment("Muestra un costo base ")
                .HasColumnType("money");
            entity.Property(e => e.DescripcionReparacion)
                .IsUnicode(false)
                .HasComment("Describe la reparación a realizar");
            entity.Property(e => e.DuracionAproximadaHoras).HasComment("Detalla cuánto aproximadamente se tarda en horas");
        });

        modelBuilder.Entity<TokenRefresco>(entity =>
        {
            entity.ToTable("TokenRefresco", "Sistema", tb => tb.HasComment("Refresh tokens para re-login silencioso (desbloqueo biometrico local)"));

            entity.HasIndex(e => e.TokenHash, "UQ_TokenRefresco_TokenHash").IsUnique();
            entity.HasIndex(e => e.UsuarioId, "IX_TokenRefresco_UsuarioID");

            entity.Property(e => e.TokenRefrescoId)
                .HasComment("Identificador unico del token de refresco")
                .HasColumnName("TokenRefrescoID");
            entity.Property(e => e.UsuarioId)
                .HasComment("Usuario propietario del token")
                .HasColumnName("UsuarioID");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasComment("Hash SHA-256 del token de refresco (nunca se guarda en texto plano)");
            entity.Property(e => e.IdentificadorDispositivo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Etiqueta opcional del dispositivo/cliente");
            entity.Property(e => e.FechaCreacion)
                .HasComment("Fecha de emision del token")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FechaExpiracion)
                .HasComment("Fecha de expiracion del token")
                .HasColumnType("datetime");
            entity.Property(e => e.Revocado)
                .HasComment("Indica si el token ya fue revocado o rotado")
                .HasDefaultValue(false);
            entity.Property(e => e.FechaRevocado)
                .HasComment("Fecha en que el token fue revocado")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.TokenRefrescos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TokenRefresco_Usuarios");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios", "Catalogo");

            entity.HasIndex(e => e.NombreUsuario, "UQ_Usuarios_NombreUsr").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuarios_Activo");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.ToTable("Vehiculos", "Catalogo", tb => tb.HasComment("Almacena el catalogo de vehículos"));

            entity.Property(e => e.VehiculoId)
                .HasComment("Identificador único del vehiculo en el sistema")
                .HasColumnName("VehiculoID");
            entity.Property(e => e.Annio).HasComment("Define el año de fabricación del vehiculo");
            entity.Property(e => e.ClienteId)
                .HasComment("Cliente asociado al vehiculo")
                .HasColumnName("ClienteID");
            entity.Property(e => e.DetallesCarroceria)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Tiene algun detalle o golpe en la carrocería al ingresar al taller");
            entity.Property(e => e.EsAutomatico).HasComment("Indica si el carro es automático o manual");
            entity.Property(e => e.Marca)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Marca del Vehículo ");
            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("Modelo del Vehículo");
            entity.Property(e => e.Motor)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Describe el tipo de motor y cilindrada");
            entity.Property(e => e.Placa)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Placa del vehiculo");
            entity.Property(e => e.Vin)
                .IsUnicode(false)
                .HasComment("VIN del vehiculo ")
                .HasColumnName("VIN");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK_Vehiculos_Clientes");
        });
        modelBuilder.HasSequence("Sequence-Clientes");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

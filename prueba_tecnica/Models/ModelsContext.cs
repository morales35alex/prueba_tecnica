using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace prueba_tecnica.Models;

public partial class ModelsContext : DbContext
{
    public ModelsContext()
    {
    }

    public ModelsContext(DbContextOptions<ModelsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cuentum> Cuenta { get; set; }

    public virtual DbSet<Historial> Historials { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<TipoMovimiento> TipoMovimientos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=prueba_tecnica;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("cliente");

            entity.HasIndex(e => e.Correo, "correo").IsUnique();

            entity.HasIndex(e => e.Dni, "dni").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .HasColumnName("correo");
            entity.Property(e => e.Dni)
                .HasMaxLength(50)
                .HasColumnName("dni");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Cuentum>(entity =>
        {
            entity.HasKey(e => e.IdCuenta).HasName("PRIMARY");

            entity.ToTable("cuenta");

            entity.HasIndex(e => e.IdCliente, "fk_cuenta_cliente");

            entity.HasIndex(e => e.NumeroCuenta, "numero_cuenta").IsUnique();

            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.NumeroCuenta).HasColumnName("numero_cuenta");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("fk_cuenta_cliente");
        });

        modelBuilder.Entity<Historial>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.ToTable("historial");

            entity.HasIndex(e => e.IdMovimiento, "fk_histoarial_movimiento");

            entity.Property(e => e.IdHistorial).HasColumnName("id_historial");
            entity.Property(e => e.Detalle)
                .HasMaxLength(255)
                .HasColumnName("detalle");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaMovimiento)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");

            entity.HasOne(d => d.IdMovimientoNavigation).WithMany(p => p.Historials)
                .HasForeignKey(d => d.IdMovimiento)
                .HasConstraintName("fk_histoarial_movimiento");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PRIMARY");

            entity.ToTable("movimiento");

            entity.HasIndex(e => e.IdCuenta, "fk_movimiento_cuenta");

            entity.HasIndex(e => e.IdTipoMovimiento, "fk_tipo_movimiento");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Deposito)
                .HasPrecision(50, 2)
                .HasColumnName("deposito");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaMovimiento)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.IdTipoMovimiento).HasColumnName("id_tipo_movimiento");
            entity.Property(e => e.Retiro)
                .HasPrecision(50, 2)
                .HasColumnName("retiro");
            entity.Property(e => e.Saldo)
                .HasPrecision(50, 2)
                .HasColumnName("saldo");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdCuenta)
                .HasConstraintName("fk_movimiento_cuenta");

            entity.HasOne(d => d.IdTipoMovimientoNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdTipoMovimiento)
                .HasConstraintName("fk_tipo_movimiento");
        });

        modelBuilder.Entity<TipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimiento).HasName("PRIMARY");

            entity.ToTable("tipo_movimiento");

            entity.Property(e => e.IdTipoMovimiento).HasColumnName("id_tipo_movimiento");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

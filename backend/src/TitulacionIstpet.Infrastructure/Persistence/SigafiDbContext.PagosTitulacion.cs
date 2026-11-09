using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence;

public partial class SigafiDbContext
{
    public DbSet<TitulConfiguracionPagoModalidad> ConfiguracionesPagosModalidad => Set<TitulConfiguracionPagoModalidad>();
    public DbSet<TitulPago> PagosTitulacion => Set<TitulPago>();
    public DbSet<TitulPagoHistorial> PagosTitulacionHistorial => Set<TitulPagoHistorial>();

    internal static void ConfigurarPagosTitulacion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TitulConfiguracionPagoModalidad>(entity =>
        {
            entity.ToTable("titul_configuracion_pagos_modalidad");
            entity.HasKey(e => e.IdConfiguracionPago);

            entity.Property(e => e.IdConfiguracionPago).HasColumnName("idConfiguracionPago");
            entity.Property(e => e.IdModalidadTitulacion).HasColumnName("idModalidadTitulacion");
            entity.Property(e => e.IdPeriodo).HasColumnName("idPeriodo").HasMaxLength(20);
            entity.Property(e => e.ValorBase).HasColumnName("valorBase").HasPrecision(10, 2);
            entity.Property(e => e.MaximoInversion).HasColumnName("maximoInversion").HasPrecision(10, 2);
            entity.Property(e => e.TotalTitulacion).HasColumnName("totalTitulacion").HasPrecision(10, 2);
            entity.Property(e => e.IdEspecie).HasColumnName("idEspecie");
            entity.Property(e => e.Activo).HasColumnName("activo").HasColumnType("tinyint(1)");
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaRegistro").HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnName("fechaModificacion").HasColumnType("datetime");

            entity.HasOne(e => e.IdModalidadTitulacionNavigation)
                .WithMany()
                .HasForeignKey(e => e.IdModalidadTitulacion)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IdEspecieNavigation)
                .WithMany()
                .HasForeignKey(e => e.IdEspecie)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TitulPago>(entity =>
        {
            entity.ToTable("titul_pagos");
            entity.HasKey(e => e.IdPagoTitulacion);

            entity.Property(e => e.IdPagoTitulacion).HasColumnName("idPagoTitulacion");
            entity.Property(e => e.IdPostulacionAlumno).HasColumnName("idPostulacionAlumno");
            entity.Property(e => e.IdMatricula).HasColumnName("idMatricula");
            entity.Property(e => e.IdConfiguracionPago).HasColumnName("idConfiguracionPago");
            entity.Property(e => e.TipoRubro).HasColumnName("tipoRubro").HasMaxLength(20).IsRequired();
            entity.Property(e => e.NumeroFactura).HasColumnName("numeroFactura").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Valor).HasColumnName("valor").HasPrecision(10, 2);
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaRegistro").HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioRegistro).HasColumnName("idUsuarioRegistro");
            entity.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(500);
            entity.Property(e => e.MotivoRechazo).HasColumnName("motivoRechazo").HasMaxLength(500);
            entity.Property(e => e.FechaRevision).HasColumnName("fechaRevision").HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioRevision).HasColumnName("idUsuarioRevision");
            entity.Property(e => e.IdPagoSigafi).HasColumnName("idPagoSigafi");
            entity.Property(e => e.IdCredito).HasColumnName("idCredito");
            entity.Property(e => e.ComprobanteArchivo).HasColumnName("comprobanteArchivo").HasMaxLength(255);

            entity.HasOne(e => e.PostulacionAlumno)
                .WithMany()
                .HasForeignKey(e => e.IdPostulacionAlumno)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Matricula)
                .WithMany()
                .HasForeignKey(e => e.IdMatricula)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConfiguracionPago)
                .WithMany()
                .HasForeignKey(e => e.IdConfiguracionPago)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UsuarioRegistro)
                .WithMany()
                .HasForeignKey(e => e.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UsuarioRevision)
                .WithMany()
                .HasForeignKey(e => e.IdUsuarioRevision)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PagoSigafi)
                .WithMany()
                .HasForeignKey(e => e.IdPagoSigafi)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Historial)
                .WithOne(h => h.Pago)
                .HasForeignKey(h => h.IdPagoTitulacion)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TitulPagoHistorial>(entity =>
        {
            entity.ToTable("titul_pagos_historial");
            entity.HasKey(e => e.IdPagoHistorial);

            entity.Property(e => e.IdPagoHistorial).HasColumnName("idPagoHistorial");
            entity.Property(e => e.IdPagoTitulacion).HasColumnName("idPagoTitulacion");
            entity.Property(e => e.EstadoAnterior).HasColumnName("estadoAnterior").HasMaxLength(20);
            entity.Property(e => e.EstadoNuevo).HasColumnName("estadoNuevo").HasMaxLength(20).IsRequired();
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(500);
            entity.Property(e => e.FechaCambio).HasColumnName("fechaCambio").HasColumnType("datetime");

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

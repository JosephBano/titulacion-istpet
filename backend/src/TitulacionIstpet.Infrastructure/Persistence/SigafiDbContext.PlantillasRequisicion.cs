using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence;

public partial class SigafiDbContext
{
    public DbSet<TitulPlantillaDocumento> PlantillasDocumentos => Set<TitulPlantillaDocumento>();

    internal static void ConfigurarPlantillasDocumentos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TitulPlantillaDocumento>(entity =>
        {
            entity.ToTable("titul_plantillas_documentos");
            entity.HasKey(e => e.IdPlantillaDocumento);
            entity.Property(e => e.IdPlantillaDocumento).HasColumnName("idPlantillaDocumento");
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Codigo).IsUnique().HasDatabaseName("ux_titul_plantillas_documentos_codigo");
            entity.Property(e => e.DefinicionJson).HasColumnName("definicionJson").HasColumnType("mediumtext").IsRequired();
            entity.Property(e => e.Revision).HasColumnName("revision").HasDefaultValue(1);
            entity.Property(e => e.ActualizadoPor).HasColumnName("actualizadoPor");
            entity.Property(e => e.FechaCreacion).HasColumnName("fechaCreacion").HasColumnType("datetime");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fechaActualizacion").HasColumnType("datetime");

            entity.HasOne(e => e.ActualizadoPorNavigation)
                .WithMany()
                .HasForeignKey(e => e.ActualizadoPor)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

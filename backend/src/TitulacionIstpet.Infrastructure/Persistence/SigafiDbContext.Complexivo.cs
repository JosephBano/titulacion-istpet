using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence;

public partial class SigafiDbContext
{
    public DbSet<TitulModuloComplexivo> ModulosComplexivo => Set<TitulModuloComplexivo>();
    public DbSet<TitulMallaComplexivo> MallasComplexivo => Set<TitulMallaComplexivo>();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "La firma parcial es generada por EF Core Power Tools y debe ser de instancia.")]
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TitulModuloComplexivo>(entity =>
        {
            entity.ToTable("titul_modulos_complexivo").HasCharSet("latin1");
            entity.HasKey(e => e.IdModulosComplexivo);
            entity.Property(e => e.IdModulosComplexivo).HasColumnName("idModulosComplexivo");
            entity.Property(e => e.IdCarrera).HasColumnName("idCarrera");
            entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(500);
            entity.Property(e => e.EsActivo).HasColumnName("esActivo").HasColumnType("tinyint(4)");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime");
            entity.Property(e => e.FechaDesactivacion).HasColumnName("fecha_desactivacion").HasColumnType("datetime");
            entity.Property(e => e.TotalHoras).HasColumnName("total_horas");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.Modalidad).HasColumnName("modalidad").HasMaxLength(500);
            entity.HasOne<Carreras>().WithMany().HasForeignKey(e => e.IdCarrera).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Temas).WithOne().HasForeignKey(e => e.IdModulosComplexivo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TitulTemaComplexivo>(entity =>
        {
            entity.ToTable("titul_temas").HasCharSet("latin1");
            entity.HasKey(e => e.IdTemas);
            entity.Property(e => e.IdTemas).HasColumnName("idTemas");
            entity.Property(e => e.IdModulosComplexivo).HasColumnName("idModulosComplexivo");
            entity.Property(e => e.IdAsignatura).HasColumnName("idAsignatura");
            entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(500);
            entity.Property(e => e.HorasPorTema).HasColumnName("horas_por_tema");
            entity.HasOne(e => e.Asignatura).WithMany().HasForeignKey(e => e.IdAsignatura).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TitulMallaComplexivo>(entity =>
        {
            entity.ToTable("titul_malla_complexivo").HasCharSet("latin1");
            entity.HasKey(e => e.IdMallaComplexivo);
            entity.Property(e => e.IdMallaComplexivo).HasColumnName("idMallaComplexivo");
            entity.Property(e => e.IdCohorteCarrera).HasColumnName("idCohorteCarrera");
            entity.Property(e => e.IdModulosComplexivo).HasColumnName("idModulosComplexivo");
            entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(150);
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo").HasColumnType("tinyint(4)");
            entity.HasOne<TitulCohortesCarreras>().WithMany().HasForeignKey(e => e.IdCohorteCarrera).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Modulo).WithMany().HasForeignKey(e => e.IdModulosComplexivo).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Docentes).WithOne().HasForeignKey(e => e.IdMallaComplexivo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TitulMallaDocenteComplexivo>(entity =>
        {
            entity.ToTable("titul_malla_docente_complexivo").HasCharSet("latin1");
            entity.HasKey(e => e.IdMallaDocenteComplexivo);
            entity.Property(e => e.IdMallaDocenteComplexivo).HasColumnName("idMallaDocenteComplexivo");
            entity.Property(e => e.IdMallaComplexivo).HasColumnName("idMallaComplexivo");
            entity.Property(e => e.IdProfesor).HasColumnName("idProfesor").HasMaxLength(14);
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion");
            entity.Property(e => e.CodigoAsignacion).HasColumnName("codigo_asignacion").HasMaxLength(10);
            entity.Property(e => e.EsActivo).HasColumnName("esActivo").HasColumnType("tinyint(4)");
            entity.HasOne(e => e.Profesor).WithMany().HasForeignKey(e => e.IdProfesor).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Horarios).WithOne().HasForeignKey(e => e.IdModulosProfesores).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TitulModuloHorarioComplexivo>(entity =>
        {
            entity.ToTable("titul_modulos_horarios").HasCharSet("latin1");
            entity.HasKey(e => e.IdModulosHorarios);
            entity.Property(e => e.IdModulosHorarios).HasColumnName("idModulosHorarios");
            entity.Property(e => e.IdModulosProfesores).HasColumnName("idModulosProfesores");
            entity.Property(e => e.Dias).HasColumnName("dias").HasMaxLength(250);
            entity.Property(e => e.FranjaHoraria).HasColumnName("franja_horarioa").HasMaxLength(150);
        });

        ConfigurarPlantillasDocumentos(modelBuilder);
    }
}

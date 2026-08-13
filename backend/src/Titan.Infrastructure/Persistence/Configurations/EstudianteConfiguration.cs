using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Titan.Domain.Entities;

namespace Titan.Infrastructure.Persistence.Configurations;

public class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("estudiantes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Cedula).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Nombres).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();

        builder.Property(e => e.CorreoInstitucional).HasMaxLength(180).IsRequired();

        builder.Property(e => e.Estado).HasConversion<int>();

        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(e => e.Cedula).IsUnique();
        builder.HasIndex(e => e.CorreoInstitucional).IsUnique();
    }
}

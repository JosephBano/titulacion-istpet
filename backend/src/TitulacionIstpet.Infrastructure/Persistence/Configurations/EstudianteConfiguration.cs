using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Configurations;

public class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("estudiantes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Cedula).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Nombres).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();

        // MySQL 5.7 con utf8mb4 limita las claves de indice a 191 caracteres (767 bytes / 4).
        // Mantener <= 191 cualquier columna que entre en un indice unico.
        builder.Property(e => e.CorreoInstitucional).HasMaxLength(180).IsRequired();

        builder.Property(e => e.Estado).HasConversion<int>();

        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(e => e.Cedula).IsUnique();
        builder.HasIndex(e => e.CorreoInstitucional).IsUnique();
    }
}

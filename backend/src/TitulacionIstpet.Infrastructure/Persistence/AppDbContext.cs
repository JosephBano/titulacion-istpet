using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Domain.Common;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IUsuarioActual _usuarioActual;

    public AppDbContext(DbContextOptions<AppDbContext> options, IUsuarioActual usuarioActual)
        : base(options) => _usuarioActual = usuarioActual;

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SellarAuditoria();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SellarAuditoria()
    {
        var ahora = DateTime.UtcNow;
        var usuario = _usuarioActual.UserId;

        foreach (var entrada in ChangeTracker.Entries<EntidadBase>())
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Entity.CreadoEn = ahora;
                entrada.Entity.CreadoPor = usuario;
            }
            else if (entrada.State == EntityState.Modified)
            {
                entrada.Entity.ModificadoEn = ahora;
                entrada.Entity.ModificadoPor = usuario;
            }
        }
    }
}

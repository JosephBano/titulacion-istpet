using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioPlantillaRequisicion(SigafiDbContext context) : IRepositorioPlantillaRequisicion
{
    private readonly SigafiDbContext _context = context;

    public async Task<TitulPlantillaDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        return await _context.PlantillasDocumentos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo, ct);
    }

    public async Task<TitulPlantillaDocumento> GuardarOActualizarAsync(
        string codigo,
        string definicionJson,
        int revisionEsperada,
        int? usuarioId,
        CancellationToken ct = default)
    {
        var plantilla = await _context.PlantillasDocumentos
            .FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

        if (plantilla == null)
        {
            if (revisionEsperada != 0)
            {
                throw new DominioException(
                    $"La plantilla '{codigo}' aún no existe en el sistema. La revisión esperada debe ser 0.");
            }

            var nueva = new TitulPlantillaDocumento
            {
                Codigo = codigo,
                DefinicionJson = definicionJson,
                Revision = 1,
                ActualizadoPor = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.PlantillasDocumentos.Add(nueva);

            try
            {
                await _context.SaveChangesAsync(ct);
                return nueva;
            }
            catch (DbUpdateException ex)
            {
                throw new DominioException(
                    "Conflicto de concurrencia: la plantilla fue creada simultáneamente por otro usuario. Recargue la plantilla.",
                    ex);
            }
        }

        if (plantilla.Revision != revisionEsperada)
        {
            throw new DominioException(
                $"Conflicto de concurrencia: la plantilla fue modificada por otro usuario (revisión actual: {plantilla.Revision}, revisión esperada: {revisionEsperada}). Recargue la versión vigente.");
        }

        plantilla.DefinicionJson = definicionJson;
        plantilla.Revision += 1;
        plantilla.ActualizadoPor = usuarioId;
        plantilla.FechaActualizacion = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(ct);
            return plantilla;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new DominioException(
                "Conflicto de concurrencia al actualizar la plantilla. Recargue la versión vigente.",
                ex);
        }
    }
}
